[<AutoOpen>]
module internal Prelude

open System.Reflection
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Analysers
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Jobs
//open BenchmarkDotNet.Diagnostics.Windows
open BenchmarkDotNet.Validators

type Dummy = Dummy

type CoreConfig() =
    inherit ManualConfig()
    do
        base.Add(Job.LongRun.WithGcServer(true).WithGcConcurrent(true).WithUnrollFactor(256))
        base.Add(EnvironmentAnalyser.Default)
        base.Add(MemoryDiagnoser.Default)
        base.Add(BaselineValidator.FailOnError)
        base.Add(JitOptimizationsValidator.FailOnError)
        base.KeepBenchmarkFiles <- true

let thisAssembly = typeof<Dummy>.GetTypeInfo().Assembly
