namespace Freya.Core

// Expression

// A simple computation expression for working with Freya functions as an
// alternative to the function/operator based syntax also available. A basic
// builder is defined in and an instance of the builder.

// Types

/// The FreyaBuilder type, implementing the basic underlying computation
/// expression builder required to supply Freya with computation expression
/// syntax for the core Freya<_> function type.

type FreyaBuilder () =

    member __.Bind (m: Freya<'a>, f: 'a -> Freya<'b>) : Freya<'b> =
        Freya.bind (m, f)

    member __.Delay (f: unit -> Freya<'a>) : Freya<'a> =
        Freya.delay (f)

    member __.Return (a: 'a) : Freya<'a> =
        Freya.init (a)

    member __.ReturnFrom (m: Freya<'a>) : Freya<'a> =
        Freya.identity (m)

    member __.Combine (m1: Freya<_>, m2: Freya<'a>) : Freya<'a> =
        Freya.combine (m1, m2)

    member __.Zero () : Freya<unit> =
        Freya.zero ()

// Builder

// The instance of the FreyaBuilder used to provide the freya computation
// expression syntax.

[<AutoOpen>]
module Builder =

    /// A computation expression for creating and working with Freya<_>
    /// function types using the various Freya functions defined for working
    /// with state, composition, etc.

    let freya =
        FreyaBuilder ()