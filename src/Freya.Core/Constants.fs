[<RequireQualifiedAccess>]
module Freya.Core.Constants

#nowarn "3190"

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
