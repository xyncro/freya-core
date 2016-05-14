namespace Freya.Core

(* Operators

   Symbolic operators for commonly used functions around core Freya
   functionality, particularly monadic operations, including monadic optic
   operations. *)

module Operators =

    (* Common

       Symbolic operators for common monadic functions, using common operators
       to allow for familiarity and some compliance with effective
       "standards". *)

    let inline (<*>) f m =
        Freya.apply (m, f)

    let inline (>>=) m f =
        Freya.bind (m, f)

    let inline (=<<) f m =
        Freya.bind (m, f)

    let inline (>=>) m1 m2 =
        fun x -> Freya.bind (m1 x, m2)

    let inline (<=<) m1 m2 =
        fun x -> Freya.bind (m2 x, m1)

    let inline ( *>) m1 m2 =
        Freya.combine (m1, m2)

    let inline ( <*) m1 m2 =
        Freya.combine (m2, m1)

    let inline (<!>) f m =
        Freya.map (m, f)

    (* Optic

       Operators for applying optic based functions to the State instance,
       wrapping the Freya.Optic.* functionality. *)

    let inline (!.) o =
        Freya.Optic.get o

    let inline (.=) o v =
        Freya.Optic.set o v

    let inline (%=) o f =
        Freya.Optic.map o f

    (* Pipeline

       Pipeline composition operators, allowing for an alternative chained
       syntax to Pipeline.compose, a more natural expression of the effective
       meaning. *)

    let inline (>?=) p1 p2 =
        Pipeline.compose p1 p2