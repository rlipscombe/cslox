# cslox: C# implementation of Lox

Lox is the programming language from "Crafting Interpreters"; see
http://craftinginterpreters.com/

I'm going to write it in C# because I don't know any Java.

## Compiling it

    dotnet build

## Running it

    dotnet run
    dotnet run input.lox

## C# on .NET Core on Linux with VS Code

- <https://docs.microsoft.com/en-gb/dotnet/core/install/linux-package-manager-ubuntu-1804>
- <https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code>

## Follow up

> There are a few well-established styles of IRs out there. Hit your
> search engine of choice and look for “control flow graph”, “static
> single-assignment”, “continuation-passing style”, and “three-address
> code”.

Erlang, since 22(?) uses SSA. What did it do before that? What are some
other examples?

----

> If you’ve ever wondered how GCC supports so many crazy languages and
> architectures, like Modula-3 on Motorola 68k, now you know. Language
> front ends target one of a handful of IRs, mainly GIMPLE and RTL.
> Target backends like the one for 68k then take those IRs and produce
> native code.

What languages can be converted to GIMPLE or RTL? What backends to
GIMPLE and RTL are there? Are there interpreters for same?

----

> Smalltalk has no built-in branching constructs, and relies on dynamic
> dispatch for selectively executing code.

Given lunchtime's conversation with Charlie about the fact that `if` is
a statement (or an expression, in Rust), not a keyword, this note about
Smalltalk intrigues me.

## Further Reading

...being things that were referenced in the "Crafting Interpreters" book
(and some that weren't) that I should take the time to read later:

- [A Unified Theory of Garbage Collection][Bacon04Unified]
- [The Next 700 Programming Languages][landin-next-700]
- [Monads for functional programming][baastad] (from Bodil's
  [parser combinators][bodil-parser-combinators] post)

[Bacon04Unified]: https://researcher.watson.ibm.com/researcher/files/us-bacon/Bacon04Unified.pdf
[landin-next-700]: https://homepages.inf.ed.ac.uk/wadler/papers/papers-we-love/landin-next-700.pdf
[baastad]: https://homepages.inf.ed.ac.uk/wadler/papers/marktoberdorf/baastad.pdf
[bodil-parser-combinators]: https://bodil.lol/parser-combinators/
