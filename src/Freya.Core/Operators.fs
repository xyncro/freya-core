namespace Freya.Core

open System

// Operators

/// Symbolic operators for commonly used functions around core Freya
/// functionality, particularly monadic operations, including monadic optic
/// operations.

module Operators =

    // Common

    // Symbolic operators for common monadic functions, using common operators
    // to allow for familiarity and some compliance with effective "standards".

    /// The Apply (Freya.apply) function for Freya function types, taking a
    /// function Freya<'a -> 'b> and a Freya<'a> and returning a Freya<'b>.

    let inline (<*>) f m =
        Freya.apply (m, f)

    /// The Bind (Freya.bind) function for Freya, taking a Freya<'a> and a
    /// function 'a -> Freya<'b> and returning a Freya<'b>.

    let inline (>>=) m f =
        Freya.bind (m, f)

    /// The reversed Bind function for Freya.

    let inline (=<<) f m =
        Freya.bind (m, f)

    /// The Kleisli composition function for Freya, taking a function
    /// 'a -> Freya<'b> and a function 'b -> Freya<'c> and returning a function
    /// 'a -> Freya<'c>.

    let inline (>=>) m1 m2 =
        fun x -> Freya.bind (m1 x, m2)

    /// The reversed Kleisli composition function for Freya.

    let inline (<=<) m1 m2 =
        fun x -> Freya.bind (m2 x, m1)

    /// The Left Combine (Freya.combine) function for Freya, taking two
    /// Freya<_> functions, composing their execution and returning the result
    /// of the first function.

    let inline ( *>) m1 m2 =
        Freya.combine (m1, m2)

    /// The Right Combine (Freya.combine) function for Freya, taking two
    /// Freya<_> functions, composing their execution and returning the result
    /// of the second function.

    let inline ( <*) m1 m2 =
        Freya.combine (m2, m1)

    /// The Map (Freya.map) function for Freya, taking a function 'a -> b' and
    /// a function Freya<'a> and returning a Freya<'b>.

    let inline (<!>) f m =
        Freya.map (m, f)

    // Optic

    // Operators for applying optic based functions to the State instance,
    // wrapping the Freya.Optic.* functionality.

    /// The optical get function (Freya.Optic.get), used to get a value within
    /// the current computation State given an optic from State to the required
    /// value.

    let inline (!.) o =
        Freya.Optic.get o

    /// The optical set function (Freya.Optic.set), used to set a value within
    /// the current computation State given an optic from State to the required
    /// value and an instance of the required value.

    let inline (.=) o v =
        Freya.Optic.set o v

    /// The optical map function (Freya.Optic.map), used to map a value within
    /// the current computation State given an optic from the State the required
    /// value and a function from the current value to the new value (a
    /// homomorphism).

    let inline (%=) o f =
        Freya.Optic.map o f

    // Pipeline

    // Pipeline composition operators, allowing for an alternative chained
    // syntax to Pipeline.compose, a more natural expression of the effective
    // meaning.

    /// The Pipeline composition function (Pipeline.compose), used to compose
    /// two functions which may be - or may be inferred to be (see
    /// Pipeline.infer) - Pipeline functions, given the composition approach of
    /// executing functions sequentially until one returns Halt.

    /// In this case, the first function will always be executed, and if the
    /// result is Next, the second pipeline will be executed and the result of
    /// the second pipeline returned. Where the result of the first Pipeline is
    /// Halt, the second pipeline will never be executed, and the Halt result
    /// will be returned.

    let inline (>?=) p1 p2 =
        Pipeline.compose p1 p2

    // Obsolete

    // Backwards compatibility shims to make the 2.x-> 3.x transition
    // less painful, providing functionally equivalent options where possible.

    // To be removed for 4.x releases.

    [<AutoOpen>]
    module Obsolete =

        [<Obsolete ("Use !. instead.")>]
        let inline (!?.) o =
            Freya.Optic.get o

        [<Obsolete ("Use .= instead.")>]
        let inline (.?=) o v =
            Freya.Optic.set o v

        [<Obsolete ("Use %= instead.")>]
        let inline (%?=) o f =
            Freya.Optic.map o f