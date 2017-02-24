/// Optic based access to the Freya computation state, analogous to the
/// Optic.* functions exposed by Aether, but working within a Freya function
/// and therefore part of the Freya ecosystem.
namespace Freya.Core.Optics

open Freya.Core
#if HOPAC
open Hopac
#endif

[<RequireQualifiedAccess>]
module Freya =

    /// Optic based access to the Freya computation state, analogous to the
    /// Optic.* functions exposed by Aether, but working within a Freya function
    /// and therefore part of the Freya ecosystem.
    [<RequireQualifiedAccess>]
    module Optic =

        /// A function to get a value within the current computation State
        /// given an optic from State to the required value.
        let inline get o : Freya<'a> =
#if HOPAC
            Job.lift (fun s -> FreyaResult.create (Aether.Optic.get o s) s)
#else
            fun s ->
                async.Return (FreyaResult.create (Aether.Optic.get o s) s)
#endif

        /// A function to set a value within the current computation State
        /// given an optic from State to the required value and an instance of
        /// the required value.
        let inline set o v : Freya<unit> =
#if HOPAC
            Job.lift (fun (s: State) -> FreyaResult.create () (Aether.Optic.set o v s))
#else
            fun (s: State) ->
                async.Return (FreyaResult.create () (Aether.Optic.set o v s))
#endif

        /// A function to map a value within the current computation State
        /// given an optic from the State the required value and a function
        /// from the current value to the new value (a homomorphism).
        let inline map o f : Freya<unit> =
#if HOPAC
            Job.lift (fun (s: State) -> FreyaResult.create () (Aether.Optic.map o f s))
#else
            fun (s: State) ->
                async.Return (FreyaResult.create () (Aether.Optic.map o f s))
#endif
