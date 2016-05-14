namespace Freya.Core

(* Configuration

   A simple basic computation expression builder specialized for minimal
   development and a simplistic approach to maintaining a threaded instance of
   some configuration state. This simple builder type is used as the basis for
   the various custom expressions used in Routers, Machines, etc. *)

(* Types

   The basic ConfigurationBuilder type and the operations type specifiying the
   init and bind functions which must be supplied to implement the builder. *)

type ConfigurationBuilder<'c> (operations: ConfigurationBuilderOperations<'c>) =

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

 and ConfigurationBuilderOperations<'c> =
    { Init: unit -> 'c
      Bind: ('c * (unit -> 'c)) -> 'c }

(* Operations

   Custom operations with syntax, currently providing a method for nesting
   expressions, giving modularity through the use of the "including" syntax
   available in all derived computation expressions. *)

type ConfigurationBuilder<'c> with

    [<CustomOperation ("including", MaintainsVariableSpaceUsingBind = true)>]
    member x.Including (m, configuration) = 
        x.Combine (m, configuration)