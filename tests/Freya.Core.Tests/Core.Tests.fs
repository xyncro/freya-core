module Freya.Core.Tests

open System.Collections.Generic
open Freya.Core
open Swensen.Unquote
open Xunit

#if HOPAC
open Hopac
#endif

//

// Helper functions to make running tests against the Freya function simpler,
// including a standard known state.

let private environment () =
    let e = Dictionary<string, obj> () :> IDictionary<string, obj>
    e.["o1"] <- false
    e.["o2"] <- false
    e

let private meta () =
    { Memos = Map.empty }

let private state initial =
    { Environment = environment ()
      Meta = meta () }

let private run f =
#if HOPAC
    Hopac.run (f (state ()))
#else
    Async.RunSynchronously (f (state ()))
#endif

// Common

// Tests of the common functions applying to the Freya function, and
// underlying the computation expression builder.

[<Fact>]
let ``Freya.init and Freya.apply behave correctly`` () =
    let aF = Freya.init 2
    let a2Fb = Freya.init ((+) 40)

    Aether.Optic.get FreyaResult.value_ (run (a2Fb |> Freya.apply aF)) =! 42

[<Fact>]
let ``Freya.combine behaves correctly`` () =
    let ret =
#if HOPAC
        Job.result
#else
        async.Return
#endif
    let aF = fun s -> s.Environment.["o1"] <- true; ret <| FreyaResult.create 4 s
    let bF = fun s -> s.Environment.["o3"] <- true; ret <| FreyaResult.create 2 s

    let ((FreyaResult.State s) as fr) = run (aF |> Freya.combine bF)
    Aether.Optic.get FreyaResult.value_ fr =! 2
    unbox s.Environment.["o1"] =! true
    unbox s.Environment.["o2"] =! false
    unbox s.Environment.["o3"] =! true

open Freya.Core.Optics

// Optic

// Tests of the various Optic functions which work on the Freya function,
// enabling optic based operations on the underlying state.

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

    Aether.Optic.get FreyaResult.value_ (run m) =! (Some 42, Some 84)

[<Fact>]
let ``OwinMidFunc.ofFreya creates a well-behaved middleware that doesn't continue pipeline on Halt`` () =
    let m = freya {
        return Halt
    }

    let f : OwinMidFunc = OwinMidFunc.ofFreya m
    let inner : OwinAppFunc = OwinAppFunc (fun env -> failwithf "Should not run"; System.Threading.Tasks.Task.CompletedTask)
    f.Invoke(inner).Invoke(dict []).Wait()

[<Fact>]
let ``OwinMidFunc.ofFreya creates a well-behaved middleware that continues the pipeline on Next`` () =
    let m = freya {
        return Next
    }

    let f : OwinMidFunc = OwinMidFunc.ofFreya m
    let mutable hit = false
    let inner : OwinAppFunc = OwinAppFunc (fun env -> hit <- true; System.Threading.Tasks.Task.CompletedTask)
    f.Invoke(inner).Invoke(dict []).Wait()
    hit =! true
