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

                static member inline Freya (xF: Freya<'x>) =
                    xF

                static member inline Freya (_: unit) =
                    fun s ->
#if HOPAC
                        Hopac.Job.result ((), s)
#else
                        async.Return ((), s)
#endif

            let inline defaults (a: ^a, _: ^b) =
                    ((^a or ^b) : (static member Freya: ^a -> Freya<_>) a)

            let inline infer (x: 'a) =
                defaults (x, Defaults)

        /// A function to return a Freya function given an instance of a type
        /// which has a suitable static Freya method. An existing Freya
        /// function will be returned as-is.

        let inline infer x =
            Inference.infer x
