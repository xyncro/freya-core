namespace Freya.Core

// Inference

[<AutoOpen>]
module Inference =

    /// Functions for inferring Freya types from suitable types which possess
    /// appropriate type signatures.

    [<RequireQualifiedAccess>]
    module Freya =

        // Inference

        [<RequireQualifiedAccess>]
        module Inference =

            type Defaults =
                | Defaults

                static member inline Freya (xF: Freya<_>) =
                    xF

#if HOPAC
                static member inline Freya (xJ: Hopac.Job<_>) =
                    Freya.fromJob xJ
#endif

                static member inline Freya (xA: Async<_>) =
                    Freya.fromAsync xA

                static member inline Freya (_: unit) =
                    Freya.empty

            let inline defaults (a: ^a, _: ^b) =
                    ((^a or ^b) : (static member Freya: ^a -> Freya<_>) a)

            let inline infer (x: 'a) =
                defaults (x, Defaults)

        /// A function to return a Freya function given an instance of a type
        /// which has a suitable static Freya method. An existing Freya
        /// function will be returned as-is.

        let inline infer x =
            Inference.infer x
