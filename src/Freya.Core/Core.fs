namespace Freya.Core

#if HOPAC
open Hopac
#endif

// Core

// The common elements of all Freya based systems, namely the basic abstraction
// of an async state function over an OWIN environment, and tools for working
// with the environment in a functional and idiomatic way.

// Types

// Core types within the Freya codebase, representing the basic units of
// execution and composition, including the core async state carrying
// abstraction.

/// The core Freya type, representing a computation which is effectively a
/// State monad, with a concurrent return (the concurrency abstraction varies
/// based on the variant of Freya in use).

type Freya<'a> =
#if HOPAC
    State -> Job<FreyaResult<'a>>
#else
    State -> Async<FreyaResult<'a>>
#endif

and FreyaResult<'a> =
#if STRUCT
    (struct ('a * State))
#else
    'a * State
#endif

/// The core Freya state type, containing the OWIN environment and other
/// metadata data structures which should be passed through a Freya
/// computation.

and State =
    { Environment: Environment
      Meta: Meta }

    static member environment_ =
        (fun x -> x.Environment),
        (fun e x -> { x with Environment = e })

    static member meta_ =
        (fun x -> x.Meta),
        (fun m x -> { x with Meta = m })

    static member create : Environment -> State =
        fun (env : Environment) ->
            { Environment = env
              Meta = Meta.empty }

/// An alias for the commonly used OWIN data type of an
/// IDictionary<string,obj>.

and Environment =
    System.Collections.Generic.IDictionary<string, obj>

/// The Freya metadata data type containing data which should be passed through
/// a Freya computation but which is not relevant to non-Freya functions and so
/// is not considered part of the OWIN data model.

and Meta =
    { Memos: Map<System.Guid, obj> }

    static member memos_ =
        (fun x -> x.Memos),
        (fun m x -> { x with Memos = m })

    static member empty =
        { Memos = Map.empty }


[<AutoOpen>]
module Patterns =
    let inline (|FreyaResult|) (fr: FreyaResult<'a>) =
        match fr with
#if STRUCT
        | struct (a, s) -> (a, s)
#else
        | a, s -> (a, s)
#endif



[<RequireQualifiedAccess>]
module FreyaResult =
    let inline (|State|) (fr: FreyaResult<'a>) =
        match fr with
#if STRUCT
        | struct (_, s) -> s
#else
        | _, s -> s
#endif

    let inline (|Value|) (fr: FreyaResult<'a>) =
        match fr with
#if STRUCT
        | struct (a, _) -> a
#else
        | a, _ -> a
#endif

    let inline create a s : FreyaResult<'a> =
#if STRUCT
        struct (a, s)
#else
        (a, s)
#endif

    let inline createWithState s a : FreyaResult<'a> =
        create a s

    let value_ : Aether.Lens<FreyaResult<'a>,'a> =
        (fun (Value a) -> a),
        (fun a (State s) -> create a s)

    let state_ : Aether.Lens<FreyaResult<'a>,State> =
        (fun (State s) -> s),
        (fun s (Value a) -> create a s)

// State

/// Basic optics for accessing elements of the State instance within the
/// current Freya function. The value_ lens is provided for keyed access
/// to the OWIN dictionary, and the memo_ lens for keyed access to the
/// memo storage in the Meta instance.

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module State =
    open Aether
    open Aether.Operators

    /// A prism from the Freya State to a value of type 'a at a given string
    /// key.

    let key_<'a> k =
            State.environment_
        >-> Dict.key_<string,obj> k
        >?> box_<'a>

    /// A lens from the Freya State to a value of type 'a option at a given
    /// string key.

    /// When working with this lens as an optic, the Some and None cases of
    /// optic carry semantic meaning, where Some indicates that the value is or
    /// should be present within the State, and None indicates that the value
    /// is not, or should not be present within the State.

    let value_<'a> k =
            State.environment_
        >-> Dict.value_<string,obj> k
        >-> Option.mapIsomorphism box_<'a>

    /// A lens from the Freya State to a memoized value of type 'a at a given
    /// Guid key.

    /// When working with this lens as an optic, the Some and None cases of
    /// optic carry semantic meaning, where Some indicates that the value is or
    /// should be present within the State, and None indicates that the value
    /// is not, or should not be present within the State.

    let memo_<'a> i =
            State.meta_
        >-> Meta.memos_
        >-> Map.value_ i
        >-> Option.mapIsomorphism box_<'a>

// Freya

/// Functions and type tools for working with Freya abstractions, particularly
/// data contained within the Freya state abstraction. Commonly defined
/// functions for treating the Freya functions as a monad, etc. are also
/// included, along with basic support for static inference.

[<RequireQualifiedAccess>]
module Freya =

    // Common

    // Commonly defined functions against the Freya types, particularly the
    // usual monadic functions (bind, apply, etc.). These are commonly used
    // directly within Freya programming but are also used within the Freya
    // computation expression defined later.

    /// The init (or pure) function, used to raise a value of type 'a to a
    /// value of type Freya<'a>.

    let init (a: 'a) : Freya<'a> =
        FreyaResult.create a
#if HOPAC
        >> Job.result
#else
        >> async.Return
#endif

    /// The map function, used to map a value of type Freya<'a> to Freya<'b>,
    /// given a function 'a -> 'b.

    let map (a2b: 'a -> 'b) (aF: Freya<'a>) : Freya<'b> =
        fun s ->
#if HOPAC
            aF s |> Job.map (fun (FreyaResult (a, s1)) -> FreyaResult.create (a2b a) s1)
#else
            async.Bind (aF s, fun (FreyaResult (a, s1)) ->
                async.Return (FreyaResult.create (a2b a) s1))
#endif

    /// Takes two Freya values and maps them into a function
    let map2 (a2b2c: 'a -> 'b -> 'c) (aF: Freya<'a>) (bF: Freya<'b>) : Freya<'c> =
        fun s ->
#if HOPAC
            aF s |> Job.bind (fun (FreyaResult (a, s1)) ->
                bF s1 |> Job.map (fun (FreyaResult (b, s2)) ->
                    FreyaResult.create (a2b2c a b) s2))
#else
            async.Bind (aF s, fun (a, s1) ->
                async.Bind (bF s1, fun (b, s2) ->
                    async.Return (FreyaResult.create (a2b2c a b) s2)))
#endif

    /// Takes two Freya values and maps them into a function
    let map3 (a2b2c2d: 'a -> 'b -> 'c -> 'd) (aF: Freya<'a>) (bF: Freya<'b>) (cF: Freya<'c>) : Freya<'d> =
        fun s ->
#if HOPAC
            aF s |> Job.bind (fun (FreyaResult (a, s1)) ->
                bF s1 |> Job.bind (fun (FreyaResult (b, s2)) ->
                    cF s2 |> Job.map (fun (FreyaResult (c, s3)) ->
                        FreyaResult.create (a2b2c2d a b c) s3)))
#else
            async.Bind (aF s, fun (a, s1) ->
                async.Bind (bF s1, fun (b, s2) ->
                    async.Bind (cF s2, fun (c, s3) ->
                        async.Return (FreyaResult.create (a2b2c2d a b c) s3))))
#endif

    /// The Bind function for Freya, taking a Freya<'a> and a function
    /// 'a -> Freya<'b> and returning a Freya<'b>.

    let bind (a2bF: 'a -> Freya<'b>) (aF: Freya<'a>) : Freya<'b> =
        fun s ->
#if HOPAC
            aF s |> Job.bind (fun (FreyaResult (a, s1)) -> a2bF a s1)
#else
            async.Bind (aF s, fun (FreyaResult (a, s1)) -> a2bF a s1)
#endif

    /// The apply function for Freya function types, taking a function
    /// Freya<'a -> 'b> and a Freya<'a> and returning a Freya<'b>.

    let apply (aF: Freya<'a>) (a2Fb: Freya<'a -> 'b>) : Freya<'b> =
        fun s ->
#if HOPAC
            a2Fb s |> Job.bind (fun (FreyaResult (a2b, s1)) ->
                aF s1 |> Job.map (fun (FreyaResult (a, s2)) ->
                    FreyaResult.create (a2b a) s2))
#else
            async.Bind (a2Fb s, fun (FreyaResult (a2b, s1)) ->
                async.Bind (aF s1, fun (FreyaResult (a, s2)) ->
                    async.Return (FreyaResult.create (a2b a) s2)))
#endif

    /// The Left Combine function for Freya, taking two Freya<_> functions,
    /// composing their execution and returning the result of the first
    /// function.

    let combine (aF: Freya<'a>) (xF: Freya<'x>) : Freya<'a> =
        fun s ->
#if HOPAC
            xF s |> Job.bind (fun (FreyaResult.State s1) -> aF s1)
#else
            async.Bind (xF s, fun (FreyaResult.State s1) -> aF s1)
#endif

    /// The Freya delay function, used to delay execution of a freya function
    /// by consuming a unit function to return the underlying Freya function.

    let delay (u2aF: unit -> Freya<'a>) : Freya<'a> =
#if HOPAC
        Job.delayWith (fun s -> u2aF () s)
#else
        fun s ->
            u2aF () s
#endif

    /// The identity function for Freya type functions.

    let identity (xF: Freya<_>) : Freya<_> =
        xF

    // Empty

    /// A simple convenience instance of an empty Freya function, returning
    /// the unit type. This can be required for various forms of branching logic
    /// etc. and is a convenience to save writing Freya.init () repeatedly.
    let empty : Freya<unit> =
        init ()

    /// The zero function, used to initialize a new function of Freya<unit>,
    /// effectively lifting the unit value to a Freya<unit> function.

    let zero () : Freya<unit> = empty

    // Extended

    // Some extended functions providing additional convenience outside of the
    // usual set of functions defined against Freya. In this case, interop with
    // the basic F# async system, and extended dual map function are given.

#if HOPAC

    /// Converts a Hopac Job to a Freya
    let fromJob (aJ: Job<'a>) : Freya<'a> =
        fun s ->
            aJ |> Job.map (FreyaResult.createWithState s)

    /// Lifts a function generating a Hopac Job to one creating a Freya
    let liftJob (a2bJ: 'a -> Job<'b>) (a: 'a) : Freya<'b> =
        fun s ->
            a2bJ a |> Job.map (FreyaResult.createWithState s)

    /// Binds a Hopac Job to a function generating a Freya
    let bindJob (a2bF: 'a -> Freya<'b>) (aJ: Job<'a>) : Freya<'b> =
        fun s ->
            aJ |> Job.bind (fun a -> a2bF a s)

#endif

    /// Converts an Async to a Freya
    let fromAsync (aA: Async<'a>) : Freya<'a> =
        fun s ->
#if HOPAC
            Job.fromAsync aA |> Job.map (FreyaResult.createWithState s)
#else
            async.Bind (aA, fun a ->
                async.Return (FreyaResult.create a s))
#endif

    /// Lifts a function generating an Async to one creating a Freya
    let liftAsync (a2bA: 'a -> Async<'b>) (a: 'a) : Freya<'b> =
        fun s ->
#if HOPAC
            a2bA a |> Job.fromAsync |> Job.map (FreyaResult.createWithState s)
#else
            async.Bind (a2bA a, fun b ->
                async.Return (FreyaResult.create b s))
#endif

    /// Binds an Async to a function generating a Freya
    let bindAsync (a2bF: 'a -> Freya<'b>) (aA: Async<'a>) : Freya<'b> =
        fun s ->
#if HOPAC
            Job.fromAsync aA |> Job.bind (fun a -> a2bF a s)
#else
            async.Bind (aA, fun a -> a2bF a s)
#endif

    // Memoisation

    /// A function supporting memoisation of parameterless Freya functions
    /// (effectively a fully applied Freya expression) which will cache the
    /// result of the function in the Environment instance. This ensures that
    /// the function will be evaluated once per request/response on any given
    /// thread.
    let memo<'a> (aF: Freya<'a>) : Freya<'a> =
        let memo_ = State.memo_<'a> (System.Guid.NewGuid ())

        fun s ->
            match Aether.Optic.get memo_ s with
            | Some memo ->
#if HOPAC
                Job.result (FreyaResult.create memo s)
#else
                async.Return (FreyaResult.create memo s)
#endif
            | _ ->
#if HOPAC
                aF s |> Job.map (fun (FreyaResult (memo, s)) ->
                    (FreyaResult.create memo (Aether.Optic.set memo_ (Some memo) s)))
#else
                async.Bind (aF s, fun (memo, s) ->
                    async.Return (FreyaResult.create memo (Aether.Optic.set memo_ (Some memo) s)))
#endif
