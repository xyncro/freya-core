namespace Freya.Core

open Aether

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

    static member environment_: Lens<State,Environment>
    static member meta_: Lens<State,Meta>
    static member create: (Environment -> State)

/// An alias for the commonly used OWIN data type of an
/// IDictionary<string,obj>.

and Environment =
    System.Collections.Generic.IDictionary<string, obj>

/// The Freya metadata data type containing data which should be passed through
/// a Freya computation but which is not relevant to non-Freya functions and so
/// is not considered part of the OWIN data model.

and Meta =
    { Memos: Map<System.Guid, obj> }

    static member memos_: Lens<Meta,Map<System.Guid, obj>>
    static member empty: Meta

[<AutoOpen>]
module Patterns =
    val inline (|FreyaResult|): fr: FreyaResult<'a> -> 'a * State

[<RequireQualifiedAccess>]
module FreyaResult =
    val inline (|State|): fr: FreyaResult<'a> -> State
    val inline (|Value|): fr: FreyaResult<'a> -> 'a

    val inline create: a:'a -> s:State -> FreyaResult<'a>
    val inline createWithState: s:State -> a:'a -> FreyaResult<'a>

    val value_: Lens<FreyaResult<'a>,'a>
    val state_: Lens<FreyaResult<'a>,State>

// State

/// Basic optics for accessing elements of the State instance within the
/// current Freya function. The value_ lens is provided for keyed access
/// to the OWIN dictionary, and the memo_ lens for keyed access to the
/// memo storage in the Meta instance.

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module State =

    /// A prism from the Freya State to a value of type 'a at a given string
    /// key.

    val key_<'a> : key:string -> Prism<State,'a>

    /// A lens from the Freya State to a value of type 'a option at a given
    /// string key.

    /// When working with this lens as an optic, the Some and None cases of
    /// optic carry semantic meaning, where Some indicates that the value is or
    /// should be present within the State, and None indicates that the value
    /// is not, or should not be present within the State.

    val value_<'a> : key:string -> Lens<State,'a option>

    /// A lens from the Freya State to a memoized value of type 'a at a given
    /// Guid key.

    /// When working with this lens as an optic, the Some and None cases of
    /// optic carry semantic meaning, where Some indicates that the value is or
    /// should be present within the State, and None indicates that the value
    /// is not, or should not be present within the State.

    val memo_<'a> : memoId:System.Guid -> Lens<State,'a option>

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
    val init: a: 'a -> Freya<'a>

    /// The map function, used to map a value of type Freya<'a> to Freya<'b>,
    /// given a function 'a -> 'b.
    val map: a2b: ('a -> 'b) -> aF: Freya<'a> -> Freya<'b>

    /// The Bind function for Freya, taking a Freya<'a> and a function
    /// 'a -> Freya<'b> and returning a Freya<'b>.
    val bind: a2bF: ('a -> Freya<'b>) -> aF: Freya<'a> -> Freya<'b>

    /// The apply function for Freya function types, taking a function
    /// Freya<'a -> 'b> and a Freya<'a> and returning a Freya<'b>.
    val apply: aF: Freya<'a> -> a2Fb: Freya<'a -> 'b> -> Freya<'b>

    /// The Left Combine function for Freya, taking two Freya<_> functions,
    /// composing their execution and returning the result of the first
    /// function. `xF` is executed before `aF`, with `aF`'s result being
    /// returned.
    val combine: aF: Freya<'a> -> xF: Freya<'x> -> Freya<'a>

    /// The Freya delay function, used to delay execution of a freya function
    /// by consuming a unit function to return the underlying Freya function.
    val delay: u2aF: (unit -> Freya<'a>) -> Freya<'a>

    /// The identity function for Freya type functions.
    val identity: aF: Freya<'a> -> Freya<'a>

    /// A simple convenience instance of an empty Freya function, returning
    /// the unit type. This can be required for various forms of branching logic
    /// etc. and is a convenience to save writing Freya.init () repeatedly.
    val empty : Freya<unit>

    /// The zero function, used to initialize a new function of Freya<unit>,
    /// effectively lifting the unit value to a Freya<unit> function.
    val zero: unit -> Freya<unit>

    // Extended

    // Some extended functions providing additional convenience outside of the
    // usual set of functions defined against Freya. In this case, interop with
    // the basic F# async system, and extended dual map function are given.

#if HOPAC
    /// Converts a Hopac Job to a Freya
    val fromJob: aJ: Job<'a> -> Freya<'a>

    /// Lifts a function generating a Hopac Job to one creating a Freya
    val liftJob: a2bJ: ('a -> Job<'b>) -> a: 'a -> Freya<'b>

    /// Binds a Hopac Job to a function generating a Freya
    val bindJob: a2bF: ('a -> Freya<'b>) -> aJ: Job<'a> -> Freya<'b>
#endif

    /// Converts an Async to a Freya
    val fromAsync: aA: Async<'a> -> Freya<'a>

    /// Lifts a function generating an Async to one creating a Freya
    val liftAsync: a2bA: ('a -> Async<'b>) -> a: 'a -> Freya<'b>

    /// Binds an Async to a function generating a Freya
    val bindAsync: a2bF: ('a -> Freya<'b>) -> aA: Async<'a> -> Freya<'b>

    /// Takes two Freya values and maps them into a function
    val map2: a2b2c: ('a -> 'b -> 'c) -> aF: Freya<'a> -> bF: Freya<'b> -> Freya<'c>


    // Memoisation

    /// A function supporting memoisation of parameterless Freya functions
    /// (effectively a fully applied Freya expression) which will cache the
    /// result of the function in the Environment instance. This ensures that
    /// the function will be evaluated once per request/response on any given
    /// thread.
    val memo<'a> : aF: Freya<'a> -> Freya<'a>
