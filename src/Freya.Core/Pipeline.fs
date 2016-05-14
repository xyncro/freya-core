namespace Freya.Core

(* Pipeline

   The basic high-level composition/chaining abstraction for Freya based
   systems. *)

(* Types

   The basic Pipeline type, a secondary abstraction for composing Freya
   functions of more specific type (using a shrt-circuiting approach to ending
   computation early based on return value). *)

type Pipeline =
    Freya<PipelineChoice>

 and PipelineChoice =
    | Next
    | Halt

(* Pipeline

   Basic functions and shorthand for working with Pipelines, including simple
   next and halt literals, and a type inferring compose method for any two
   things which may be inferred as pipelines. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Pipeline =

    (* Inference

       Inference for a Pipeline type, where a Pipeline is returned with no
       modification, PipelineChoice is lifted to Pipeline, and a Freya<'a>
       function becomes a Pipeline, discarding the original result,
       effectively implemented as monadic combine. *)

    [<RequireQualifiedAccess>]
    module Inference =

        type Defaults =
            | Defaults

            static member Pipeline (x: Pipeline) =
                x

            static member Pipeline (x: PipelineChoice) : Pipeline =
                Freya.init x

            static member Pipeline (x: Freya<_>) : Pipeline =
                Freya.map2 ((fun _ x -> x), x, Freya.init Next)

        let inline defaults (a: ^a, _: ^b) =
                ((^a or ^b) : (static member Pipeline: ^a -> Pipeline) a)

        let inline infer (x: 'a) =
            defaults (x, Defaults)

    let inline infer x =
        Inference.infer x

    (* Functions

       Simple shorthand convenience instances of a Pipeline returning Next
       and a Pipeline returning halt. Additionally a composition function which
       will infer any two values which support Pipeline static inference is
       defined. *)

    let next : Pipeline =
        Freya.init Next

    let halt : Pipeline =
        Freya.init Halt

    let inline compose p1 p2 : Pipeline =
        Freya.bind (infer p1, (function | Next -> infer p2
                                        | _ -> halt))