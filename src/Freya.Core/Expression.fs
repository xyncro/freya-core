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

    member __.Bind (aF: Freya<'a>, a2bF: 'a -> Freya<'b>) : Freya<'b> =
        aF |> Freya.bind a2bF

    member __.Delay (u2aF: unit -> Freya<'a>) : Freya<'a> =
        Freya.delay u2aF

    member __.Return (a: 'a) : Freya<'a> =
        Freya.init a

    member __.ReturnFrom (aF: Freya<'a>) : Freya<'a> =
        aF

    member __.Combine (xF: Freya<_>, aF: Freya<'a>) : Freya<'a> =
        xF |> Freya.combine aF

    member __.Zero () : Freya<unit> =
        Freya.empty

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