# Framework Design Guidelines digest

Source: the FDG pages on Microsoft Learn (https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/), which reprint the 2nd edition (Cwalina and Abrams, 2008). Every DO / CONSIDER / AVOID / DO NOT rule is kept with its label and exact wording; rationale is one line where the page gives one. Dropped as dead for present-day review: WPF dependency properties, the 2008 serialization chapter, System.Xml usage, assembly and resource naming, pointer parameters.

Where modern .NET has moved on, `modern-dotnet.md` overrides this file: sealing new types by default, optional parameters, `IReadOnlyList<T>` and `Span<T>` in signatures, async shape, nullable annotations. Read the section that matches the surface under review; each H3 carries its page URL.

The FDG's own framing: the rules exist so that a developer can "implement main scenarios without reading the documentation", and there "might be situations where good library design requires that you violate these design guidelines. Such cases should be rare, and it is important that you have a clear and compelling reason."

## Naming Guidelines

### Capitalization Conventions

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions

PascalCasing is used for all identifiers except parameter names and capitalizes the first character of each word, including acronyms over two letters (`PropertyDescriptor`, `HtmlTag`). Two-letter acronyms have both letters capitalized (`IOStream`). camelCasing is used only for parameter names; two-letter acronyms that begin a camel-cased identifier are both lowercase (`ioStream`, `htmlTag`). Do not use underscores anywhere in identifiers.

- ✔️ DO use PascalCasing for all public member, type, and namespace names consisting of multiple words.
- ✔️ DO use camelCasing for parameter names.
  - Casing table: Namespace, Type, Interface, Method, Property, Event, Field, Enum value are all Pascal; Parameter is camel.
- ❌ DO NOT capitalize each word in so-called closed-form compound words.
  - Treat closed-form compound words (written as a single word, e.g. endpoint) as one word; use a current dictionary. Examples: `BitFlag` not `Bitflag`; `Callback` not `CallBack`; `Canceled` not `Cancelled`; `Email` not `EMail`; `Endpoint` not `EndPoint`; `FileName` not `Filename`; `Gridline` not `GridLine`; `Hashtable` not `HashTable`; `Id` not `ID`; `Indexes` not `Indices`; `LogOff` not `LogOut`; `LogOn` not `LogIn`; `Metadata` not `MetaData`; `Multipanel`; `Multiview`; `Namespace` not `NameSpace`; `Ok` not `OK`; `Pi` not `PI`; `Placeholder` not `PlaceHolder`; `SignIn` not `SignOn`; `SignOut` not `SignOff`; `UserName` not `Username`; `WhiteSpace` not `Whitespace`; `Writable` not `Writeable`.
- ❌ DO NOT assume that all programming languages are case sensitive. They are not. Names cannot differ by case alone.
  - Rationale: CLR languages are not required to be case-sensitive, so externally accessible APIs cannot rely on case alone to distinguish two names in the same context.

### General Naming Conventions

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions

Word choice:

- ✔️ DO choose easily readable identifier names.
  - `HorizontalAlignment` reads better than `AlignmentHorizontal`.
- ✔️ DO favor readability over brevity.
  - `CanScrollHorizontally` is better than `ScrollableX`.
- ❌ DO NOT use underscores, hyphens, or any other nonalphanumeric characters.
- ❌ DO NOT use Hungarian notation.
- ❌ AVOID using identifiers that conflict with keywords of widely used programming languages.
  - Rationale: CLS languages must offer an escape (C# uses `@`), but a method that needs the escape is harder to use.

Abbreviations and acronyms:

- ❌ DO NOT use abbreviations or contractions as part of identifier names.
  - `GetWindow` rather than `GetWin`.
- ❌ DO NOT use any acronyms that are not widely accepted, and even if they are, only when necessary.

Avoiding language-specific names:

- ✔️ DO use semantically interesting names rather than language-specific keywords for type names.
  - `GetLength` is better than `GetInt`.
- ✔️ DO use a generic CLR type name, rather than a language-specific name, in the rare cases when an identifier has no semantic meaning beyond its type.
  - `ToInt64`, not `ToLong`. CLR names: SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Boolean, Char, String, Object.
- ✔️ DO use a common name, such as `value` or `item`, rather than repeating the type name, in the rare cases when an identifier has no semantic meaning and the type of the parameter is not important.

Naming new versions of existing APIs:

- ✔️ DO use a name similar to the old API when creating new versions of an existing API.
  - Rationale: highlights the relationship between the APIs.
- ✔️ DO prefer adding a suffix rather than a prefix to indicate a new version of an existing API.
  - Rationale: documentation browsers and IntelliSense sort alphabetically, so the old and new versions sit together.
- ✔️ CONSIDER using a brand new, but meaningful identifier, instead of adding a suffix or a prefix.
- ✔️ DO use a numeric suffix to indicate a new version of an existing API, particularly if the existing name of the API is the only name that makes sense (i.e., if it is an industry standard) and if adding any meaningful suffix (or changing the name) is not an appropriate option.
- ❌ DO NOT use the "Ex" (or a similar) suffix for an identifier to distinguish it from an earlier version of the same API.
- ✔️ DO use the "64" suffix when introducing versions of APIs that operate on a 64-bit integer (a long integer) instead of a 32-bit integer. You only need to take this approach when the existing 32-bit API exists; don't do it for brand new APIs with only a 64-bit version.

### Names of Namespaces

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

Template: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]` (e.g. `Fabrikam.Math`, `Litware.Security`).

- ✔️ DO prefix namespace names with a company name to prevent namespaces from different companies from having the same name.
- ✔️ DO use a stable, version-independent product name at the second level of a namespace name.
- ❌ DO NOT use organizational hierarchies as the basis for names in namespace hierarchies, because group names within corporations tend to be short-lived. Organize the hierarchy of namespaces around groups of related technologies.
- ✔️ DO use PascalCasing, and separate namespace components with periods (e.g., `Microsoft.Office.PowerPoint`). If your brand employs nontraditional casing, you should follow the casing defined by your brand, even if it deviates from normal namespace casing.
- ✔️ CONSIDER using plural namespace names where appropriate.
  - `System.Collections` not `System.Collection`; brand names and acronyms are exceptions (`System.IO`).
- ❌ DO NOT use the same name for a namespace and a type in that namespace.
  - Rationale: several compilers require such types to be fully qualified.

Namespaces and type name conflicts:

- ❌ DO NOT introduce generic type names such as `Element`, `Node`, `Log`, and `Message`.
  - Rationale: very likely to conflict; qualify them (`FormElement`, `XmlNode`, `EventLog`, `SoapMessage`).
- ❌ DO NOT give the same name to types in namespaces within a single application model.
  - Application model namespaces (e.g. `System.Windows*`, `System.Web.UI*`) are used together but rarely with other application models; e.g. don't add `Page` to `System.Web.UI.Adapters` because `System.Web.UI` has `Page`.
- Infrastructure namespaces (e.g. `.Design`) are rarely imported, so avoiding conflicts with them is not critical.
- ❌ DO NOT give types names that would conflict with any type in the Core namespaces.
  - Core namespaces are all `System` namespaces except application model and infrastructure ones (`System`, `System.IO`, `System.Xml`, `System.Net`); never use `Stream` as a type name.
- ❌ DO NOT assign type names that would conflict with other types within a single technology.
  - Technology namespace group = same first two nodes (`<Company>.<Technology>*`).
- ❌ DO NOT introduce type name conflicts between types in technology namespaces and an application model namespace (unless the technology is not intended to be used with the application model).

### Names of Classes, Structs, and Interfaces

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces

- ✔️ DO name classes and structs with nouns or noun phrases, using PascalCasing.
  - Rationale: distinguishes type names from methods, which are verb phrases.
- ✔️ DO name interfaces with adjective phrases, or occasionally with nouns or noun phrases.
  - Nouns should be rare and might indicate the type should be an abstract class, not an interface.
- ❌ DO NOT give class names a prefix (e.g., "C").
- ✔️ CONSIDER ending the name of derived classes with the name of the base class.
  - `ArgumentOutOfRangeException`, `SerializableAttribute`; but use judgment (`Button` is a `Control`).
- ✔️ DO prefix interface names with the letter I, to indicate that the type is an interface.
  - `IComponent`, `ICustomAttributeProvider`, `IPersistable`.
- ✔️ DO ensure that the names differ only by the "I" prefix on the interface name when you are defining a class–interface pair where the class is a standard implementation of the interface.

Names of generic type parameters:

- ✔️ DO name generic type parameters with descriptive names unless a single-letter name is completely self-explanatory and a descriptive name would not add value.
- ✔️ CONSIDER using `T` as the type parameter name for types with one single-letter type parameter.
  - `IComparer<T>`, `Predicate<T>`, `Nullable<T>`.
- ✔️ DO prefix descriptive type parameter names with `T`.
  - `ISessionChannel<TSession> where TSession : ISession`.
- ✔️ CONSIDER indicating constraints placed on a type parameter in the name of the parameter.
  - A parameter constrained to `ISession` might be called `TSession`.

Names of common types:

- ✔️ DO follow the guidelines described in the following table when naming types derived from or implementing certain .NET Framework types.
  - `System.Attribute`: ✔️ DO add the suffix "Attribute" to names of custom attribute classes.
  - `System.Delegate`: ✔️ DO add the suffix "EventHandler" to names of delegates that are used in events. ✔️ DO add the suffix "Callback" to names of delegates other than those used as event handlers. ❌ DO NOT add the suffix "Delegate" to a delegate.
  - `System.EventArgs`: ✔️ DO add the suffix "EventArgs."
  - `System.Enum`: ❌ DO NOT derive from this class; use the keyword supported by your language instead; for example, in C#, use the `enum` keyword. ❌ DO NOT add the suffix "Enum" or "Flag."
  - `System.Exception`: ✔️ DO add the suffix "Exception."
  - `IDictionary`, `IDictionary<TKey,TValue>`: ✔️ DO add the suffix "Dictionary." (takes precedence over the general collections guideline)
  - `IEnumerable`, `ICollection`, `IList`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`: ✔️ DO add the suffix "Collection."
  - `System.IO.Stream`: ✔️ DO add the suffix "Stream."
  - `CodeAccessPermission`, `IPermission`: ✔️ DO add the suffix "Permission."

Naming enumerations:

- ✔️ DO use a singular type name for an enumeration unless its values are bit fields.
- ✔️ DO use a plural type name for an enumeration with bit fields as values, also called flags enum.
- ❌ DO NOT use an "Enum" suffix in enum type names.
- ❌ DO NOT use "Flag" or "Flags" suffixes in enum type names.
- ❌ DO NOT use a prefix on enumeration value names (e.g., "ad" for ADO enums, "rtf" for rich text enums, etc.).

### Names of Type Members

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members

Names of methods:

- ✔️ DO give methods names that are verbs or verb phrases.
  - Rationale: methods take action; this also distinguishes them from properties and types (noun/adjective phrases). E.g. `CompareTo`, `Split`, `Trim`.

Names of properties:

- ✔️ DO name properties using a noun, noun phrase, or adjective.
  - Rationale: a property refers to data.
- ❌ DO NOT have properties that match the name of "Get" methods as in the following example: `public string TextWriter { get {...} set {...} }` / `public string GetTextWriter(int value) { ... }`
  - Rationale: this pattern typically indicates the property should really be a method.
- ✔️ DO name collection properties with a plural phrase describing the items in the collection instead of using a singular phrase followed by "List" or "Collection".
- ✔️ DO name Boolean properties with an affirmative phrase (`CanSeek` instead of `CantSeek`). Optionally, you can also prefix Boolean properties with "Is", "Can", or "Has", but only where it adds value.
- ✔️ CONSIDER giving a property the same name as its type.
  - `public Color Color { get; set; }` for an enum `Color`.

Names of events:

- ✔️ DO name events with a verb or a verb phrase.
  - `Clicked`, `Painting`, `DroppedDown`.
- ✔️ DO give events names with a concept of before and after, using the present and past tenses.
  - `Closing` before a window closes, `Closed` after.
- ❌ DO NOT use "Before" or "After" prefixes or postfixes to indicate pre- and post-events. Use present and past tenses as just described.
- ✔️ DO name event handlers (delegates used as types of events) with the "EventHandler" suffix, as shown in the following example: `public delegate void ClickedEventHandler(object sender, ClickedEventArgs e);`
- ✔️ DO use two parameters named `sender` and `e` in event handlers.
  - `sender` is the object that raised the event and is typically of type `object` even if a more specific type is possible.
- ✔️ DO name event argument classes with the "EventArgs" suffix.

Names of fields (apply to static public and protected fields; public/protected instance fields are not allowed by member design guidelines):

- ✔️ DO use PascalCasing in field names.
- ✔️ DO name fields using a noun, noun phrase, or adjective.
- ❌ DO NOT use a prefix for field names.
  - E.g. no "g*" or "s*" for static fields.

### Naming Parameters

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-parameters

Rationale for the page: parameter names appear in documentation and IntelliSense.

- ✔️ DO use camelCasing in parameter names.
- ✔️ DO use descriptive parameter names.
- ✔️ CONSIDER using names based on a parameter’s meaning rather than the parameter’s type.

Naming operator overload parameters:

- ✔️ DO use `left` and `right` for binary operator overload parameter names if there is no meaning to the parameters.
- ✔️ DO use `value` for unary operator overload parameter names if there is no meaning to the parameters.
- ✔️ CONSIDER meaningful names for operator overload parameters if doing so adds significant value.
- ❌ DO NOT use abbreviations or numeric indices for operator overload parameter names.

## Type Design Guidelines

### Choosing Between Class and Struct

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct

Background: value types are allocated on the stack or inline and are cheaper to allocate/deallocate; value type arrays are allocated inline with better locality; value types box when cast to object/interfaces, and too much boxing hurts the GC; assignments copy the whole value; value types are passed by value, so mutable value types confuse users and should be immutable. The majority of types in a framework should be classes.

- ✔️ CONSIDER defining a struct instead of a class if instances of the type are small and commonly short-lived or are commonly embedded in other objects.
- ❌ AVOID defining a struct unless the type has all of the following characteristics:
  - It logically represents a single value, similar to primitive types (`int`, `double`, etc.).
  - It has an instance size under 16 bytes.
  - It is immutable.
  - It will not have to be boxed frequently.
  - In all other cases, define the type as a class.

### Abstract Class Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/abstract-class

- ❌ DO NOT define public or protected internal constructors in abstract types.
  - Rationale: you cannot create instances of an abstract type, so a public constructor is misleading.
- ✔️ DO define a protected or an internal constructor in abstract classes.
  - Protected lets the base class initialize itself when subtypes are created; internal limits concrete implementations to the defining assembly.
- ✔️ DO provide at least one concrete type that inherits from each abstract class that you ship.
  - Rationale: validates the design of the abstract class (e.g. `FileStream` for `Stream`).

### Static Class Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/static-class

A static class contains only static members (plus inherited `Object` instance members and possibly a private constructor). In C# 2.0+ a static class is sealed, abstract, and cannot declare or override instance members. Static classes are a compromise between pure OO design and simplicity, used for shortcuts (`System.IO.File`), extension method holders, or functionality not warranting an OO wrapper (`System.Environment`).

- ✔️ DO use static classes sparingly.
  - Only as supporting classes for the object-oriented core of the framework.
- ❌ DO NOT treat static classes as a miscellaneous bucket.
- ❌ DO NOT declare or override instance members in static classes.
- ✔️ DO declare static classes as sealed, abstract, and add a private instance constructor if your programming language does not have built-in support for static classes.

### Interface Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/interface

Interfaces are used to achieve the effect of multiple inheritance (e.g. `IDisposable`) and to provide a common base type that value types can share, since value types can only inherit from `ValueType`. Except for these situations, in general choose classes rather than interfaces.

- ✔️ DO define an interface if you need some common API to be supported by a set of types that includes value types.
- ✔️ CONSIDER defining an interface if you need to support its functionality on types that already inherit from some other type.
- ❌ AVOID using marker interfaces (interfaces with no members).
  - Use a custom attribute instead to mark a class as having a characteristic.
- ✔️ DO provide at least one type that is an implementation of an interface.
  - Rationale: validates the interface design (e.g. `List<T>` for `IList<T>`).
- ✔️ DO provide at least one API that consumes each interface you define (a method taking the interface as a parameter or a property typed as the interface).
  - Rationale: validates the interface design (e.g. `List<T>.Sort` consumes `IComparer<T>`).
- ❌ DO NOT add members to an interface that has previously shipped.
  - Rationale: breaks implementations; create a new interface instead.

### Struct Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/struct

- ❌ DO NOT provide a parameterless constructor for a struct.
  - Rationale: lets arrays of structs be created without running the constructor per item; C# does not allow it anyway.
- ❌ DO NOT define mutable value types.
  - Rationale: property getters return copies, so developers may mutate a copy unknowingly; dynamic languages copy even on dereference of locals.
- ✔️ DO ensure that a state where all instance data is set to zero, false, or null (as appropriate) is valid.
  - Rationale: prevents accidental invalid instances when an array of the struct is created.
- ✔️ DO implement `IEquatable<T>` on value types.
  - Rationale: `Object.Equals` on value types boxes and its default implementation uses reflection; `IEquatable<T>.Equals` avoids boxing.
- ❌ DO NOT explicitly extend `ValueType`. In fact, most languages prevent this.
- Summary: structs should only be used for small, single, immutable values that will not be boxed frequently.

### Enum Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum

Simple enums represent small closed sets of choices; flag enums support bitwise operations on values.

- ✔️ DO use an enum to strongly type parameters, properties, and return values that represent sets of values.
- ✔️ DO favor using an enum instead of static constants.
- ❌ DO NOT use an enum for open sets (such as the operating system version, names of your friends, etc.).
- ❌ DO NOT provide reserved enum values that are intended for future use.
  - Rationale: you can add values later; reserved values pollute the set and lead to user errors.
- ❌ AVOID publicly exposing enums with only one value.
  - Rationale: the C habit of reserved parameters via single-value enums is unnecessary; method overloading allows adding parameters later.
- ❌ DO NOT include sentinel values in enums.
  - Rationale: sentinels track the state of the enum rather than being a real value and confuse users.
- ✔️ DO provide a value of zero on simple enums.
  - Call it something like "None"; otherwise give zero to the most common default value.
- ✔️ CONSIDER using `Int32` (the default in most programming languages) as the underlying type of an enum unless any of the following is true:
  - The enum is a flags enum and you have more than 32 flags, or expect to have more in the future.
  - The underlying type needs to be different than `Int32` for easier interoperability with unmanaged code expecting different-size enums.
  - A smaller underlying type would result in substantial savings in space (enum used as a field in a very frequently instantiated type, large arrays/collections of it, or many serialized instances). Managed objects are DWORD-aligned, so a smaller enum only helps if packed with other small fields.
- ✔️ DO name flag enums with plural nouns or noun phrases and simple enums with singular nouns or noun phrases.
- ❌ DO NOT extend `System.Enum` directly.
  - Use the language keyword (C# `enum`).

Designing flag enums:

- ✔️ DO apply the `System.FlagsAttribute` to flag enums. Do not apply this attribute to simple enums.
- ✔️ DO use powers of two for the flag enum values so they can be freely combined using the bitwise OR operation.
- ✔️ CONSIDER providing special enum values for commonly used combinations of flags.
  - Rationale: bitwise operations are advanced; e.g. `FileAccess.ReadWrite`.
- ❌ AVOID creating flag enums where certain combinations of values are invalid.
- ❌ AVOID using flag enum values of zero unless the value represents "all flags are cleared" and is named appropriately, as prescribed by the next guideline.
- ✔️ DO name the zero value of flag enums `None`. For a flag enum, the value must always mean "all flags are cleared."

Adding value to enums:

- ✔️ CONSIDER adding values to enums, despite a small compatibility risk.
  - Rationale: poorly written apps may mishandle a new value returned from an existing API; if you have real incompatibility data, add a new API returning old and new values and deprecate the old one.

### Nested Types

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/nested-types

Nested types have access to all members of the enclosing type. Use them sparingly: some developers are unfamiliar with the syntax, and nested types are tightly coupled to the enclosing type so they are not general-purpose. They are best for modeling implementation details (e.g. a collection's enumerator).

- ✔️ DO use nested types when the relationship between the nested type and its outer type is such that member-accessibility semantics are desirable.
- ❌ DO NOT use public nested types as a logical grouping construct; use namespaces for this.
- ❌ AVOID publicly exposed nested types. The only exception to this is if variables of the nested type need to be declared only in rare scenarios such as subclassing or other advanced customization scenarios.
- ❌ DO NOT use nested types if the type is likely to be referenced outside of the containing type.
  - E.g. an enum passed to a method on a class should not be nested in the class.
- ❌ DO NOT use nested types if they need to be instantiated by client code. If a type has a public constructor, it should probably not be nested.
  - Rationale: a type you can create, use, and destroy without the outer type has a place in the framework on its own.
- ❌ DO NOT define a nested type as a member of an interface. Many languages do not support such a construct.

## Member Design Guidelines

### Member Overloading

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/member-overloading

Overloading = two or more members on the same type with the same name differing only in number or type of parameters. Only methods, constructors, and indexed properties can be overloaded. Overloading on parameter count gives simpler versions; overloading on type lets one name perform identical operations on different types.

- ✔️ DO try to use descriptive parameter names to indicate the default used by shorter overloads.
- ❌ AVOID arbitrarily varying parameter names in overloads. If a parameter in one overload represents the same input as a parameter in another overload, the parameters should have the same name.
- ❌ AVOID being inconsistent in the ordering of parameters in overloaded members. Parameters with the same name should appear in the same position in all overloads.
- ✔️ DO make only the longest overload virtual (if extensibility is required). Shorter overloads should simply call through to a longer overload.
- ❌ DO NOT use `ref` or `out` modifiers to overload members.
  - Rationale: some languages cannot resolve such overloads, and they usually have different semantics and should be separate methods.
- ❌ DO NOT have overloads with parameters at the same position and similar types yet with different semantics.
- ✔️ DO allow `null` to be passed for optional arguments.
- ✔️ DO use member overloading rather than defining members with default arguments.
  - Rationale: default arguments are not CLS compliant.

### Property Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/property

Properties are smart fields: the calling syntax of fields with the flexibility of methods.

- ✔️ DO create get-only properties if the caller should not be able to change the value of the property.
  - If the property type is a mutable reference type, the value can still be changed through it.
- ❌ DO NOT provide set-only properties or properties with the setter having broader accessibility than the getter.
  - E.g. no public setter with protected getter. If a getter cannot be provided, use a method named `Set...` (e.g. `AppDomain.SetCachePath` instead of a set-only `CachePath`).
- ✔️ DO provide sensible default values for all properties, ensuring that the defaults do not result in a security hole or terribly inefficient code.
- ✔️ DO allow properties to be set in any order even if this results in a temporary invalid state of the object.
  - Rationale: interrelated properties are common; postpone exceptions from invalid combinations until the properties are actually used together.
- ✔️ DO preserve the previous value if a property setter throws an exception.
- ❌ AVOID throwing exceptions from property getters.
  - Rationale: getters should be simple with no preconditions; a getter that can throw should probably be a method. Does not apply to indexers, where argument validation exceptions are expected.

Indexed property design (indexers should be used only in APIs that provide access to items in a logical collection, e.g. `String`'s indexer over characters):

- ✔️ CONSIDER using indexers to provide access to data stored in an internal array.
- ✔️ CONSIDER providing indexers on types representing collections of items.
- ❌ AVOID using indexed properties with more than one parameter.
  - If multiple parameters are needed, reconsider whether it is really a logical collection accessor; otherwise use `Get`/`Set` methods.
- ❌ AVOID indexers with parameter types other than `System.Int32`, `System.Int64`, `System.String`, `System.Object`, or an enum.
  - Same reasoning: otherwise use a `Get`/`Set` method.
- ✔️ DO use the name `Item` for indexed properties unless there is an obviously better name (e.g., see the `Chars[]` property on `System.String`).
  - C# defaults indexers to `Item`; `IndexerNameAttribute` customizes it.
- ❌ DO NOT provide both an indexer and methods that are semantically equivalent.
- ❌ DO NOT provide more than one family of overloaded indexers in one type.
  - Enforced by the C# compiler.
- ❌ DO NOT use nondefault indexed properties.
  - Enforced by the C# compiler.

Property change notification events:

- ✔️ CONSIDER raising change notification events when property values in high-level APIs (usually designer components) are modified.
  - Not worth the overhead for low-level APIs such as base types or collections (e.g. `List<T>` does not raise one when `Count` changes).
- ✔️ CONSIDER raising change notification events when the value of a property changes via external forces.
  - E.g. `TextBox.Text` changes when the user types.

### Constructor Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/constructor

Type (static) constructors run before the type is used and take no parameters; instance constructors run on instance creation. Most developers try a constructor before alternatives such as factory methods.

- ✔️ CONSIDER providing simple, ideally default, constructors.
  - A simple constructor has very few parameters, all primitives or enums.
- ✔️ CONSIDER using a static factory method instead of a constructor if the semantics of the desired operation do not map directly to the construction of a new instance, or if following the constructor design guidelines feels unnatural.
- ✔️ DO use constructor parameters as shortcuts for setting main properties.
  - No semantic difference between the empty constructor plus property sets and a constructor with arguments.
- ✔️ DO use the same name for constructor parameters and a property if the constructor parameters are used to simply set the property.
  - The only difference should be casing.
- ✔️ DO minimal work in the constructor.
  - Capture the parameters; delay other processing until required.
- ✔️ DO throw exceptions from instance constructors, if appropriate.
- ✔️ DO explicitly declare the public parameterless constructor in classes, if such a constructor is required.
  - Rationale: adding a parameterized constructor stops the compiler from adding the parameterless one, which often causes accidental breaking changes.
- ❌ AVOID explicitly defining parameterless constructors on structs.
  - Rationale: array creation is faster when no constructor runs per slot; many compilers including C# disallow it.
- ❌ AVOID calling virtual members on an object inside its constructor.
  - Rationale: the most derived override runs before the most derived constructor has fully run.

Type constructor guidelines:

- ✔️ DO make static constructors private.
  - Rationale: if not private, code other than the CLR could call it, causing unexpected behavior; C# forces this.
- ❌ DO NOT throw exceptions from static constructors.
  - Rationale: the type becomes unusable in the current application domain.
- ✔️ CONSIDER initializing static fields inline rather than explicitly using static constructors, because the runtime is able to optimize the performance of types that don’t have an explicitly defined static constructor.

### Event Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/event

Events are the most commonly used callback form; usability studies show developers are more comfortable with events than delegates, virtual members, or interface plug-ins. Pre-events (e.g. `Form.Closing`) are raised before state changes; post-events (`Form.Closed`) after.

- ✔️ DO use the term "raise" for events rather than "fire" or "trigger."
- ✔️ DO use `System.EventHandler<TEventArgs>` instead of manually creating new delegates to be used as event handlers.
- ✔️ CONSIDER using a subclass of `EventArgs` as the event argument, unless you are absolutely sure the event will never need to carry any data to the event handling method, in which case you can use the `EventArgs` type directly.
  - Rationale: shipping with `EventArgs` directly means you can never add data without breaking compatibility; an empty subclass can grow properties later.
- ✔️ DO use a protected virtual method to raise each event. This is only applicable to nonstatic events on unsealed classes, not to structs, sealed classes, or static events.
  - Rationale: lets derived classes handle the event via override, which is more flexible, faster, and more natural. Name it "On" + event name. The derived class may not call the base implementation, so put nothing required for the base class to work in it.
- ✔️ DO take one parameter to the protected method that raises an event.
  - Named `e`, typed as the event argument class.
- ❌ DO NOT pass null as the sender when raising a nonstatic event.
- ✔️ DO pass null as the sender when raising a static event.
- ❌ DO NOT pass null as the event data parameter when raising an event.
  - Pass `EventArgs.Empty` when there is no data; developers expect non-null.
- ✔️ CONSIDER raising events that the end user can cancel. This only applies to pre-events.
  - Use `System.ComponentModel.CancelEventArgs` or a subclass.

Custom event handler design (for when `EventHandler<T>` cannot be used, e.g. pre-generics CLR):

- ✔️ DO use a return type of void for event handlers.
  - Rationale: an event can invoke many handlers, so there would be multiple return values.
- ✔️ DO use `object` as the type of the first parameter of the event handler, and call it `sender`.
- ✔️ DO use `System.EventArgs` or its subclass as the type of the second parameter of the event handler, and call it `e`.
- ❌ DO NOT have more than two parameters on event handlers.

### Field Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/field

Encapsulation: a type should be designed so changes to its fields (name or type) break no code outside the type's members, which implies all fields must be private. Constant and static read-only fields are excluded because they are never required to change.

- ❌ DO NOT provide instance fields that are public or protected.
  - Provide properties instead.
- ✔️ DO use constant fields for constants that will never change.
  - Rationale: the compiler burns const values into calling code, so they can never change without risking compatibility.
- ✔️ DO use public static `readonly` fields for predefined object instances.
- ❌ DO NOT assign instances of mutable types to `readonly` fields.
  - Rationale: `readonly` prevents replacing the instance but not modifying its data (arrays, most collections, and streams are mutable; `Int32`, `Uri`, `String` are immutable).

### Extension Methods

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/extension-methods

Extension methods are static methods callable with instance syntax; the defining "sponsor" class must be static, and its namespace must be imported to use them.

- ❌ AVOID frivolously defining extension methods, especially on types you don't own.
  - Rationale: if you own the type, prefer instance methods; liberal use clutters APIs of types not designed for them.
- ✔️ CONSIDER using extension methods in any of the following scenarios:
  - To provide helper functionality relevant to every implementation of an interface, if said functionality can be written in terms of the core interface (e.g. LINQ to Objects operators on `IEnumerable<T>`).
  - When an instance method would introduce a dependency on some type, but such a dependency would break dependency management rules (e.g. `Uri.ToUri(this string str)` rather than `String.ToUri()`).
- ❌ AVOID defining extension methods on `System.Object`.
  - Rationale: VB late-binds calls on `Object` references, while extension methods bind early, so VB users cannot call them; the same applies to other languages with that binding behavior or no extension method support.
- ❌ DO NOT put extension methods in the same namespace as the extended type unless it is for adding methods to interfaces or for dependency management.
- ❌ AVOID defining two or more extension methods with the same signature, even if they reside in different namespaces.
- ✔️ CONSIDER defining extension methods in the same namespace as the extended type if the type is an interface and if the extension methods are meant to be used in most or all cases.
- ❌ DO NOT define extension methods implementing a feature in namespaces normally associated with other features. Instead, define them in the namespace associated with the feature they belong to.
- ❌ AVOID generic naming of namespaces dedicated to extension methods (e.g., "Extensions"). Use a descriptive name (e.g., "Routing") instead.

### Operator Overloads

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads

Operator overloads make framework types feel like built-in primitives; they have been abused for operations that should be simple methods.

- ❌ AVOID defining operator overloads, except in types that should feel like primitive (built-in) types.
- ✔️ CONSIDER defining operator overloads in a type that should feel like a primitive type.
  - E.g. `System.String` defines `operator==` and `operator!=`.
- ✔️ DO define operator overloads in structs that represent numbers (such as `System.Decimal`).
- ❌ DO NOT be cute when defining operator overloads.
  - Rationale: only when the result is immediately obvious (`DateTime - DateTime = TimeSpan`); not union for database queries or shift to write to a stream.
- ❌ DO NOT provide operator overloads unless at least one of the operands is of the type defining the overload.
- ✔️ DO overload operators in a symmetric fashion.
  - `==` with `!=`, `<` with `>`, etc.
- ✔️ CONSIDER providing methods with friendly names that correspond to each overloaded operator.
  - Rationale: many languages do not support operator overloading. Friendly names: `op_Implicit`/`op_Explicit` -> `To<TypeName>`/`From<TypeName>`; `+` Add; `-` Subtract; `*` Multiply; `/` Divide; `%` Mod or Remainder; `^` Xor; `&` BitwiseAnd; `|` BitwiseOr; `&&` And; `||` Or; `=` Assign; `<<` LeftShift; `>>` RightShift; `op_SignedRightShift` SignedRightShift; `op_UnsignedRightShift` UnsignedRightShift; `==`/`!=` Equals; `>`, `<`, `>=`, `<=` CompareTo; compound assignments map to the base operation name (`*=` Multiply, `-=` Subtract, `^=` Xor, `<<=` LeftShift, `%=` Mod, `+=` Add, `&=` BitwiseAnd, `|=` BitwiseOr, `/=` Divide); `,` Comma; `--` Decrement; `++` Increment; unary `-` Negate; unary `+` Plus; `~` OnesComplement.

Overloading operator ==: quite complicated; its semantics must be compatible with other members such as `Object.Equals` (see Equality Operators page in Usage Guidelines).

Conversion operators (unary, static members on the operand or return type; implicit or explicit):

- ❌ DO NOT provide a conversion operator if such conversion is not clearly expected by the end users.
- ❌ DO NOT define conversion operators outside of a type’s domain.
  - E.g. no conversion from `Double`/`long` to `DateTime`; use a constructor.
- ❌ DO NOT provide an implicit conversion operator if the conversion is potentially lossy.
  - E.g. no implicit `Double` -> `Int32`; an explicit operator is allowed for lossy conversions.
- ❌ DO NOT throw exceptions from implicit casts.
  - Rationale: users may not be aware a conversion is taking place.
- ✔️ DO throw `System.InvalidCastException` if a call to a cast operator results in a lossy conversion and the contract of the operator does not allow lossy conversions.

### Parameter Design

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/parameter-design

- ✔️ DO use the least derived parameter type that provides the functionality required by the member.
  - E.g. a method that enumerates and prints items should take `IEnumerable`, not `ArrayList` or `IList`.
- ❌ DO NOT use reserved parameters.
  - Rationale: add a new overload when more input is needed.
- ❌ DO NOT have publicly exposed methods that take pointers, arrays of pointers, or multidimensional arrays as parameters.
  - Rationale: hard to use properly; APIs can almost always be redesigned.
- ✔️ DO place all `out` parameters following all of the by-value and `ref` parameters (excluding parameter arrays), even if it results in an inconsistency in parameter ordering between overloads (see Member Overloading).
  - Rationale: `out` parameters are extra return values; grouping them makes the signature easier to understand.
- ✔️ DO be consistent in naming parameters when overriding members or implementing interface members.
  - Rationale: communicates the relationship between the methods.

Choosing between enum and Boolean parameters:

- ✔️ DO use enums if a member would otherwise have two or more Boolean parameters.
- ❌ DO NOT use Booleans unless you are absolutely sure there will never be a need for more than two values.
  - Enums leave room for future values (with the caveats in Enum Design).
- ✔️ CONSIDER using Booleans for constructor parameters that are truly two-state values and are simply used to initialize Boolean properties.

Validating arguments:

- ✔️ DO validate arguments passed to public, protected, or explicitly implemented members. Throw `System.ArgumentException`, or one of its subclasses, if the validation fails.
  - Validation may happen in a lower private/internal routine; the point is that the whole exposed surface checks arguments.
- ✔️ DO throw `ArgumentNullException` if a null argument is passed and the member does not support null arguments.
- ✔️ DO validate enum parameters.
  - Rationale: the CLR allows casting any integer to an enum, so arguments may be out of range.
- ❌ DO NOT use `Enum.IsDefined` for enum range checks.
- ✔️ DO be aware that mutable arguments might have changed after they were validated.
  - For security-sensitive members, copy, then validate and process the copy.

Parameter passing (by-value copies the argument; `ref` passes a reference to the argument; `out` is like `ref` but unassigned on entry and must be assigned before return):

- ❌ AVOID using `out` or `ref` parameters.
  - Rationale: requires pointer-like experience, understanding of value vs reference types, and multiple return values; the `out`/`ref` distinction is not widely understood.
- ❌ DO NOT pass reference types by reference.
  - Limited exceptions, such as a method that swaps references.

Members with variable number of parameters (expressed via an array parameter; C# `params` on the last parameter lets callers pass elements inline, e.g. `String.Format(string format, params object[] parameters)`):

- ✔️ CONSIDER adding the params keyword to array parameters if you expect the end users to pass arrays with a small number of elements. If it's expected that lots of elements will be passed in common scenarios, users will probably not pass these elements inline anyway, and so the params keyword is not necessary.
- ❌ AVOID using params arrays if the caller would almost always have the input already in an array.
  - E.g. byte array parameters in .NET do not use params.
- ❌ DO NOT use params arrays if the array is modified by the member taking the params array parameter.
  - Rationale: compilers create a temporary array at the call site, so modifications are lost.
- ✔️ CONSIDER using the params keyword in a simple overload, even if a more complex overload could not use it.
- ✔️ DO try to order parameters to make it possible to use the params keyword.
- ✔️ CONSIDER providing special overloads and code paths for calls with a small number of arguments in extremely performance-sensitive APIs.
  - Avoids array allocation; name parameters with the singular form plus a numeric suffix; only if you special-case the entire code path rather than building an array and calling the general method.
- ✔️ DO be aware that null could be passed as a params array argument.
  - Validate the array is not null before processing.
- ❌ DO NOT use the `varargs` methods, otherwise known as the ellipsis.
  - Rationale: not CLS compliant.

## Designing for Extensibility

### Unsealed Classes

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/unsealed-classes

- ✔️ CONSIDER using unsealed classes with no added virtual or protected members as a great way to provide inexpensive yet much appreciated extensibility to a framework.
  - Rationale: developers inherit to add convenience members (custom constructors, new methods, overloads), e.g. custom `MessageQueue` subclasses; unsealed is the default in most languages and the recommended default for most framework classes because test costs are low.

### Protected Members

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/protected-members

Protected members do not provide extensibility by themselves but make subclassing more powerful, exposing advanced customization without complicating the main public interface. "Protected" can give a false sense of security.

- ✔️ CONSIDER using protected members for advanced customization.
- ✔️ DO treat protected members in unsealed classes as public for the purpose of security, documentation, and compatibility analysis.
  - Rationale: anyone can inherit from a class and access the protected members.

### Events and Callbacks

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/events-and-callbacks

Callbacks let a framework call back into user code through a delegate, usually passed as a method parameter. Events are a special case of callbacks with convenient syntax and Visual Studio designer support.

- ✔️ CONSIDER using callbacks to allow users to provide custom code to be executed by the framework.
- ✔️ CONSIDER using events to allow users to customize the behavior of a framework without the need for understanding object-oriented design.
- ✔️ DO prefer events over plain callbacks, because they are more familiar to a broader range of developers and are integrated with Visual Studio statement completion.
- ❌ AVOID using callbacks in performance-sensitive APIs.
- ✔️ DO use the new `Func<...>`, `Action<...>`, or `Expression<...>` types instead of custom delegates, when defining APIs with callbacks.
  - `Func`/`Action` are generic delegates; `Expression` represents function definitions that can be compiled and invoked at run time but also serialized and passed to remote processes.
- ✔️ DO measure and understand performance implications of using `Expression<...>`, instead of using `Func<...>` and `Action<...>` delegates.
  - Delegates are for local process scenarios; expressions for cases where evaluation in a remote process or machine is beneficial.
- ✔️ DO understand that by calling a delegate, you are executing arbitrary code and that could have security, correctness, and compatibility repercussions.

### Virtual Members

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/virtual-members

Virtual members give extensibility similar to callbacks with better execution performance and memory use, and feel natural for specialization; but they can only be modified at compile time, are costly to design, test, and maintain (any call can be overridden to run arbitrary code), and need more effort to define their contract. They perform better than callbacks and events but not better than non-virtual methods.

- ❌ DO NOT make members virtual unless you have a good reason to do so and you are aware of all the costs related to designing, testing, and maintaining virtual members.
  - Rationale: virtual members are less forgiving of compatible changes and slower (calls are not inlined).
- ✔️ CONSIDER limiting extensibility to only what is absolutely necessary.
- ✔️ DO prefer protected accessibility over public accessibility for virtual members. Public members should provide extensibility (if required) by calling into a protected virtual member.
  - Rationale: public members serve direct consumers; protected scopes virtual extensibility points to where they can be used.

### Abstractions (Abstract Types and Interfaces)

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/abstractions-abstract-types-and-interfaces

An abstraction describes a contract without a full implementation (abstract class or interface) plus reference documentation of required semantics (e.g. `Stream`, `IEnumerable<T>`, `Object`). Getting the right set of members is hard: too many and it is hard to implement; too few and it is useless. Too many abstractions hurt usability, but abstractions enable plug-ins, IoC, pipelines, and testability.

- ❌ DO NOT provide abstractions unless they are tested by developing several concrete implementations and APIs consuming the abstractions.
- ✔️ DO choose carefully between an abstract class and an interface when designing an abstraction.
- ✔️ CONSIDER providing reference tests for concrete implementations of abstractions. Such tests should allow users to test whether their implementations correctly implement the contract.

### Base Classes for Implementing Abstractions

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/base-classes-for-implementing-abstractions

A base class here is one designed to provide a common abstraction or reusable default implementation, sitting between a root abstraction and custom implementations (e.g. `Collection<T>` and `KeyedCollection<TKey,TItem>` help implement `IList<T>`). Base classes usually contain too much implementation to be abstractions themselves. They add surface area and hierarchy depth, so use them only if they provide significant value to framework users; if they help only implementers, prefer delegation to an internal implementation.

- ✔️ CONSIDER making base classes abstract even if they don’t contain any abstract members. This clearly communicates to the users that the class is designed solely to be inherited from.
- ✔️ CONSIDER placing base classes in a separate namespace from the mainline scenario types. By definition, base classes are intended for advanced extensibility scenarios and therefore are not interesting to the majority of users.
- ❌ AVOID naming base classes with a "Base" suffix if the class is intended for use in public APIs.

### Sealing

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/sealing

Sealing a class prevents inheritance; sealing a member prevents overriding.

- ❌ DO NOT seal classes without having a good reason to do so.
  - Rationale: not being able to think of an extensibility scenario is not a good reason; users inherit for non-obvious reasons such as convenience members. Good reasons: the class is static; it stores security-sensitive secrets in inherited protected members; it inherits many virtual members and sealing them individually costs more than sealing the class; it is an attribute needing very fast runtime lookup.
- ❌ DO NOT declare protected or virtual members on sealed types.
  - Rationale: sealed types cannot be inherited, so protected members cannot be called and virtual methods cannot be overridden.
- ✔️ CONSIDER sealing members that you override.
  - Rationale: the problems of virtual members apply to overrides too; sealing the override shields you from that point in the hierarchy down.

## Design Guidelines for Exceptions

### Exception Throwing

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/exception-throwing

Execution failure = a member cannot do what its name implies (e.g. `OpenFile` cannot return an opened handle). In the Framework, exceptions are used for all error conditions, including execution errors.

- ❌ DO NOT return error codes.
  - Rationale: exceptions are the primary means of reporting errors in frameworks.
- ✔️ DO report execution failures by throwing exceptions.
- ✔️ CONSIDER terminating the process by calling `System.Environment.FailFast` (.NET Framework 2.0 feature) instead of throwing an exception if your code encounters a situation where it is unsafe for further execution.
- ❌ DO NOT use exceptions for the normal flow of control, if possible.
  - Rationale: except for system failures and race-prone operations, design APIs so users can write code that does not throw, e.g. a precondition check (tester) before the member that does the work (doer). When Tester-Doer has unacceptable overhead, consider the Try-Parse pattern.
- ✔️ CONSIDER the performance implications of throwing exceptions. Throw rates above 100 per second are likely to noticeably impact the performance of most applications.
- ✔️ DO document all exceptions thrown by publicly callable members because of a violation of the member contract (rather than a system failure) and treat them as part of your contract.
  - Contract exceptions should not change between versions: no type changes, no new exceptions.
- ❌ DO NOT have public members that can either throw or not based on some option.
- ❌ DO NOT have public members that return exceptions as the return value or an `out` parameter.
  - Rationale: defeats the benefits of exception-based error reporting.
- ✔️ CONSIDER using exception builder methods.
  - Rationale: avoids code bloat when the same exception is thrown in many places; members that throw are not inlined, so moving the throw into a builder may allow inlining.
- ❌ DO NOT throw exceptions from exception filter blocks.
  - Rationale: the CLR catches the exception and the filter returns false, indistinguishable from an explicit false, so very hard to debug.
- ❌ AVOID explicitly throwing exceptions from finally blocks. Implicitly thrown exceptions resulting from calling methods that throw are acceptable.

### Using Standard Exception Types

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types

Exception and SystemException:

- ❌ DO NOT throw `System.Exception` or `System.SystemException`.
- ❌ DO NOT catch `System.Exception` or `System.SystemException` in framework code, unless you intend to rethrow.
- ❌ AVOID catching `System.Exception` or `System.SystemException`, except in top-level exception handlers.

ApplicationException:

- ❌ DO NOT throw or derive from `ApplicationException`.

InvalidOperationException:

- ✔️ DO throw an `InvalidOperationException` if the object is in an inappropriate state.

ArgumentException, ArgumentNullException, and ArgumentOutOfRangeException:

- ✔️ DO throw `ArgumentException` or one of its subtypes if bad arguments are passed to a member. Prefer the most derived exception type, if applicable.
- ✔️ DO set the `ParamName` property when throwing one of the subclasses of `ArgumentException`.
  - The name of the offending parameter; settable via constructor overloads.
- ✔️ DO use `value` for the name of the implicit value parameter of property setters.

NullReferenceException, IndexOutOfRangeException, and AccessViolationException:

- ❌ DO NOT allow publicly callable APIs to explicitly or implicitly throw `NullReferenceException`, `AccessViolationException`, or `IndexOutOfRangeException`. These exceptions are reserved and thrown by the execution engine and in most cases indicate a bug.
  - Do argument checking instead; throwing these exposes implementation details that may change.

StackOverflowException:

- ❌ DO NOT explicitly throw `StackOverflowException`. The exception should be explicitly thrown only by the CLR.
- ❌ DO NOT catch `StackOverflowException`.
  - Rationale: it is almost impossible to write managed code that stays consistent under arbitrary stack overflows.

OutOfMemoryException:

- ❌ DO NOT explicitly throw `OutOfMemoryException`. This exception is to be thrown only by the CLR infrastructure.

ComException, SEHException, and ExecutionEngineException:

- ❌ DO NOT explicitly throw `COMException`, `ExecutionEngineException`, and `SEHException`. These exceptions are to be thrown only by the CLR infrastructure.

### Exceptions and Performance

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/exceptions-and-performance

Throwing can be orders of magnitude slower, but good performance is achievable while still disallowing error codes, via two patterns.

- ❌ DO NOT use error codes because of concerns that exceptions might affect performance negatively.

Tester-Doer pattern: split a throwing member into a tester (e.g. `ICollection<T>.IsReadOnly`) and a doer (`Add`), so callers test before doing:

```csharp
if (!numbers.IsReadOnly)
{
    numbers.Add(1);
}
```

- ✔️ CONSIDER the Tester-Doer Pattern for members that might throw exceptions in common scenarios to avoid performance problems related to exceptions.

Try-Parse pattern: for extremely performance-sensitive APIs, make a well-defined test case part of the member semantics, e.g. `DateTime.Parse` throws while `DateTime.TryParse(string dateTime, out DateTime result)` returns false on failure. Define the try functionality strictly: if the member fails for any reason other than the well-defined try, it must still throw.

- ✔️ CONSIDER the Try-Parse Pattern for members that might throw exceptions in common scenarios to avoid performance problems related to exceptions.
- ✔️ DO use the prefix "Try" and Boolean return type for methods implementing this pattern.
- ✔️ DO provide an exception-throwing member for each member using the Try-Parse Pattern.

## Usage Guidelines

### Arrays

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/arrays

- ✔️ DO prefer using collections over arrays in public APIs. The Collections section provides details about how to choose between collections and arrays.
- ❌ DO NOT use read-only array fields. The field itself is read-only and can't be changed, but elements in the array can be changed.
- ✔️ CONSIDER using jagged arrays instead of multidimensional arrays.
  - Rationale: jagged arrays waste less space for sparse data, and the CLR optimizes index operations on them.

### Attributes

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/attributes

Attributes annotate assemblies, types, members, and parameters, stored in metadata and read via reflection. Required properties (required arguments) are positional constructor parameters; optional properties (optional arguments) are settable properties.

- ✔️ DO name custom attribute classes with the suffix "Attribute."
- ✔️ DO apply the `AttributeUsageAttribute` to custom attributes.
- ✔️ DO provide settable properties for optional arguments.
- ✔️ DO provide get-only properties for required arguments.
- ✔️ DO provide constructor parameters to initialize properties corresponding to required arguments. Each parameter should have the same name (although with different casing) as the corresponding property.
- ❌ AVOID providing constructor parameters to initialize properties corresponding to the optional arguments.
  - Rationale: no property should be settable both via constructor and setter; this makes explicit which arguments are optional and required and avoids two ways of doing the same thing.
- ❌ AVOID overloading custom attribute constructors.
  - Rationale: one constructor clearly communicates required vs optional arguments.
- ✔️ DO seal custom attribute classes, if possible. This makes the look-up for the attribute faster.

### Guidelines for Collections

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/guidelines-for-collections

Any type designed to manipulate a group of objects with a common characteristic is a collection; this section treats only types implementing `IEnumerable` or `IEnumerable<T>` as collections.

- ❌ DO NOT use weakly typed collections in public APIs.
  - All return values and parameters representing collection items should be the exact item type, not a base type (applies to public members of the collection).
- ❌ DO NOT use `ArrayList` or `List<T>` in public APIs.
  - Rationale: they are internal-implementation data structures; `List<T>` trades API cleanness and flexibility for performance (no notifications when clients modify it, many members like `BinarySearch` not applicable).
- ❌ DO NOT use `Hashtable` or `Dictionary<TKey,TValue>` in public APIs.
  - Use `IDictionary`, `IDictionary<TKey,TValue>`, or a custom type implementing them.
- ❌ DO NOT use `IEnumerator<T>`, `IEnumerator`, or any other type that implements either of these interfaces, except as the return type of a `GetEnumerator` method.
  - Rationale: enumerators from other methods cannot be used with `foreach`.
- ❌ DO NOT implement both `IEnumerator<T>` and `IEnumerable<T>` on the same type. The same applies to the nongeneric interfaces `IEnumerator` and `IEnumerable`.

Collection parameters:

- ✔️ DO use the least-specialized type possible as a parameter type. Most members taking collections as parameters use the `IEnumerable<T>` interface.
- ❌ AVOID using `ICollection<T>` or `ICollection` as a parameter just to access the `Count` property.
  - Use `IEnumerable<T>`/`IEnumerable` and dynamically check for `ICollection<T>`/`ICollection`.

Collection properties and return values:

- ❌ DO NOT provide settable collection properties.
  - Users can clear and re-add; if replacing the whole collection is common, consider an `AddRange` method.
- ✔️ DO use `Collection<T>` or a subclass of `Collection<T>` for properties or return values representing read/write collections.
  - If `Collection<T>` does not meet a requirement (e.g. must not implement `IList`), use a custom collection implementing `IEnumerable<T>`, `ICollection<T>`, or `IList<T>`.
- ✔️ DO use `ReadOnlyCollection<T>`, a subclass of `ReadOnlyCollection<T>`, or in rare cases `IEnumerable<T>` for properties or return values representing read-only collections.
  - Prefer `ReadOnlyCollection<T>`; custom read-only collections should implement `ICollection<T>.IsReadOnly` returning `true`; use `IEnumerable<T>` only when forward-only iteration is the only scenario you will ever support.
- ✔️ CONSIDER using subclasses of generic base collections instead of using the collections directly.
  - Rationale: better name and room for helper members, especially in high-level APIs.
- ✔️ CONSIDER returning a subclass of `Collection<T>` or `ReadOnlyCollection<T>` from very commonly used methods and properties.
  - Rationale: lets you add helper methods or change the implementation later.
- ✔️ CONSIDER using a keyed collection if the items stored in the collection have unique keys (names, IDs, etc.). Keyed collections are collections that can be indexed by both an integer and a key and are usually implemented by inheriting from `KeyedCollection<TKey,TItem>`.
  - Keyed collections have larger memory footprints; skip them if the overhead outweighs the benefit.
- ❌ DO NOT return null values from collection properties or from methods returning collections. Return an empty collection or an empty array instead.
  - General rule: null and empty collections/arrays should be treated the same.

Snapshots versus live collections (snapshot = state at a point in time, e.g. rows from a query; live = always current state, e.g. `ComboBox` items):

- ❌ DO NOT return snapshot collections from properties. Properties should return live collections.
  - Rationale: getters should be lightweight; a snapshot is an O(n) copy.
- ✔️ DO use either a snapshot collection or a live `IEnumerable<T>` (or its subtype) to represent collections that are volatile (i.e., that can change without explicitly modifying the collection).
  - Collections of shared resources (e.g. files in a directory) are volatile and hard to implement live except as a forward-only enumerator.

Choosing between arrays and collections:

- ✔️ DO prefer collections over arrays.
  - Rationale: more control over contents, can evolve, more usable; arrays for read-only scenarios require prohibitive cloning. Low-level read-write APIs may prefer arrays for smaller footprint and faster element access.
- ✔️ CONSIDER using arrays in low-level APIs to minimize memory consumption and maximize performance.
- ✔️ DO use byte arrays instead of collections of bytes.
- ❌ DO NOT use arrays for properties if the property would have to return a new array (e.g., a copy of an internal array) every time the property getter is called.

Implementing custom collections:

- ✔️ CONSIDER inheriting from `Collection<T>`, `ReadOnlyCollection<T>`, or `KeyedCollection<TKey,TItem>` when designing new collections.
- ✔️ DO implement `IEnumerable<T>` when designing new collections. Consider implementing `ICollection<T>` or even `IList<T>` where it makes sense.
  - Follow the API pattern of `Collection<T>` and `ReadOnlyCollection<T>` closely (same explicit members, same parameter names).
- ✔️ CONSIDER implementing nongeneric collection interfaces (`IList` and `ICollection`) if the collection will often be passed to APIs taking these interfaces as input.
- ❌ AVOID implementing collection interfaces on types with complex APIs unrelated to the concept of a collection.
- ❌ DO NOT inherit from nongeneric base collections such as `CollectionBase`. Use `Collection<T>`, `ReadOnlyCollection<T>`, and `KeyedCollection<TKey,TItem>` instead.

Naming custom collections (data structures such as `List<T>`, `LinkedList<T>`, `Stack<T>` are mostly internal; specialized collections such as `StringCollection` are exposed in APIs):

- ✔️ DO use the "Dictionary" suffix in names of abstractions implementing `IDictionary` or `IDictionary<TKey,TValue>`.
- ✔️ DO use the "Collection" suffix in names of types implementing `IEnumerable` (or any of its descendants) and representing a list of items.
- ✔️ DO use the appropriate data structure name for custom data structures.
- ❌ AVOID using any suffixes implying particular implementation, such as "LinkedList" or "Hashtable," in names of collection abstractions.
- ✔️ CONSIDER prefixing collection names with the name of the item type. For example, a collection storing items of type `Address` (implementing `IEnumerable<Address>`) should be named `AddressCollection`. If the item type is an interface, the "I" prefix of the item type can be omitted. Thus, a collection of `IDisposable` items can be called `DisposableCollection`.
- ✔️ CONSIDER using the "ReadOnly" prefix in names of read-only collections if a corresponding writeable collection might be added or already exists in the framework.
  - E.g. `ReadOnlyStringCollection`.

### Equality Operators

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/equality-operators

- ❌ DO NOT overload one of the equality operators and not the other.
- ✔️ DO ensure that `Object.Equals` and the equality operators have exactly the same semantics and similar performance characteristics.
  - Usually means overriding `Object.Equals` when overloading the operators.
- ❌ AVOID throwing exceptions from equality operators.
  - E.g. return false for a null argument instead of throwing `NullReferenceException`.

Equality operators on value types:

- ✔️ DO overload the equality operators on value types, if equality is meaningful.
  - Rationale: most languages have no default `operator==` for value types.

Equality operators on reference types:

- ❌ AVOID overloading equality operators on mutable reference types.
  - Rationale: built-in operators implement reference equality and developers are surprised by value equality; immutability makes the difference much harder to notice.
- ❌ AVOID overloading equality operators on reference types if the implementation would be significantly slower than that of reference equality.

## Common Design Patterns

### Dispose Pattern

URL: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern

Managed memory is released by the GC; other (unmanaged) resources must be released explicitly. `Object.Finalize` (the finalizer) runs before memory is reclaimed, but finalization is delayed to an undetermined time and postpones memory collection to a later GC round, so it is inappropriate when resources are scarce or must be reclaimed quickly. `IDisposable` gives a manual release path and `GC.SuppressFinalize` tells the GC finalization is unnecessary. The Dispose Pattern standardizes the use and implementation of finalizers and `IDisposable`, reducing the complexity of `Finalize` and `Dispose` sharing some but not all code paths.

- ✔️ DO implement the Basic Dispose Pattern on types containing instances of disposable types. See the Basic Dispose Pattern section for details on the basic pattern.
  - Rationale: if a type owns the lifetime of other disposables, users need a way to dispose them too.
- ✔️ DO implement the Basic Dispose Pattern and provide a finalizer on types holding resources that need to be freed explicitly and that do not have finalizers.
  - E.g. types storing unmanaged memory buffers.
- ✔️ CONSIDER implementing the Basic Dispose Pattern on classes that themselves don’t hold unmanaged resources or disposable objects but are likely to have subtypes that do.
  - E.g. `System.IO.Stream` is abstract and holds no resources but most subclasses do.

Basic Dispose Pattern: implement `IDisposable` and declare `Dispose(bool)` holding all cleanup logic shared between `Dispose` and the optional finalizer. The `disposing` parameter indicates whether the call came from `IDisposable.Dispose` (true) or the finalizer (false); other reference objects should only be accessed when `disposing` is true, because finalization order is unpredictable. If the base class already implements the pattern, just override `Dispose(bool)`.

```csharp
public class DisposableResourceHolder : IDisposable {

    private SafeHandle resource; // handle to a resource

    public DisposableResourceHolder() {
        this.resource = ... // allocates the resource
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (resource!= null) resource.Dispose();
        }
    }
}
```

- ✔️ DO declare a `protected virtual void Dispose(bool disposing)` method to centralize all logic related to releasing unmanaged resources.
  - All cleanup goes here; called from both the finalizer (false) and `IDisposable.Dispose` (true); use the flag to avoid touching other finalizable objects during finalization.
- ✔️ DO implement the `IDisposable` interface by simply calling `Dispose(true)` followed by `GC.SuppressFinalize(this)`.
  - Call `SuppressFinalize` only if `Dispose(true)` executes successfully.
- ❌ DO NOT make the parameterless `Dispose` method virtual.
  - Rationale: `Dispose(bool)` is the method subclasses should override.
- ❌ DO NOT declare any overloads of the `Dispose` method other than `Dispose()` and `Dispose(bool)`.
  - Rationale: `Dispose` is effectively a reserved word for this pattern; some languages may implement the pattern automatically.
- ✔️ DO allow the `Dispose(bool)` method to be called more than once. The method might choose to do nothing after the first call.
  - E.g. `if (disposed) return; ... disposed = true;`.
- ❌ AVOID throwing an exception from within `Dispose(bool)` except under critical situations where the containing process has been corrupted (leaks, inconsistent shared state, etc.).
  - Rationale: users expect `Dispose` not to throw; a throw would skip further finally-block cleanup. Never throw when `disposing` is false, as that terminates the process inside a finalizer.
- ✔️ DO throw an `ObjectDisposedException` from any member that cannot be used after the object has been disposed of.
- ✔️ CONSIDER providing method `Close()`, in addition to the `Dispose()`, if close is standard terminology in the area.
  - Make `Close` identical to `Dispose` and consider implementing `IDisposable.Dispose` explicitly:
  ```csharp
  public class Stream : IDisposable {
      IDisposable.Dispose() {
          Close();
      }
      public void Close() {
          Dispose(true);
          GC.SuppressFinalize(this);
      }
  }
  ```

Finalizable types extend the Basic Dispose Pattern by overriding the finalizer and providing a finalization code path in `Dispose(bool)`. Finalizers are hard to implement correctly because you cannot make normal assumptions about system state. If the base class is already finalizable and implements the pattern, do not override `Finalize` again; override `Dispose(bool)`.

```csharp
public class ComplexResourceHolder : IDisposable {

    private IntPtr buffer; // unmanaged memory buffer
    private SafeHandle resource; // disposable handle to a resource

    protected virtual void Dispose(bool disposing) {
        ReleaseBuffer(buffer); // release unmanaged memory
        if (disposing) { // release other disposable objects
            if (resource!= null) resource.Dispose();
        }
    }

    ~ComplexResourceHolder() {
        Dispose(false);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

- ❌ AVOID making types finalizable.
  - Rationale: real performance and complexity cost; prefer resource wrappers such as `SafeHandle`, which handle their own cleanup so no finalizer is needed.
- ❌ DO NOT make value types finalizable.
  - Rationale: only reference types are finalized; C# and C++ compilers enforce this.
- ✔️ DO make a type finalizable if the type is responsible for releasing an unmanaged resource that does not have its own finalizer.
  - The finalizer should simply call `Dispose(false)` with all cleanup in `Dispose(bool)`.
- ✔️ DO implement the Basic Dispose Pattern on every finalizable type.
  - Rationale: gives users deterministic cleanup of the same resources.
- ❌ DO NOT access any finalizable objects in the finalizer code path, because there is significant risk that they will have already been finalized.
  - Finalizers run in random order (apart from a weak ordering guarantee for critical finalization); statics referring to finalizable objects may be unsafe if `Environment.HasShutdownStarted` is true.
- ✔️ DO make your `Finalize` method protected.
  - C#, C++, and VB.NET compilers enforce this.
- ❌ DO NOT let exceptions escape from the finalizer logic, except for system-critical failures.
  - Rationale: as of .NET Framework 2.0 the CLR shuts down the whole process, preventing other finalizers from running.
- ✔️ CONSIDER creating and using a critical finalizable object (a type with a type hierarchy that contains `CriticalFinalizerObject`) for situations in which a finalizer absolutely must execute even in the face of forced application domain unloads and thread aborts.
