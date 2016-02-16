module Freya.Core

open System
open System.Collections.Generic
open System.Threading.Tasks
open Aether
open Aether.Operators

// TODO: Documentation
// TODO: Integration functions
// TODO: Recording? Decide on inclusion in Core
// TODO: Testing - fuller coverage

(* Prelude

   Generically useful functions and types for working with Freya relevant data
   structures and approaches, particularly around the prevalent use of optics
   throughout the Freya codebase. *)

[<AutoOpen>]
module Prelude =

    (* Comparison/Equality

       Functions for simplifying the customization of comparison and equality
       on types where this is required. *)

    let compareOn f x (y: obj) =
        match y with
        | :? 'a as y -> compare (f x) (f y)
        | _ -> invalidArg "y" "Cannot compare values of different types."

    let equalsOn f x (y: obj) =
        match y with
        | :? 'a as y -> (f x) = (f y)
        | _ -> false

    let hashOn f x =
        hash (f x)

    (* Common

       Commonly used glue functions for shortening code. *)

    let flip f a b =
        f b a

    let tuple a b =
        a, b

    (* Dictionary

       Extensions and functions for working with generic dictionaries,
       significantly through optics (providing a degenerate but useful lens
       which mutates state). *)

    [<RequireQualifiedAccess>]
    module Dict =

        let value_<'k,'v when 'v: null> k : Lens<IDictionary<'k,'v>,'v option> =
            (fun d ->
                match d.TryGetValue k with
                | false, _ | true, null -> None
                | true, v -> Some v),
            (fun v d ->
                match v with
                | Some v -> d.[k] <- v; d
                | _ -> d)

    (* Option

       Extensions and functions for working with Option types, significantly
       through optics, providing functions for mapping existing optics to the
       value where possible. *)

    [<RequireQualifiedAccess>]
    module Option =

        let mapEpimorphism (e: Epimorphism<'a,'b>) : Isomorphism<'a option,'b option> =
            Option.bind (fst e), Option.map (snd e)

        let mapIsomorphism (i: Isomorphism<'a,'b>) : Isomorphism<'a option, 'b option> =
            Option.map (fst i), Option.map (snd i)

        let mapLens (l: Lens<'a,'b>) : Prism<'a option,'b> =
            Option.map (fst l), snd l >> Option.map

(* Types

   Core types within the Freya codebase, representing the basic units of
   execution and composition, including the core async state carrying
   abstraction and a short-circuiting pipeline abstraction. *)

type Freya<'a> =
    State -> Async<'a * State>

 and State =
    { Environment: Environment
      Meta: MetaState }

    static member internal environment_ =
        (fun x -> x.Environment), 
        (fun e x -> { x with Environment = e })

    static member internal meta_ =
        (fun x -> x.Meta), 
        (fun m x -> { x with Meta = m })

    static member empty e =
        { Environment = e
          Meta = MetaState.empty }

 and Environment =
    IDictionary<string, obj>

 and MetaState =
    { Memos: Map<Guid, obj> }

    static member internal memos_ =
        (fun x -> x.Memos),
        (fun m x -> { x with Memos = m })

    static member empty =
        { Memos = Map.empty }

type Pipeline =
    Freya<PipelineChoice>

 and PipelineChoice =
    | Next
    | Halt

(* State

   Optics for working with elements of the State type, providing deep
   access within the overall data structure. Specifically, values within the
   environment, and memos within the metastate are made available through
   suitable lenses. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module State =

    let value_<'a> k =
            State.environment_
        >-> Dict.value_<string,obj> k
        >-> Option.mapIsomorphism box_<'a>

    let memo_<'a> i =
            State.meta_
        >-> MetaState.memos_
        >-> Map.value_ i
        >-> Option.mapIsomorphism box_<'a>

(* Freya

   Functions for working with the core abstractions of Freya, specifically
   common monadic operations with the async state abstraction, including
   memoization, etc., and sets of namespaced functions for working with the
   state directly and through optics. *)

[<RequireQualifiedAccess>]
module Freya =

    (* Common *)

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
            async.Bind (f () s, fun (a, s) ->
                async.Return (a, s))

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

    (* Basic *)

    let empty : Freya<unit> =
        init ()

    (* Extended *)

    let fromAsync (a: 'a, f: 'a -> Async<'b>) : Freya<'b> =
        fun s ->
            async.Bind (f a, fun b ->
                async.Return (b, s))
                    
    let map2 (f: 'a -> 'b -> 'c, m1: Freya<'a>, m2: Freya<'b>) : Freya<'c> =
        fun s ->
            async.Bind (m1 s, fun (a, s) ->
                async.Bind (m2 s, fun (b, s) ->
                    async.Return (f a b, s)))

    let memo<'a> (m: Freya<'a>) : Freya<'a> =
        let memo_ = State.memo_<'a> (Guid.NewGuid ())

        fun s ->
            match Optic.get memo_ s with
            | Some memo ->
                async.Return (memo, s)
            | _ ->
                async.Bind (m s, fun (memo, s) ->
                    async.Return (memo, Optic.set memo_ (Some memo) s))

    (* State

       Functions for working directly with the State within a Freya<'a>
       function, the async state abstraction. *)

    [<RequireQualifiedAccess>]
    module State =

        let get : Freya<State> =
            fun s ->
                async.Return (s, s)

        let set : State -> Freya<unit> =
            fun s ->
                fun _ ->
                    async.Return ((), s)

        let map : (State -> State) -> Freya<unit> =
            fun f ->
                fun s ->
                    async.Return ((), f s)

    (* Optics

       Functions for working with the State within a Freya<'a> function using
       optics to work with the data structure in more complex ways. *)

    [<RequireQualifiedAccess>]
    module Optic =

        let inline get o =
            map (State.get, Optic.get o)

        let inline set o v =
            State.map (Optic.set o v)

        let inline map o f =
            State.map (Optic.map o f)

(* Configuration

   Components for building simple configuration builders, used to provide more
   abstract syntax across Freya, particularly in Routers, Machines, etc. The
   simple builder is supplied with an operations type defining how init
   and bind are applied to the parameterised type of the builder. *)

[<RequireQualifiedAccess>]
module Configuration =

    type Operations<'c> =
        { Init: unit -> 'c
          Bind: ('c * (unit -> 'c)) -> 'c }

    (* Builder *)

    type Builder<'c> (operations: Operations<'c>) =

        member __.Return _ : 'c =
            operations.Init ()

        member __.ReturnFrom (c:  'c) : 'c =
            c

        member __.Bind (m: 'c, f: unit -> 'c) : 'c =
            operations.Bind (m, f)

        member __.Combine (m1: 'c, m2: 'c) : 'c =
            operations.Bind (m1, (fun () -> m2))

        member __.Zero () : 'c =
            operations.Init ()

    (* Syntax *)

    type Builder<'c> with

        [<CustomOperation ("including", MaintainsVariableSpaceUsingBind = true)>]
        member x.Including (m, configuration) = 
            x.Combine (m, configuration)

(* Inference

   Pseudo-Typeclass based inference of various types, automatically converting
   values of these of types to a specific known core type, allowing APIs to be
   more concise where appropriate. *)

[<RequireQualifiedAccess>]
module Infer =

    (* Freya

       Inference for a Freya type (no conversion) and unit which is converted
       to a Freya function returning unit. *)

    [<RequireQualifiedAccess>]
    module Freya =

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

    let inline freya x =
        Freya.infer x

    (* Pipeline

       Inference for a Pipeline type, where a Pipeline is returned with no
       modification, PipelineChoice is lifted to Pipeline, and a Freya<'a>
       function becomes a Pipeline, discarding the original result,
       effectively monadic combine. *)

    [<RequireQualifiedAccess>]
    module Pipeline =

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

    let inline pipeline x =
        Pipeline.infer x

(* Pipeline

   Basic functions and shorthand for working with Pipelines, including simple
   next and halt literals, and a type inferring compose method for any two
   things which may be inferred as pipelines. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Pipeline =

    let next : Pipeline =
        Freya.init Next

    let halt : Pipeline =
        Freya.init Halt

    let inline compose p1 p2 : Pipeline =
        Freya.bind (Infer.pipeline p1, (function | Next -> Infer.pipeline p2
                                                 | _ -> halt))

(* Integration *)

[<AutoOpen>]
module Integration =

    (* Types *)

    type OwinEnvironment =
        Environment

    type OwinAppFunc = 
        Func<OwinEnvironment, Task>

    type OwinMidFunc =
        Func<OwinAppFunc, OwinAppFunc>

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
    module OwinAppFunc =

        [<CompiledName ("FromFreya")>]
        let inline ofFreya freya =
            let freya = Infer.freya freya

            OwinAppFunc (fun e ->
                Async.StartAsTask (
                    Async.Ignore (freya (State.empty e))) :> Task)

(* Builders

   Computation expression builders for the common Freya function, providing
   an alternative syntax for those who wish to avoid monadic composition and/or
   symbolic operators. *)

type FreyaBuilder () =

    member __.Bind (m: Freya<'a>, f: 'a -> Freya<'b>) : Freya<'b> =
        Freya.bind (m, f)

    member __.Delay (f: unit -> Freya<'a>) : Freya<'a> =
        Freya.delay (f)

    member __.Return (a: 'a) : Freya<'a> =
        Freya.init (a)

    member __.ReturnFrom (m: Freya<'a>) : Freya<'a> =
        Freya.initFrom (m)

    member __.Combine (m1: Freya<_>, m2: Freya<'a>) : Freya<'a> =
        Freya.combine (m1, m2)

    member __.Zero () : Freya<unit> =
        Freya.zero ()

let freya =
    FreyaBuilder ()

(* Operators

   Symbolic operators for commonly used functions around core Freya
   functionality, particularly monadic operations, including monadic optic
   operations. *)

module Operators =

    (* Common *)

    let inline (<*>) f m =
        Freya.apply (m, f)

    let inline (>>=) m f =
        Freya.bind (m, f)

    let inline (=<<) f m =
        Freya.bind (m, f)

    let inline (>=>) m1 m2 =
        fun x -> Freya.bind (m1 x, m2)

    let inline (<=<) m1 m2 =
        fun x -> Freya.bind (m2 x, m1)

    let inline ( *>) m1 m2 =
        Freya.combine (m1, m2)

    let inline ( <*) m1 m2 =
        Freya.combine (m2, m1)

    let inline (<!>) f m =
        Freya.map (m, f)

    (* Optic *)

    let inline (!.) o =
        Freya.Optic.get o

    let inline (.=) o v =
        Freya.Optic.set o v

    let inline (%=) o f =
        Freya.Optic.map o f

    (* Pipeline *)

    let inline (>?=) p1 p2 =
        Pipeline.compose p1 p2

(* Constants

   Literal constants for the values of keys within the OWIN environment,
   provided here as being generically useful to any code needing to work
   within the OWIN environment dictionary. *)

[<RequireQualifiedAccess>]
module Constants =

    (* OWIN 1.1.0

       Taken from [https://github.com/owin/owin/blob/master/spec/owin-1.1.0.md] *)

    (* 3.2.1 Request Data *)

    [<CompiledName ("RequestScheme")>]
    let [<Literal>] RequestScheme = "owin.RequestScheme"

    [<CompiledName ("RequestMethod")>]
    let [<Literal>] RequestMethod = "owin.RequestMethod"

    [<CompiledName ("RequestPathBase")>]
    let [<Literal>] RequestPathBase = "owin.RequestPathBase"

    [<CompiledName ("RequestPath")>]
    let [<Literal>] RequestPath = "owin.RequestPath"

    [<CompiledName ("RequestQueryString")>]
    let [<Literal>] RequestQueryString = "owin.RequestQueryString"

    [<CompiledName ("RequestProtocol")>]
    let [<Literal>] RequestProtocol = "owin.RequestProtocol"

    [<CompiledName ("RequestHeaders")>]
    let [<Literal>] RequestHeaders = "owin.RequestHeaders"

    [<CompiledName ("RequestBody")>]
    let [<Literal>] RequestBody = "owin.RequestBody"

    [<CompiledName ("RequestId")>]
    let [<Literal>] RequestId = "owin.RequestId"

    [<CompiledName ("RequestUser")>]
    let [<Literal>] RequestUser = "owin.RequestUser"

    (* 3.2.2 Response Data *)

    [<CompiledName ("ResponseStatusCode")>]
    let [<Literal>] ResponseStatusCode = "owin.ResponseStatusCode"

    [<CompiledName ("ResponseReasonPhrase")>]
    let [<Literal>] ResponseReasonPhrase = "owin.ResponseReasonPhrase"

    [<CompiledName ("ResponseProtocol")>]
    let [<Literal>] ResponseProtocol = "owin.ResponseProtocol"

    [<CompiledName ("ResponseHeaders")>]
    let [<Literal>] ResponseHeaders = "owin.ResponseHeaders"

    [<CompiledName ("ResponseBody")>]
    let [<Literal>] ResponseBody = "owin.ResponseBody"

    (* 3.2.3 Other Data *)

    [<CompiledName ("CallCancelled")>]
    let [<Literal>] CallCancelled = "owin.CallCancelled"

    [<CompiledName ("OwinVersion")>]
    let [<Literal>] OwinVersion = "owin.Version"

    (* http://owin.org/spec/CommonKeys.html *)

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module CommonKeys =

        [<CompiledName ("ClientCertificate")>]
        let [<Literal>] ClientCertificate = "ssl.ClientCertificate"

        [<CompiledName ("RemoteIpAddress")>]
        let [<Literal>] RemoteIpAddress = "server.RemoteIpAddress"

        [<CompiledName ("RemotePort")>]
        let [<Literal>] RemotePort = "server.RemotePort"

        [<CompiledName ("LocalIpAddress")>]
        let [<Literal>] LocalIpAddress = "server.LocalIpAddress"

        [<CompiledName ("LocalPort")>]
        let [<Literal>] LocalPort = "server.LocalPort"

        [<CompiledName ("IsLocal")>]
        let [<Literal>] IsLocal = "server.IsLocal"

        [<CompiledName ("TraceOutput")>]
        let [<Literal>] TraceOutput = "host.TraceOutput"

        [<CompiledName ("Addresses")>]
        let [<Literal>] Addresses = "host.Addresses"

        [<CompiledName ("Capabilities")>]
        let [<Literal>] Capabilities = "server.Capabilities"

        [<CompiledName ("OnSendingHeaders")>]
        let [<Literal>] OnSendingHeaders = "server.OnSendingHeaders"

        [<CompiledName ("ServerName")>]
        let [<Literal>] ServerName = "server.Name"

    (* SendFile Extensions
       See [http://owin.org/extensions/owin-SendFile-Extension-v0.3.0.htm] *)

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module SendFiles =

        (* 3.1. Startup *)

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "sendfile.Version"

        [<CompiledName ("Support")>]
        let [<Literal>] Support = "sendfile.Support"

        [<CompiledName ("Concurrency")>]
        let [<Literal>] Concurrency = "sendfile.Concurrency"

        (* 3.2. Per Request *)

        [<CompiledName ("SendAsync")>]
        let [<Literal>] SendAsync = "sendfile.SendAsync"

    (* Opaque Stream Extensions
       See [http://owin.org/extensions/owin-OpaqueStream-Extension-v0.3.0.htm] *)

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Opaque =

        (* 3.1. Startup *)

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "opaque.Version"

        (* 3.2. Per Request *)

        [<CompiledName ("Upgrade")>]
        let [<Literal>] Upgrade = "opaque.Upgrade"

        (* 5. Consumption *)

        [<CompiledName ("Stream")>]
        let [<Literal>] Stream = "opaque.Stream"

        [<CompiledName ("CallCancelled")>]
        let [<Literal>] CallCancelled = "opaque.CallCancelled"

    (* Web Socket Extensions
       See [http://owin.org/extensions/owin-WebSocket-Extension-v0.4.0.htm] *)

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module WebSocket =

        (* 3.1. Startup *)

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "websocket.Version"

        (* 3.2. Per Request *)

        [<CompiledName ("Accept")>]
        let [<Literal>] Accept = "websocket.Accept"

        (* 4. Accept *)

        [<CompiledName ("SubProtocol")>]
        let [<Literal>] SubProtocol = "websocket.SubProtocol"

        (* 5. Consumption *)

        [<CompiledName ("SendAsync")>]
        let [<Literal>] SendAsync = "websocket.SendAsync"

        [<CompiledName ("ReceiveAsync")>]
        let [<Literal>] ReceiveAsync = "websocket.ReceiveAsync"

        [<CompiledName ("CloseAsync")>]
        let [<Literal>] CloseAsync = "websocket.CloseAsync"

        [<CompiledName ("CallCancelled")>]
        let [<Literal>] CallCancelled = "websocket.CallCancelled"

        [<CompiledName ("ClientCloseStatus")>]
        let [<Literal>] ClientCloseStatus = "websocket.ClientCloseStatus"

        [<CompiledName ("ClientCloseDescription")>]
        let [<Literal>] ClientCloseDescription = "websocket.ClientCloseDescription"