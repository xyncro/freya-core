namespace Freya.Core

open System
open Aether

// Obsolete

// Backwards compatibility shims to make the 2.x-> 3.x transition
// less painful, providing functionally equivalent options where possible.

// To be removed for 4.x releases.

[<AutoOpen>]
module Obsolete =

    // Freya

    [<RequireQualifiedAccess>]
    module Freya =

        // Pipeline

        [<Obsolete ("Use Pipeline.next instead.")>]
        let next =
            Pipeline.next

        [<Obsolete ("Use Pipeline.halt instead.")>]
        let halt =
            Pipeline.halt

        [<Obsolete ("Use Pipeline.compose instead.")>]
        let inline pipe p1 p2 =
            Pipeline.compose p1 p2

        // Unqualified State

        [<Obsolete ("Use Freya.Optic.get id_ instead.")>]
        let getState : Freya<State> =
            Freya.Optic.get id_

        [<Obsolete ("Use Freya.Optic.set id_ instead.")>]
        let setState : State -> Freya<unit> =
            Freya.Optic.set id_

        [<Obsolete ("Use Freya.Optic.map id_ instead.")>]
        let mapState : (State -> State) -> Freya<unit> =
            Freya.Optic.map id_

        // Unqualified Lens

        [<Obsolete ("Use Freya.Optic.get instead.")>]
        let inline getLens l =
            Freya.Optic.get l

        [<Obsolete ("Use Freya.Optic.get instead.")>]
        let inline getLensPartial l =
            Freya.Optic.get l

        [<Obsolete ("Use Freya.Optic.set instead.")>]
        let inline setLens l b =
            Freya.Optic.set l b

        [<Obsolete ("Use Freya.Optic.set instead.")>]
        let inline setLensPartial l b =
            Freya.Optic.set l b

        [<Obsolete ("Use Freya.Optic.map instead.")>]
        let inline mapLens l f =
            Freya.Optic.map l f

        [<Obsolete ("Use Freya.Optic.map instead.")>]
        let inline mapLensPartial l f =
            Freya.Optic.map l f

        // Qualified Lens

        [<RequireQualifiedAccess>]
        [<Obsolete ("Use Freya.Optic module functions instead.")>]
        module Lens =

            [<Obsolete ("Use Freya.Optic.get instead.")>]
            let inline get l =
                Freya.Optic.get l

            [<Obsolete ("Use Freya.Optic.get instead.")>]
            let inline getPartial l =
                Freya.Optic.get l

            [<Obsolete ("Use Freya.Optic.set instead.")>]
            let inline set l b =
                Freya.Optic.set l b

            [<Obsolete ("Use Freya.Optic.set instead.")>]
            let inline setPartial l b =
                Freya.Optic.set l b

            [<Obsolete ("Use Freya.Optic.map instead.")>]
            let inline map l f =
                Freya.Optic.map l f

            [<Obsolete ("Use Freya.Optic.map instead.")>]
            let inline mapPartial l f =
                Freya.Optic.map l f

        // Qualified State

        [<RequireQualifiedAccess>]
        [<Obsolete ("Use Freya.Optic module functions instead.")>]
        module State =

            [<Obsolete ("Use Freya.Optic.get id_ instead.")>]
            let get : Freya<State> =
                Freya.Optic.get id_

            [<Obsolete ("Use Freya.Optic.set id_ instead.")>]
            let set : State -> Freya<unit> =
                Freya.Optic.set id_

            [<Obsolete ("Use Freya.Optic.map id_ instead.")>]
            let map : (State -> State) -> Freya<unit> =
                Freya.Optic.map id_
