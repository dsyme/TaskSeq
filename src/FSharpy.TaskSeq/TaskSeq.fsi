namespace FSharpy

module TaskSeq =
    open System.Collections.Generic
    open System.Threading.Tasks
    open FSharpy.TaskSeqBuilders

    /// Initialize an empty taskSeq.
    val empty<'T> : taskSeq<'T>

    /// <summary>
    /// Returns <see cref="true" /> if the task sequence contains no elements, <see cref="false" /> otherwise.
    /// </summary>
    val isEmpty: source: taskSeq<'T> -> Task<bool>

    /// Returns taskSeq as an array. This function is blocking until the sequence is exhausted and will properly dispose of the resources.
    val toList: source: taskSeq<'T> -> 'T list

    /// Returns taskSeq as an array. This function is blocking until the sequence is exhausted and will properly dispose of the resources.
    val toArray: source: taskSeq<'T> -> 'T[]

    /// Returns taskSeq as a seq, similar to Seq.cached. This function is blocking until the sequence is exhausted and will properly dispose of the resources.
    val toSeqCached: source: taskSeq<'T> -> seq<'T>

    /// Unwraps the taskSeq as a Task<array<_>>. This function is non-blocking.
    val toArrayAsync: source: taskSeq<'T> -> Task<'T[]>

    /// Unwraps the taskSeq as a Task<list<_>>. This function is non-blocking.
    val toListAsync: source: taskSeq<'T> -> Task<'T list>

    /// Unwraps the taskSeq as a Task<ResizeArray<_>>. This function is non-blocking.
    val toResizeArrayAsync: source: taskSeq<'T> -> Task<ResizeArray<'T>>

    /// Unwraps the taskSeq as a Task<IList<_>>. This function is non-blocking.
    val toIListAsync: source: taskSeq<'T> -> Task<IList<'T>>

    /// Unwraps the taskSeq as a Task<seq<_>>. This function is non-blocking,
    /// exhausts the sequence and caches the results of the tasks in the sequence.
    val toSeqCachedAsync: source: taskSeq<'T> -> Task<seq<'T>>

    /// Create a taskSeq of an array.
    val ofArray: source: 'T[] -> taskSeq<'T>

    /// Create a taskSeq of a list.
    val ofList: source: 'T list -> taskSeq<'T>

    /// Create a taskSeq of a seq.
    val ofSeq: source: seq<'T> -> taskSeq<'T>

    /// Create a taskSeq of a ResizeArray, aka List.
    val ofResizeArray: source: ResizeArray<'T> -> taskSeq<'T>

    /// Create a taskSeq of a sequence of tasks, that may already have hot-started.
    val ofTaskSeq: source: seq<#Task<'T>> -> taskSeq<'T>

    /// Create a taskSeq of a list of tasks, that may already have hot-started.
    val ofTaskList: source: #Task<'T> list -> taskSeq<'T>

    /// Create a taskSeq of an array of tasks, that may already have hot-started.
    val ofTaskArray: source: #Task<'T> array -> taskSeq<'T>

    /// Create a taskSeq of a seq of async.
    val ofAsyncSeq: source: seq<Async<'T>> -> taskSeq<'T>

    /// Create a taskSeq of a list of async.
    val ofAsyncList: source: Async<'T> list -> taskSeq<'T>

    /// Create a taskSeq of an array of async.
    val ofAsyncArray: source: Async<'T> array -> taskSeq<'T>

    /// Iterates over the taskSeq applying the action function to each item. This function is non-blocking
    /// exhausts the sequence as soon as the task is evaluated.
    val iter: action: ('T -> unit) -> source: taskSeq<'T> -> Task<unit>

    /// Iterates over the taskSeq applying the action function to each item. This function is non-blocking,
    /// exhausts the sequence as soon as the task is evaluated.
    val iteri: action: (int -> 'T -> unit) -> source: taskSeq<'T> -> Task<unit>

    /// Iterates over the taskSeq applying the async action to each item. This function is non-blocking
    /// exhausts the sequence as soon as the task is evaluated.
    val iterAsync: action: ('T -> #Task<unit>) -> source: taskSeq<'T> -> Task<unit>

    /// Iterates over the taskSeq, applying the async action to each item. This function is non-blocking,
    /// exhausts the sequence as soon as the task is evaluated.
    val iteriAsync: action: (int -> 'T -> #Task<unit>) -> source: taskSeq<'T> -> Task<unit>

    /// Maps over the taskSeq, applying the mapper function to each item. This function is non-blocking.
    val map: mapper: ('T -> 'U) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Maps over the taskSeq with an index, applying the mapper function to each item. This function is non-blocking.
    val mapi: mapper: (int -> 'T -> 'U) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Maps over the taskSeq, applying the async mapper function to each item. This function is non-blocking.
    val mapAsync: mapper: ('T -> #Task<'U>) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Maps over the taskSeq with an index, applying the async mapper function to each item. This function is non-blocking.
    val mapiAsync: mapper: (int -> 'T -> #Task<'U>) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Applies the given function to the items in the taskSeq and concatenates all the results in order.
    val collect: binder: ('T -> #taskSeq<'U>) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Applies the given function to the items in the taskSeq and concatenates all the results in order.
    val collectSeq: binder: ('T -> #seq<'U>) -> source: taskSeq<'T> -> taskSeq<'U>

    /// Applies the given async function to the items in the taskSeq and concatenates all the results in order.
    val collectAsync:
        binder: ('T -> #Task<'TSeqU>) -> source: taskSeq<'T> -> taskSeq<'U> when 'TSeqU :> taskSeq<'U>

    /// Applies the given async function to the items in the taskSeq and concatenates all the results in order.
    val collectSeqAsync:
        binder: ('T -> #Task<'SeqU>) -> source: taskSeq<'T> -> taskSeq<'U> when 'SeqU :> seq<'U>

    /// <summary>
    /// Returns the first element of the <see cref="taskSeq" />, or <see cref="None" /> if the sequence is empty.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the sequence is empty.</exception>
    val tryHead: source: taskSeq<'T> -> Task<'T option>

    /// <summary>
    /// Returns the first element of the <see cref="taskSeq" />.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the sequence is empty.</exception>
    val head: source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Returns the last element of the <see cref="taskSeq" />, or <see cref="None" /> if the sequence is empty.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the sequence is empty.</exception>
    val tryLast: source: taskSeq<'T> -> Task<'T option>

    /// <summary>
    /// Returns the last element of the <see cref="taskSeq" />.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the sequence is empty.</exception>
    val last: source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Returns the nth element of the <see cref="taskSeq" />, or <see cref="None" /> if the sequence
    /// does not contain enough elements, or if <paramref name="index" /> is negative.
    /// Parameter <paramref name="index" /> is zero-based, that is, the value 0 returns the first element.
    /// </summary>
    val tryItem: index: int -> source: taskSeq<'T> -> Task<'T option>

    /// <summary>
    /// Returns the nth element of the <see cref="taskSeq" />, or <see cref="None" /> if the sequence
    /// does not contain enough elements, or if <paramref name="index" /> is negative.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the sequence has insufficient length or
    /// <paramref name="index" /> is negative.</exception>
    val item: index: int -> source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Returns the only element of the task sequence, or <see cref="None" /> if the sequence is empty of
    /// contains more than one element.
    /// </summary>
    val tryExactlyOne: source: taskSeq<'T> -> Task<'T option>

    /// <summary>
    /// Returns the only element of the task sequence.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the input sequence does not contain precisely one element.</exception>
    val exactlyOne: source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Applies the given function <paramref name="chooser" /> to each element of the task sequence. Returns
    /// a sequence comprised of the results "x" for each element where
    /// the function returns <c>Some(x)</c>.
    /// If <paramref name="chooser" /> is asynchronous, consider using <see cref="TaskSeq.chooseAsync" />.
    /// </summary>
    val choose: chooser: ('T -> 'U option) -> source: taskSeq<'T> -> taskSeq<'U>

    /// <summary>
    /// Applies the given asynchronous function <paramref name="chooser" /> to each element of the task sequence. Returns
    /// a sequence comprised of the results "x" for each element where
    /// the function returns <see cref="Some(x)" />.
    /// If <paramref name="chooser" /> does not need to be asynchronous, consider using <see cref="TaskSeq.choose" />.
    /// </summary>
    val chooseAsync: chooser: ('T -> #Task<'U option>) -> source: taskSeq<'T> -> taskSeq<'U>

    /// <summary>
    /// Returns a new collection containing only the elements of the collection
    /// for which the given <paramref name="predicate" /> function returns <see cref="true" />.
    /// If <paramref name="predicate" /> is asynchronous, consider using <see cref="TaskSeq.filterAsync" />.
    /// </summary>
    val filter: predicate: ('T -> bool) -> source: taskSeq<'T> -> taskSeq<'T>

    /// <summary>
    /// Returns a new collection containing only the elements of the collection
    /// for which the given asynchronous function <paramref name="predicate" /> returns <see cref="true" />.
    /// If <paramref name="predicate" /> does not need to be asynchronous, consider using <see cref="TaskSeq.filter" />.
    /// </summary>
    val filterAsync: predicate: ('T -> #Task<bool>) -> source: taskSeq<'T> -> taskSeq<'T>

    /// <summary>
    /// Applies the given function <paramref name="chooser" /> to successive elements of the task sequence
    /// in <paramref name="source" />, returning the first result where the function returns <see cref="Some(x)" />.
    /// If <paramref name="chooser" /> is asynchronous, consider using <see cref="TaskSeq.tryPickAsync" />.
    /// </summary>
    val tryPick: chooser: ('T -> 'U option) -> source: taskSeq<'T> -> Task<'U option>

    /// <summary>
    /// Applies the given asynchronous function <paramref name="chooser" /> to successive elements of the task sequence
    /// in <paramref name="source" />, returning the first result where the function returns <see cref="Some(x)" />.
    /// If <paramref name="chooser" /> does not need to be asynchronous, consider using <see cref="TaskSeq.tryPick" />.
    /// </summary>
    val tryPickAsync: chooser: ('T -> #Task<'U option>) -> source: taskSeq<'T> -> Task<'U option>

    /// <summary>
    /// Returns the first element of the task sequence in <paramref name="source" /> for which the given function
    /// <paramref name="predicate" /> returns <see cref="true" />. Returns <see cref="None" /> if no such element exists.
    /// If <paramref name="predicate" /> is asynchronous, consider using <see cref="TaskSeq.tryFindAsync" />.
    /// </summary>
    val tryFind: predicate: ('T -> bool) -> source: taskSeq<'T> -> Task<'T option>

    /// <summary>
    /// Returns the first element of the task sequence in <paramref name="source" /> for which the given asynchronous function
    /// <paramref name="predicate" /> returns <see cref="true" />. Returns <see cref="None" /> if no such element exists.
    /// If <paramref name="predicate" /> does not need to be asynchronous, consider using <see cref="TaskSeq.tryFind" />.
    /// </summary>
    val tryFindAsync: predicate: ('T -> #Task<bool>) -> source: taskSeq<'T> -> Task<'T option>


    /// <summary>
    /// Applies the given function <paramref name="chooser" /> to successive elements of the task sequence
    /// in <paramref name="source" />, returning the first result where the function returns <see cref="Some(x)" />.
    /// If <paramref name="chooser" /> is asynchronous, consider using <see cref="TaskSeq.pickAsync" />.
    /// <exception cref="KeyNotFoundException">Thrown when every item of the sequence
    /// evaluates to <see cref="None" /> when the given function is applied.</exception>
    /// </summary>
    val pick: chooser: ('T -> 'U option) -> source: taskSeq<'T> -> Task<'U>

    /// <summary>
    /// Applies the given asynchronous function <paramref name="chooser" /> to successive elements of the task sequence
    /// in <paramref name="source" />, returning the first result where the function returns <see cref="Some(x)" />.
    /// If <paramref name="chooser" /> does not need to be asynchronous, consider using <see cref="TaskSeq.pick" />.
    /// <exception cref="KeyNotFoundException">Thrown when every item of the sequence
    /// evaluates to <see cref="None" /> when the given function is applied.</exception>
    /// </summary>
    val pickAsync: chooser: ('T -> #Task<'U option>) -> source: taskSeq<'T> -> Task<'U>

    /// <summary>
    /// Returns the first element of the task sequence in <paramref name="source" /> for which the given function
    /// <paramref name="predicate" /> returns <see cref="true" />.
    /// If <paramref name="predicate" /> is asynchronous, consider using <see cref="TaskSeq.findAsync" />.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown if no element returns <see cref="true" /> when
    /// evaluated by the <paramref name="predicate" /> function.</exception>
    val find: predicate: ('T -> bool) -> source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Returns the first element of the task sequence in <paramref name="source" /> for which the given
    /// asynchronous function <paramref name="predicate" /> returns <see cref="true" />.
    /// If <paramref name="predicate" /> does not need to be asynchronous, consider using <see cref="TaskSeq.find" />.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown if no element returns <see cref="true" /> when
    /// evaluated by the <paramref name="predicate" /> function.</exception>
    val findAsync: predicate: ('T -> #Task<bool>) -> source: taskSeq<'T> -> Task<'T>

    /// <summary>
    /// Zips two task sequences, returning a taskSeq of the tuples of each sequence, in order. May raise ArgumentException
    /// if the sequences are or unequal length.
    /// </summary>
    /// <exception cref="ArgumentException">The sequences have different lengths.</exception>
    val zip: source1: taskSeq<'T> -> source2: taskSeq<'U> -> taskSeq<'T * 'U>

    /// <summary>
    /// Applies the function <paramref name="folder" /> to each element in the task sequence,
    /// threading an accumulator argument of type <paramref name="'State" /> through the computation.
    /// If the accumulator function <paramref name="folder" /> is asynchronous, consider using <see cref="TaskSeq.foldAsync" />.
    /// </summary>
    val fold: folder: ('State -> 'T -> 'State) -> state: 'State -> source: taskSeq<'T> -> Task<'State>

    /// <summary>
    /// Applies the asynchronous function <paramref name="folder" /> to each element in the task sequence,
    /// threading an accumulator argument of type <paramref name="'State" /> through the computation.
    /// If the accumulator function <paramref name="folder" /> does not need to be asynchronous, consider using <see cref="TaskSeq.fold" />.
    /// </summary>
    val foldAsync:
        folder: ('State -> 'T -> #Task<'State>) -> state: 'State -> source: taskSeq<'T> -> Task<'State>
