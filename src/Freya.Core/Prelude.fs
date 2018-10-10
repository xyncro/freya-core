namespace Freya.Core

open System.Collections.Generic
open Aether

// Prelude

/// Generically useful functions and types for working with Freya relevant data
/// structures and approaches, particularly around the prevalent use of optics
/// throughout the Freya codebase.

[<AutoOpen>]
module Common =

    // Comparison/Equality

    // Functions for simplifying the customization of comparison and equality
    // on types where this is required.

    /// A general comparison function to simplify implementing comparable
    /// interfaces.

    let compareOn f x (y: obj) =
        match y with
        | :? 'a as y -> compare (f x) (f y)
        | _ -> invalidArg "y" "Cannot compare values of different types."

    /// A general equality function to simplify implementing equatable
    /// interfaces.

    let equalsOn f x (y: obj) =
        match y with
        | :? 'a as y -> (f x) = (f y)
        | _ -> false

    let hashOn f x =
        hash (f x)

    // Glue

    // Commonly used glue functions for shortening code.

    /// Simple functional flip, reversing the argument order of a function
    /// taking two arguments.

    let flip f a b =
        f b a

    /// Simple tuple, creating a new tuple given two arguments.

    let tuple a b =
        a, b

// Dictionary

/// Extensions and functions for working with generic dictionaries,
/// significantly through optics (providing a degenerate but useful lens which
/// mutates state).

[<RequireQualifiedAccess>]
module Dict =

    /// A prism to a possible value within a dictionary given a key which may
    /// be present within the dictionary.

    let key_<'k,'v when 'v: null> k : Prism<IDictionary<'k,'v>,'v> =
        (fun d ->
            match d.TryGetValue k with
            | false, _ | true, null -> None
            | true, v -> Some v),
        (fun v d ->
            match d.ContainsKey k with
            | true -> d.[k] <- v; d
            | _ -> d)

    /// A lens to a possible value option within a dictionary given a key which
    /// may be present within a dictionary.

    /// When working with this lens as an optic, the Some and None cases of
    /// optic carry semantic meaning, where Some indicates that the value is or
    /// should be present within the dictionary, and None indicates that the
    /// value is not, or should not be present within the dictionary.

    let value_<'k,'v when 'v: null> k : Lens<IDictionary<'k,'v>,'v option> =
        (fun d ->
            match d.TryGetValue k with
            | false, _ | true, null -> None
            | true, v -> Some v),
        (fun v d ->
            match v with
            | Some v -> d.[k] <- v; d
            | _ -> d)

// Option

/// Extensions and functions for working with Option types, significantly
/// through optics, providing functions for mapping existing optics to the value
/// where possible.

[<RequireQualifiedAccess>]
module Option =

    let mapEpimorphism (e: Epimorphism<'a,'b>) : Isomorphism<'a option,'b option> =
        Option.bind (fst e), Option.map (snd e)

    let mapIsomorphism (i: Isomorphism<'a,'b>) : Isomorphism<'a option, 'b option> =
        Option.map (fst i), Option.map (snd i)

    let mapLens (l: Lens<'a,'b>) : Prism<'a option,'b> =
        Option.map (fst l), snd l >> Option.map

    let bindLens (p : Lens<'a, 'b option>) : Lens<'a option, 'b option> =
        Option.bind (fst p), snd p >> Option.map

// Constants

/// Literal constants for the values of keys within the OWIN environment,
/// provided here as being generically useful to any code needing to work
/// within the OWIN environment dictionary.

[<RequireQualifiedAccess>]
module Constants =

    // OWIN 1.1.0

    // Taken from [https://github.com/owin/owin/blob/master/spec/owin-1.1.0.md]

    // 3.2.1 Request Data

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

    // 3.2.2 Response Data

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

    // 3.2.3 Other Data

    [<CompiledName ("CallCancelled")>]
    let [<Literal>] CallCancelled = "owin.CallCancelled"

    [<CompiledName ("OwinVersion")>]
    let [<Literal>] OwinVersion = "owin.Version"

    // http://owin.org/spec/CommonKeys.html

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

    // SendFile Extensions

    // See [http://owin.org/extensions/owin-SendFile-Extension-v0.3.0.htm]

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module SendFiles =

        // 3.1. Startup

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "sendfile.Version"

        [<CompiledName ("Support")>]
        let [<Literal>] Support = "sendfile.Support"

        [<CompiledName ("Concurrency")>]
        let [<Literal>] Concurrency = "sendfile.Concurrency"

        // 3.2. Per Request

        [<CompiledName ("SendAsync")>]
        let [<Literal>] SendAsync = "sendfile.SendAsync"

    // Opaque Stream Extensions

    // See [http://owin.org/extensions/owin-OpaqueStream-Extension-v0.3.0.htm]

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Opaque =

        // 3.1. Startup

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "opaque.Version"

        // 3.2. Per Request

        [<CompiledName ("Upgrade")>]
        let [<Literal>] Upgrade = "opaque.Upgrade"

        // 5. Consumption

        [<CompiledName ("Stream")>]
        let [<Literal>] Stream = "opaque.Stream"

        [<CompiledName ("CallCancelled")>]
        let [<Literal>] CallCancelled = "opaque.CallCancelled"

    // Web Socket Extensions

    // See [http://owin.org/extensions/owin-WebSocket-Extension-v0.4.0.htm]

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module WebSocket =

        // 3.1. Startup

        [<CompiledName ("Version")>]
        let [<Literal>] Version = "websocket.Version"

        // 3.2. Per Request

        [<CompiledName ("Accept")>]
        let [<Literal>] Accept = "websocket.Accept"

        // 4. Accept

        [<CompiledName ("SubProtocol")>]
        let [<Literal>] SubProtocol = "websocket.SubProtocol"

        // 5. Consumption

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
