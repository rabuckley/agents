# Ousterhout's red flags (A Philosophy of Software Design)

Provenance: the book's "Summary of Red Flags" back-matter is not published on
any Ousterhout-hosted page; this list is transcribed from Sébastien
Portebois's compilation (notes.portebois.net, consistent across independent
fetches), with the book's section references. Treat wording as
high-confidence transcription, not author-hosted text.

1. **Shallow Module** (§4.6) — "A shallow module is one whose interface is
   complicated relative to the functionality it provides."
2. **Information Leakage** (§5.2) — "Information leakage occurs when the same
   knowledge is used in multiple places, such as two different classes that
   both understand the format of a particular type of file."
3. **Temporal Decomposition** (§5.3) — "In temporal decomposition, execution
   order is reflected in the code structure: operations that happen at
   different times are in different methods or classes."
4. **Overexposure** (§5.7) — "If the API for a commonly used feature forces
   users to learn about other features that are rarely used, this increases
   the cognitive load on users who don't need the rarely used features."
5. **Pass-Through Method** (§7.1) — "A pass-through method is one that does
   nothing except pass its arguments to another method, usually with the same
   API as the pass-through method."
6. **Repetition** (§9.4) — "If the same piece of code appears over and over
   again, that's a red flag that you haven't found the right abstractions."
7. **Special-General Mixture** (§9.5) — "This red flag occurs when a
   general-purpose mechanism also contains code specialized for a particular
   use of that mechanism."
8. **Conjoined Methods** (§9.8) — "It should be possible to understand each
   method independently. If you can't understand the implementation of one
   method without also understanding the implementation of another, that's a
   red flag."
9. **Comment Repeats Code** (§13.2) — "If the information in a comment is
   already obvious from the code next to the comment, then the comment isn't
   helpful."
10. **Implementation Documentation Contaminates Interface** (§13.5) — "This
    red flag occurs when interface documentation, such as that for a method,
    describes implementation details that aren't needed in order to use the
    thing being documented."
11. **Vague Name** (§14.3) — "If a variable or method name is broad enough to
    refer to many different things, then it doesn't convey much information
    to the developer and the underlying entity is more likely to be misused."
12. **Hard to Pick Name** (§14.3) — "If it's hard to find a simple name for a
    variable or method that creates a clear image of the underlying object,
    that's a hint that the underlying object may not have a clean design."
13. **Hard to Describe** (§15.3) — "The comment that describes a method or
    variable should be simple and yet complete. If you find it difficult to
    write such a comment, that's an indicator that there might be a problem
    with the design of the thing you are describing."
14. **Nonobvious Code** (§18.2) — "If the meaning and behavior of code cannot
    be understood with a quick reading, it is a red flag."

On splitting (APOSD 2nd ed. §9.8, Ousterhout-hosted extract) — the primary
text behind the length ruling, for when a finding is disputed:

> "Depth is more important than length: first make functions deep, then try
> to make them short enough to be easily read. Don't sacrifice depth for
> length."

> "If functions are made too small, they lose their independence, resulting
> in conjoined functions that must be read and understood together. When this
> happens, then it's better to keep the larger function, so all of the
> related code is one place."
