namespace Freya.Core

#if Hopac

open Hopac

#endif

(* Inference *)

[<AutoOpen>]
module Inference =

    [<RequireQualifiedAccess>]
    module Freya =

        (* Inference *)

        [<RequireQualifiedAccess>]
        module Inference =

            type Defaults =
                | Defaults

                static member Freya (x: Freya<_>) =
                    x

#if Hopac

                static member inline Freya (_: unit) =
                    fun s ->
                        Job.result ((), s)

#else

                static member inline Freya (_: unit) =
                    fun s ->
                        async.Return ((), s)

#endif

            let inline defaults (a: ^a, _: ^b) =
                    ((^a or ^b) : (static member Freya: ^a -> Freya<_>) a)

            let inline infer (x: 'a) =
                defaults (x, Defaults)

        let inline infer x =
            Inference.infer x