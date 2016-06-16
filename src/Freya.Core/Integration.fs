namespace Freya.Core

open System
open System.Threading.Tasks

(* Integration

   Utility functionality for integrating the Freya model of computation with
   wider standards, in this case generally OWIN compatible servers and tools
   through the use of adapter functions from specification signatures to
   Freya signatures and vice versa. *)

(* Types

   Common type aliases and shorthand for working with OWIN systems, giving a
   more appropriate grammar when dealing with standards compliant software. *)

type OwinEnvironment =
    Freya.Core.Environment

type OwinAppFunc = 
    Func<OwinEnvironment, Task>

type OwinMidFunc =
    Func<OwinAppFunc, OwinAppFunc>

(* OwinAppFunc

   Functions for mapping to/from the OwinAppFunc signature given Freya
   functions, using static inference to allow any type which implements
   the appropriate member function to be used. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module OwinAppFunc =

    [<CompiledName ("FromFreya")>]
    let inline ofFreya freya =
        
        let freya =
            Freya.infer freya

        OwinAppFunc (fun e ->
            Async.StartAsTask (
                Async.Ignore (freya (State.create e))) :> Task)