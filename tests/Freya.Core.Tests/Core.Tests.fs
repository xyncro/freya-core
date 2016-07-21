﻿module Freya.Core.Tests

open System.Collections.Generic
open Freya.Core
open Swensen.Unquote
open Xunit

#if Hopac

open Hopac

#endif

(* Prelude

   Helper functions to make running tests against the Freya function simpler,
   including a standard known state. *)

let private environment () =
    let e = Dictionary<string, obj> () :> IDictionary<string, obj>
    e.["o1"] <- false
    e.["o2"] <- false
    e

let private meta () =
    { Memos = Map.empty }

let private state () =
    { Environment = environment ()
      Meta = meta () }

#if Hopac

let private run f =
    Job.Global.run (f (state ()))

#else

let private run f =
    Async.RunSynchronously (f (state ()))

#endif

(* Common

   Tests of the common functions applying to the Freya function, and
   underlying the computation expression builder. *)

[<Fact>]
let ``Freya.init and Freya.apply behave correctly`` () =
    let m = Freya.init 2
    let f = Freya.init ((+) 40)

    fst (run (Freya.apply (m, f))) =! 42

(* Optic

   Tests of the various Optic functions which work on the Freya function,
   enabling optic based operations on the underlying state. *)

let private key_ =
    State.value_ "key"

[<Fact>]
let ``Freya.Optic.get|set|map behave correctly`` () =
    let m =
        freya {
            do! Freya.Optic.set key_ (Some 42)
            let! v1 = Freya.Optic.get key_

            do! Freya.Optic.map key_ (Option.map ((*) 2))
            let! v2 = Freya.Optic.get key_

            return v1, v2 }

    fst (run m) =! (Some 42, Some 84)