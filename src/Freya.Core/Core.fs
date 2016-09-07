namespace Freya.Core

open System
open System.Collections.Generic
open Aether
open Aether.Operators

#if Hopac

open Hopac
open Hopac.Extensions

#endif

(* Core

   The common elements of all Freya based systems, namely the basic abstraction
   of an async state function over an OWIN environment, and tools for working
   with the environment in a functional and idiomatic way. *)

(* Types

   Core types within the Freya codebase, representing the basic units of
   execution and composition, including the core async state carrying
   abstraction. *)

#if Hopac

type Freya<'a> =
    State -> Job<'a * State>

#else

type Freya<'a> =
    State -> Async<'a * State>

#endif

 and State =
    { Environment: Environment
      Meta: Meta }

    static member internal environment_ =
        (fun x -> x.Environment), 
        (fun e x -> { x with Environment = e })

    static member internal meta_ =
        (fun x -> x.Meta), 
        (fun m x -> { x with Meta = m })

    static member create =
        fun environment ->
            { Environment = environment
              Meta = Meta.empty }

 and Environment =
    IDictionary<string, obj>

 and Meta =
    { Memos: Map<Guid, obj> }

    static member internal memos_ =
        (fun x -> x.Memos),
        (fun m x -> { x with Memos = m })

    static member empty =
        { Memos = Map.empty }

(* State

   Basic optics for accessing elements of the State instance within the
   current Freya function. The value_ lens is provided for keyed access
   to the OWIN dictionary, and the memo_ lens for keyed access to the
   memo storage in the Meta instance. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module State =

    (* Optics *)

    let key_<'a> k =
            State.environment_
        >-> Dict.key_<string,obj> k
        >?> box_<'a>

    let value_<'a> k =
            State.environment_
        >-> Dict.value_<string,obj> k
        >-> Option.mapIsomorphism box_<'a>

    let memo_<'a> i =
            State.meta_
        >-> Meta.memos_
        >-> Map.value_ i
        >-> Option.mapIsomorphism box_<'a>

(* Freya

   Functions and type tools for working with Freya abstractions, particularly
   data contained within the Freya state abstraction. Commonly defined
   functions for treating the Freya functions as a monad, etc. are also
   included, along with basic support for static inference. *)

[<RequireQualifiedAccess>]
module Freya =

    (* Optic

       Optic based access to the Freya computation state, analogous to the
       Optic.* functions exposed by Aether, but working within a Freya function
       and therefore part of the Freya ecosystem. *)

    [<RequireQualifiedAccess>]
    module Optic =

        (* Functions *)

#if Hopac

        let inline get o =
            fun s ->
                Job.result (Optic.get o s, s)

        let inline set o v =
            fun s ->
                Job.result ((), Optic.set o v s)

        let inline map o f =
            fun s ->
                Job.result ((), Optic.map o f s)

#else

        let inline get o =
            fun s ->
                async.Return (Optic.get o s, s)

        let inline set o v =
            fun s ->
                async.Return ((), Optic.set o v s)

        let inline map o f =
            fun s ->
                async.Return ((), Optic.map o f s)

#endif

    (* Common

       Commonly defined functions against the Freya types, particularly the
       usual monadic functions (bind, apply, etc.). These are commonly used
       directly within Freya programming but are also used within the Freya
       computation expression defined later. *)

#if Hopac

    let apply (m: Freya<'a>, f: Freya<'a -> 'b>) : Freya<'b> =
        fun s ->
            Job.bind (fun (f, s) ->
                Job.map (fun (a, s) ->
                    (f a, s)) (m s)) (f s)

    let bind (m: Freya<'a>, f: 'a -> Freya<'b>) : Freya<'b> =
        fun s ->
            Job.bind (fun (a, s) ->
                f a s) (m s)

    let combine (m1: Freya<_>, m2: Freya<'a>) : Freya<'a> =
        fun s ->
            Job.bind (fun (_, s) ->
                m2 s) (m1 s)

    let delay (f: unit -> Freya<'a>) : Freya<'a> =
        fun s ->
            f () s

    let init (a: 'a) : Freya<'a> =
        fun s ->
            Job.result (a, s)

    let initFrom (m: Freya<'a>) : Freya<'a> =
        m

    let map (m: Freya<'a>, f: 'a -> 'b) : Freya<'b> =
        fun s ->
            Job.map (fun (a, s) ->
                (f a, s)) (m s)

    let zero () : Freya<unit> =
        fun s ->
            Job.result ((), s)

#else

    let apply (m: Freya<'a>, f: Freya<'a -> 'b>) : Freya<'b> =
        fun s ->
            async.Bind (f s, fun (f, s) ->
                async.Bind (m s, fun (a, s) ->
                    async.Return (f a, s)))

    let bind (m: Freya<'a>, f: 'a -> Freya<'b>) : Freya<'b> =
        fun s ->
            async.Bind (m s, fun (a, s) ->
                async.ReturnFrom (f a s))

    let combine (m1: Freya<_>, m2: Freya<'a>) : Freya<'a> =
        fun s ->
            async.Bind (m1 s, fun (_, s) ->
                async.ReturnFrom (m2 s))

    let delay (f: unit -> Freya<'a>) : Freya<'a> =
        fun s ->
            f () s

    let init (a: 'a) : Freya<'a> =
        fun s ->
            async.Return (a, s)

    let initFrom (m: Freya<'a>) : Freya<'a> =
        m

    let map (m: Freya<'a>, f: 'a -> 'b) : Freya<'b> =
        fun s ->
            async.Bind (m s, fun (a, s') ->
                async.Return (f a, s'))

    let zero () : Freya<unit> =
        fun s ->
            async.Return ((), s)

#endif

    (* Empty

       A simple convenience instance of an empty Freya function, returning
       the unit type. This can be required for various forms of branching logic
       etc. and is a convenience to save writing Freya.init () repeatedly. *)

    let empty : Freya<unit> =
        init ()

    (* Extended

       Some extended functions providing additional convenience outside of the
       usual set of functions defined against Freya. In this case, interop with
       the basic F# async system, and extended dual map function are given. *)

#if Hopac

    let fromAsync (a: 'a, f: 'a -> Async<'b>) : Freya<'b> =
        fun s ->
            Job.map (fun b ->
                (b, s)) (Job.fromAsync (f a))

    let map2 (f: 'a -> 'b -> 'c, m1: Freya<'a>, m2: Freya<'b>) : Freya<'c> =
        fun s ->
            Job.bind (fun (a, s) ->
                Job.map (fun (b, s)->
                    (f a b, s)) (m2 s)) (m1 s)

#else

    let fromAsync (a: 'a, f: 'a -> Async<'b>) : Freya<'b> =
        fun s ->
            async.Bind (f a, fun b ->
                async.Return (b, s))
                    
    let map2 (f: 'a -> 'b -> 'c, m1: Freya<'a>, m2: Freya<'b>) : Freya<'c> =
        fun s ->
            async.Bind (m1 s, fun (a, s) ->
                async.Bind (m2 s, fun (b, s) ->
                    async.Return (f a b, s)))

#endif

    (* Memoisation

       A simple function supporting memoisation of parameterless Freya
       functions (effectively a fully applied Freya expression) which will
       cache the result of the function in the Environment instance. This
       ensures that the function will be evaluated once per request/response
       on any given thread. *)

#if Hopac

    let memo<'a> (m: Freya<'a>) : Freya<'a> =
        let memo_ = State.memo_<'a> (Guid.NewGuid ())

        fun s ->
            match Aether.Optic.get memo_ s with
            | Some memo ->
                Job.result (memo, s)
            | _ ->
                Job.map (fun (memo, s) ->
                    (memo, Aether.Optic.set memo_ (Some memo) s)) (m s)

#else

    let memo<'a> (m: Freya<'a>) : Freya<'a> =
        let memo_ = State.memo_<'a> (Guid.NewGuid ())

        fun s ->
            match Aether.Optic.get memo_ s with
            | Some memo ->
                async.Return (memo, s)
            | _ ->
                async.Bind (m s, fun (memo, s) ->
                    async.Return (memo, Aether.Optic.set memo_ (Some memo) s))

#endif