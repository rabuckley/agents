# Foundations: what makes an abstraction right

The primary texts behind the shape tests, digested with the quotes a disputed finding can cite. Read the section that matches the disputed test.

## Parnas, "On the Criteria To Be Used in Decomposing Systems into Modules" (CACM, 1972)

Source: the paper itself (CACM 15(12), 1053-1058). Quotes verbatim.

- A module is "a responsibility assignment rather than a subprogram". The criterion: "it is almost always incorrect to begin the decomposition of a system into modules on the basis of a flowchart. We propose instead that one begins with a list of difficult design decisions or design decisions which are likely to change. Each module is then designed to hide such a decision from the others. Since, in most cases, design decisions transcend time of execution, modules will not correspond to steps in the processing."
- The interface rule: "Every module in the second decomposition is characterized by its knowledge of a design decision which it hides from all others. Its interface or definition was chosen to reveal as little as possible about its inner workings." In the good decomposition "the interfaces are more abstract; they consist primarily in the function names and the numbers and types of the parameters."
- Over-specification is a design error. On the circular-shift module: "By prescribing the order for the shifts we have given more information than necessary and so unnecessarily restricted the class of systems that we can build without changing the definitions ... Our failure to do this ... must clearly be classified as a design error." An API that promises more than callers need (an ordering, a concrete collection type, a timing) is over-specified.
- The benefits the criterion buys: changeability (a change confined to one module), independent development (interfaces are simple decisions, so work starts early), comprehensibility (one module at a time). Decisions that belong hidden, from the paper's list: a data structure with its accessors and modifiers; the formats of control blocks; character codes, alphabetic orderings and similar data; the sequence in which items are processed.
- "hierarchical structure and 'clean' decomposition are two desirable but independent properties of a system structure." A layered design is not evidence of information hiding.

For API review: the KWIC first decomposition (Input, Circular Shift, Alphabetize, Output, Master Control) is the shape a type takes when its members mirror the steps of a process and share a representation. Ask of a type "what decision does this hide?"; if the answer is a step rather than a secret, it is decomposition 1.

## Ousterhout, "Designing Abstractions" (Stanford CS349W lecture notes)

Source: web.stanford.edu/~ouster/CS349W/lectures/abstraction.html. Paraphrase of the lecture's points.

- The fundamental problem is complexity, not algorithms; systems outgrow understanding, and the remedy is decomposition into abstractions. A good abstraction: simple interface hiding substantial implementation; less code and learning to use; changes stay inside; reusable in ways not anticipated; hides complex decisions (Parnas).
- Graded examples: a list abstraction with First/Next adds little over pointers (weak); a hash table hides hashing, collisions and resizing behind get/set (good); Unix pipes let tiny filters compose (excellent).
- How to design one: start from observed code patterns; find the simplest design for the common case, then add special cases through optional arguments or extra methods without complicating the baseline; avoid excessive interfaces (Java I/O needing several classes to read a file versus Unix's restraint); provide sensible defaults; eliminate errors by design and centralize the handling that remains; remove arbitrary restrictions (tables and strings grow); eliminate explicit steps that can happen automatically (initialization, loading).
- The best designers move complexity inside the abstraction; very capable designers often fail to, because the complexity is transparent to them. Expect several revisions. The quality lives in many small details.

## Ousterhout, _A Philosophy of Software Design_ (2nd ed.)

The principles the shape tests draw on, by chapter. Red flags are listed verbatim at `../../braid-review/references/red-flags.md` when sharper language is needed.

- Complexity is anything that makes software hard to understand or modify; its symptoms are change amplification, cognitive load, and unknown unknowns; its causes are dependencies and obscurity (ch. 2).
- Modules should be deep: a lot of functionality behind a simple interface. The interface is everything a developer must know to use the module, formal (signatures) and informal (ordering, side effects, constraints). Shallow modules add interface without adding much function. Classitis (many small classes) is the failure mode (ch. 4).
- Information hiding (Parnas) is the main technique for depth. Leakage is the same knowledge in several modules; temporal decomposition (structure follows execution order) is its common cause. Overexposure: a common feature whose API forces learning rare features (ch. 5).
- General-purpose modules are deeper. Design "somewhat general-purpose": the interface general enough to serve several uses, the implementation only what today needs. Tests: what is the simplest interface that covers my current needs; in how many situations will this method be used (one means too specialized); is this API easy to use for my current needs (ch. 6).
- Different layer, different abstraction. Pass-through methods (same signature, forwards the call) and pass-through variables (threaded through layers that never read them) are symptoms (ch. 7).
- Pull complexity downward: it is better for the module to be complex than its users. Configuration parameters push complexity up; choose a default (ch. 8).
- Better together or better apart: bring together what shares information, is always used together, or simplifies the interface; separate general-purpose from special-purpose code. Depth is more important than length (ch. 9).
- Define errors out of existence: redesign so the exceptional case is not exceptional (unset of a missing key is a no-op; substring clips to range). Mask exceptions inside the module, aggregate handlers; exceptions thrown by an interface are part of its complexity (ch. 10).
- Design it twice: produce a second, materially different design before choosing; compare on ease of use for the common case, simplicity of the interface, generality, efficiency (ch. 11).
- Interface comments describe the contract, never the implementation; if the comment is hard to write simply and completely, the design is wrong (chs. 13, 15). Names that are vague or hard to pick are the same signal (ch. 14).
- Consistency reduces what a reader must learn: similar things done the same way (ch. 17). This is the one principle the FDG serves directly.

## Hickey, "Simple Made Easy" (Strange Loop, 2011)

Source: the talk transcript (github.com/matthiasn/talk-transcripts). Slide text and quotes.

- Simple: from _sim-plex_, one fold or braid; not interleaved. Objective: you can look and see whether things are entangled. Easy: "to lie near": at hand, familiar, within present skill. Relative to a person.
- Complect: to braid together, entwine. The source of incidental complexity. State complects value and time; objects complect state, identity and value; methods complect function and state; inheritance complects types.
- "If you focus on ease and ignore simplicity, complexity will eventually kill you." Reliability needs understanding; understanding needs a bounded mental load; entanglement multiplies it.
- Simpler constructs: values over state; functions over methods; data over syntax; polymorphism a la carte (protocols, interfaces added independently) over inheritance; set functions and queues over loops and actors; declarative data manipulation over ORM; rules over conditionals.
- "Information is simple. The only thing you can possibly do with it is ruin it." Data that is only data should be plain data, not wrapped in classes with behaviour. Encapsulation hides implementation, not information.
- "Simplicity is a choice."

For API review: idiomatic is _easy_ (familiar, near at hand); the review must judge _simple_ (unbraided). Findings in this vocabulary: a stateful API whose result depends on call order braids value with time; a mutable type that stands for a value braids identity with value; a flag parameter braids two behaviours into one member; a type that is information plus methods braids data with behaviour and stops callers treating it as data.
