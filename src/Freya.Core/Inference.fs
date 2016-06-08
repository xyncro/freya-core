namespace Freya.Core

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

                static member Freya (_: unit) =
                    fun s -> async { return (), s }

            let inline defaults (a: ^a, _: ^b) =
                    ((^a or ^b) : (static member Freya: ^a -> Freya<_>) a)

            let inline infer (x: 'a) =
                defaults (x, Defaults)

        let inline infer x =
            Inference.infer x