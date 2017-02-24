namespace Freya.Core

// Pipeline

// The basic high-level composition/chaining abstraction for Freya based
// systems.

// Types

/// The basic Pipeline type, a secondary abstraction for composing Freya
/// functions of more specific type (using a short-circuiting approach to
/// ending computation early based on return value).

type Pipeline =
    Freya<PipelineChoice>

/// The return type for the Pipeline function, indicating whether the function
/// should continue with the next function in a chain of Pipelines or halt.

 and PipelineChoice =
    | Next
    | Halt

// Pipeline

/// Basic functions and shorthand for working with Pipelines, including simple
/// next and halt literals, and a type inferring compose method for any two
/// things which may be inferred as pipelines.

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Pipeline =

    // Functions

    // Simple shorthand convenience instances of a Pipeline returning Next
    // and a Pipeline returning halt. Additionally a composition function which
    // will infer any two values which support Pipeline static inference is
    // defined.

    /// An instance of a Pipeline which will always return Next.

    let next : Pipeline =
        Freya.init Next

    /// An instance of a Pipleine which will always return Halt.

    let halt : Pipeline =
        Freya.init Halt

    // Inference

    /// Inference for a Pipeline type, where a Pipeline is returned with no
    /// modification, PipelineChoice is lifted to Pipeline, and a Freya<'a>
    /// function becomes a Pipeline, discarding the original result,
    /// effectively implemented as monadic combine.

    [<RequireQualifiedAccess>]
    module Inference =

        type Defaults =
            | Defaults

            static member Pipeline (x: Pipeline) =
                x

            static member Pipeline (x: PipelineChoice) : Pipeline =
                Freya.init x

            static member Pipeline (x: Freya<_>) : Pipeline =
                Freya.map2 (fun _ x -> x) x next

        let inline defaults (a: ^a, _: ^b) =
                ((^a or ^b) : (static member Pipeline: ^a -> Pipeline) a)

        let inline infer (x: 'a) =
            defaults (x, Defaults)

    /// A function to return a Pipeline function given an instance of a type
    /// which either has a suitable static Pipeline method, or which is of type
    /// Pipeline, PipelineChoice, or Freya<_> (the Freya<_> function will be
    /// returned as a Pipeline which will always return Next).

    let inline infer x =
        Inference.infer x

    /// A function to compose two functions which may be - or may be inferred
    /// to be (see Pipeline.infer) - Pipeline functions, given the composition
    /// approach of executing functions sequentially until one returns Halt.

    /// In this case, the first function will always be executed, and if the
    /// result is Next, the second pipeline will be executed and the result of
    /// the second pipeline returned. Where the result of the first Pipeline is
    /// Halt, the second pipeline will never be executed, and the Halt result
    /// will be returned.

    let inline compose p1 p2 : Pipeline =
        infer p1
        |> Freya.bind (function | Next -> infer p2
                                | _ -> halt)
