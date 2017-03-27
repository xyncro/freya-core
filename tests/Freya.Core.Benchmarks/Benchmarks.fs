namespace Freya.Core.Benchmarks

open BenchmarkDotNet.Attributes
open Hopac
open Freya.Core
open Freya.Core.Operators
open Freya.Core.Optics

[<Config(typeof<CoreConfig>)>]
type RunFreya () =
    let key_ =
        State.value_ "key"

    [<Benchmark>]
    member x.Freya () =
        let initialState =
            let newDictionary = System.Collections.Generic.Dictionary<_,_>() :> Environment
            State.create newDictionary
        let m = freya {
            let! v1 = Freya.Optic.get key_
            do! Freya.Optic.set key_ (Some (42UL :: (v1 |> Option.defaultValue [])))
            do! Freya.Optic.map key_ (Option.map (List.map ((*) 2UL)))
            let! v2 = Freya.Optic.get key_
            return v1, v2
        }
        m initialState
        |> Hopac.run

[<Config(typeof<CoreConfig>)>]
type HandleOwinMidFunc () =
    let key_ =
        State.value_ "key"

    let myFreya = freya {
        let! r = Freya.Optic.get key_
        do! Freya.Optic.set key_ (Some 42UL)
        let! _ = Freya.Optic.get key_
        return Halt
    }

    let next : OwinAppFunc = OwinAppFunc (fun e -> System.Threading.Tasks.Task.CompletedTask)
    let omf : OwinMidFunc = OwinMidFunc.ofFreya myFreya

    [<Benchmark>]
    member __.OwinMidFunc () =
        let env = System.Collections.Generic.Dictionary<_,_>()
        omf.Invoke(next).Invoke(env).Wait()
