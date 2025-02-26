namespace FSharpy

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open FSharpy.TaskSeqBuilders

[<AutoOpen>]
module ExtraTaskSeqOperators =
    /// A TaskSeq workflow for IAsyncEnumerable<'T> types.
    let taskSeq = TaskSeqBuilder()

[<Struct>]
type Action<'T, 'U, 'TaskU when 'TaskU :> Task<'U>> =
    | CountableAction of countable_action: (int -> 'T -> 'U)
    | SimpleAction of simple_action: ('T -> 'U)
    | AsyncCountableAction of async_countable_action: (int -> 'T -> 'TaskU)
    | AsyncSimpleAction of async_simple_action: ('T -> 'TaskU)

[<Struct>]
type FolderAction<'T, 'State, 'TaskState when 'TaskState :> Task<'State>> =
    | FolderAction of state_action: ('State -> 'T -> 'State)
    | AsyncFolderAction of async_state_action: ('State -> 'T -> 'TaskState)

[<Struct>]
type ChooserAction<'T, 'U, 'TaskOption when 'TaskOption :> Task<'U option>> =
    | TryPick of try_pick: ('T -> 'U option)
    | TryPickAsync of async_try_pick: ('T -> 'TaskOption)

[<Struct>]
type FilterAction<'T, 'U, 'TaskBool when 'TaskBool :> Task<bool>> =
    | TryFilter of try_filter: ('T -> bool)
    | TryFilterAsync of async_try_filter: ('T -> 'TaskBool)

module internal TaskSeqInternal =
    let inline raiseEmptySeq () =
        ArgumentException("The asynchronous input sequence was empty.", "source")
        |> raise

    let inline raiseInsufficient () =
        ArgumentException("The asynchronous input sequence was has an insufficient number of elements.", "source")
        |> raise

    let inline raiseNotFound () =
        KeyNotFoundException("The predicate function or index did not satisfy any item in the async sequence.")
        |> raise

    let isEmpty (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())
        let! step = e.MoveNextAsync()
        return not step
    }

    let tryExactlyOne (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())

        match! e.MoveNextAsync() with
        | true ->
            // grab first item and test if there's a second item
            let current = e.Current

            match! e.MoveNextAsync() with
            | true -> return None // 2 or more items
            | false -> return Some current // exactly one

        | false ->
            // zero items
            return None
    }

    let iter action (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())
        let mutable go = true
        let! step = e.MoveNextAsync()
        go <- step

        // this ensures that the inner loop is optimized for the closure
        // though perhaps we need to split into individual functions after all to use
        // InlineIfLambda?
        match action with
        | CountableAction action ->
            let mutable i = 0

            while go do
                do action i e.Current
                let! step = e.MoveNextAsync()
                i <- i + 1
                go <- step

        | SimpleAction action ->
            while go do
                do action e.Current
                let! step = e.MoveNextAsync()
                go <- step

        | AsyncCountableAction action ->
            let mutable i = 0

            while go do
                do! action i e.Current
                let! step = e.MoveNextAsync()
                i <- i + 1
                go <- step

        | AsyncSimpleAction action ->
            while go do
                do! action e.Current
                let! step = e.MoveNextAsync()
                go <- step
    }

    let fold folder initial (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())
        let mutable go = true
        let mutable result = initial
        let! step = e.MoveNextAsync()
        go <- step

        match folder with
        | FolderAction folder ->
            while go do
                result <- folder result e.Current
                let! step = e.MoveNextAsync()
                go <- step

        | AsyncFolderAction folder ->
            while go do
                let! tempResult = folder result e.Current
                result <- tempResult
                let! step = e.MoveNextAsync()
                go <- step

        return result
    }

    let toResizeArrayAsync source = task {
        let res = ResizeArray()
        do! source |> iter (SimpleAction(fun item -> res.Add item))
        return res
    }

    let toResizeArrayAndMapAsync mapper source = (toResizeArrayAsync >> Task.map mapper) source

    let map mapper (taskSequence: taskSeq<_>) =
        match mapper with
        | CountableAction mapper -> taskSeq {
            let mutable i = 0

            for c in taskSequence do
                yield mapper i c
                i <- i + 1
          }

        | SimpleAction mapper -> taskSeq {
            for c in taskSequence do
                yield mapper c
          }

        | AsyncCountableAction mapper -> taskSeq {
            let mutable i = 0

            for c in taskSequence do
                let! result = mapper i c
                yield result
                i <- i + 1
          }

        | AsyncSimpleAction mapper -> taskSeq {
            for c in taskSequence do
                let! result = mapper c
                yield result
          }

    let zip (source1: taskSeq<_>) (source2: taskSeq<_>) = taskSeq {
        let inline validate step1 step2 =
            if step1 <> step2 then
                if step1 then
                    invalidArg "taskSequence1" "The task sequences have different lengths."

                if step2 then
                    invalidArg "taskSequence2" "The task sequences have different lengths."


        let e1 = source1.GetAsyncEnumerator(CancellationToken())
        let e2 = source2.GetAsyncEnumerator(CancellationToken())
        let mutable go = true
        let! step1 = e1.MoveNextAsync()
        let! step2 = e2.MoveNextAsync()
        go <- step1 && step2
        validate step1 step2

        while go do
            yield e1.Current, e2.Current
            let! step1 = e1.MoveNextAsync()
            let! step2 = e2.MoveNextAsync()
            validate step1 step2
            go <- step1 && step2
    }

    let collect (binder: _ -> #IAsyncEnumerable<_>) (source: taskSeq<_>) = taskSeq {
        for c in source do
            yield! binder c :> IAsyncEnumerable<_>
    }

    let collectSeq (binder: _ -> #seq<_>) (source: taskSeq<_>) = taskSeq {
        for c in source do
            yield! binder c :> seq<_>
    }

    let collectAsync (binder: _ -> #Task<#IAsyncEnumerable<_>>) (source: taskSeq<_>) = taskSeq {
        for c in source do
            let! result = binder c
            yield! result :> IAsyncEnumerable<_>
    }

    let collectSeqAsync (binder: _ -> #Task<#seq<_>>) (source: taskSeq<_>) = taskSeq {
        for c in source do
            let! result = binder c
            yield! result :> seq<_>
    }

    let tryLast (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())
        let mutable go = true
        let mutable last = ValueNone
        let! step = e.MoveNextAsync()
        go <- step

        while go do
            last <- ValueSome e.Current
            let! step = e.MoveNextAsync()
            go <- step

        match last with
        | ValueSome value -> return Some value
        | ValueNone -> return None
    }

    let tryHead (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())
        let mutable go = true
        let! step = e.MoveNextAsync()
        go <- step

        if go then return Some e.Current else return None
    }

    let tryItem index (source: taskSeq<_>) = task {
        if index < 0 then
            // while the loop below wouldn't run anyway, we don't want to call MoveNext in this case
            // to prevent side effects hitting unnecessarily
            return None
        else
            let e = source.GetAsyncEnumerator(CancellationToken())
            let mutable go = true
            let mutable idx = 0
            let mutable foundItem = None
            let! step = e.MoveNextAsync()
            go <- step

            while go && idx <= index do
                if idx = index then
                    foundItem <- Some e.Current

                let! step = e.MoveNextAsync()
                go <- step
                idx <- idx + 1

            return foundItem
    }

    let tryPick chooser (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())

        let mutable go = true
        let mutable foundItem = None
        let! step = e.MoveNextAsync()
        go <- step

        match chooser with
        | TryPick picker ->
            while go do
                match picker e.Current with
                | Some value ->
                    foundItem <- Some value
                    go <- false
                | None ->
                    let! step = e.MoveNextAsync()
                    go <- step

        | TryPickAsync picker ->
            while go do
                match! picker e.Current with
                | Some value ->
                    foundItem <- Some value
                    go <- false
                | None ->
                    let! step = e.MoveNextAsync()
                    go <- step

        return foundItem
    }

    let tryFind chooser (source: taskSeq<_>) = task {
        let e = source.GetAsyncEnumerator(CancellationToken())

        let mutable go = true
        let mutable foundItem = None
        let! step = e.MoveNextAsync()
        go <- step

        match chooser with
        | TryFilter filterer ->
            while go do
                let current = e.Current

                match filterer current with
                | true ->
                    foundItem <- Some current
                    go <- false
                | false ->
                    let! step = e.MoveNextAsync()
                    go <- step

        | TryFilterAsync filterer ->
            while go do
                let current = e.Current

                match! filterer current with
                | true ->
                    foundItem <- Some current
                    go <- false
                | false ->
                    let! step = e.MoveNextAsync()
                    go <- step

        return foundItem
    }

    let choose chooser (source: taskSeq<_>) = taskSeq {
        match chooser with
        | TryPick picker ->
            for item in source do
                match picker item with
                | Some value -> yield value
                | None -> ()

        | TryPickAsync picker ->
            for item in source do
                match! picker item with
                | Some value -> yield value
                | None -> ()
    }

    let filter chooser (source: taskSeq<_>) = taskSeq {
        match chooser with
        | TryFilter filterer ->
            for item in source do
                if filterer item then
                    yield item

        | TryFilterAsync filterer ->
            for item in source do
                match! filterer item with
                | true -> yield item
                | false -> ()
    }
