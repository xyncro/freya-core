module Main

open BenchmarkDotNet.Configs
open BenchmarkDotNet.Analysers
open BenchmarkDotNet.Diagnosers
//open BenchmarkDotNet.Diagnostics.Windows
open BenchmarkDotNet.Validators
open BenchmarkDotNet.Running

[<EntryPoint>]
let main argv =
    // let switcher = BenchmarkSwitcher thisAssembly
    // let _ = switcher.Run argv
    let _ = BenchmarkRunner.Run<Freya.Core.Benchmarks.HandleOwinMidFunc>()
    0
