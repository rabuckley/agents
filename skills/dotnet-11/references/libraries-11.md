# .NET 11 libraries

Source: [What's new in .NET libraries for .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/libraries). Entries marked **(signature)** have no signature on that page; look the API up in the [.NET API browser](https://learn.microsoft.com/dotnet/api/) before writing the call; don't guess overloads.

Read **Behaviour changes** in full. For the other sections, sweep only those whose namespaces the codebase imports (grep the `using` directives first).

## Behaviour changes to check first

| Change | Search | Action |
| --- | --- | --- |
| `decimal` ↔ `double`/`float` and `BigInteger` → `double`/`float` conversions are now correctly rounded. No compat switch. `(decimal)1.23` keeps the double's exact binary value instead of producing `1.23`. | `(decimal)` casts of `double`/`float` values or literals, `Convert.ToDecimal(` on doubles, `new decimal(double)` | Use `m` literals; in money paths, keep values `decimal` end to end. Golden-value tests may change. |
| `DeflateStream`/`GZipStream` always write a header and footer, even for an empty payload. | Tests or protocols that check for zero compressed bytes | Check for emptiness before compressing, or update the expectation. |
| ZIP entries are CRC32-checked on read; corrupt or truncated entries throw `InvalidDataException`. | `ZipArchive` reads of untrusted or legacy archives | Handle `InvalidDataException` where partial reads used to be tolerated. |
| `MemoryCache` metrics tag renamed `dotnet.cache.request.type` → `dotnet.cache.request.result`. | Dashboards, alert queries, tests | Update. |
| HTTP `open_connections`/`active_requests` metrics are observable instruments. | `new MeterListener` | Call `RecordObservableInstruments()`. |
| `IStartupValidator` is obsolete. | `IStartupValidator` | Implement `IAsyncStartupValidator`. |
| `ChangeToken.OnChange` gains `Func<Task>` overloads; re-registration waits for the task. | `ChangeToken.OnChange(` with `async () =>` | Previously `async void`; now awaited. Check nothing relied on overlapping callbacks. |
| STJ source generation sets an `init` property only when present in the JSON, so its initializer survives. | `init;` properties with initializers on source-generated types | Usually the wanted behaviour; check tests. |
| `FORCE_COLOR` makes the console emit ANSI codes even when redirected. | CI logs | Usually wanted. |
| `ConfigurationBinder` binds an empty array to a constructor parameter instead of throwing; `PhysicalFilesWatcher` tolerates a missing root. | `try`/`catch` around binding or watching added for these cases | Remove the workaround. |
| iOS/tvOS `FileStream` takes no advisory locks by default. | Mobile apps relying on `FileShare` | `System.IO.DisableFileLocking=false` to restore. |
| Linux TLS sends alerts on certificate validation failure; HttpClient downgrades HTTP/2 to 1.1 for NTLM/Negotiate. | `request.Version = HttpVersion.Version11` set only to make Windows auth work | Remove the override. |
| `[RequiresUnsafe]` removed from many pointer APIs (`Buffer.MemoryCopy`, `Span<T>(void*, int)`, `Unsafe.AsRef/Read/Write/Copy`, `NativeMemory`, `Encoding` pointer overloads). | | No change needed; don't remove `AllowUnsafeBlocks` on the strength of this. |

## Process

| Signal | Replace with | Fit |
| --- | --- | --- |
| `RedirectStandardOutput = true` + `StandardOutput.ReadToEnd[Async]()` + `WaitForExit[Async]()` + `ExitCode`; `OutputDataReceived`/`BeginOutputReadLine` used only to collect output | `await Process.RunAndCaptureTextAsync("git", ["status", "--porcelain"])` → `result.StandardOutput`, `result.ExitStatus.ExitCode`; `Process.Run[Async]` when output isn't needed | Apply when the old code just runs and collects. Keep event-based code that reacts to lines as they arrive, or use `ReadAllLinesAsync(ct)` (yields `ProcessOutputLine`, separating stdout from stderr). |
| `CliWrap` or similar used only for run-and-capture | Same | **Propose**: dependency removal. |
| `try { Process.GetProcessById(id) } catch (ArgumentException)` | `if (Process.TryGetProcessById(id, out var p))` | Apply. |
| `[DllImport("libc")] kill(pid, SIGTERM)` | `process.Signal(PosixSignal.SIGTERM)`; `WaitForExitStatusAsync()` returns `ProcessExitStatus` (signal vs exit) | Unix only; keep the `OperatingSystem` guard. |
| `ExitCode > 128` decoded as a signal | `ProcessExitStatus` | |
| Windows Job Object P/Invoke (`CreateJobObject`, `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`) to kill children with the parent | `ProcessStartInfo.KillOnParentExit = true` | Windows only. |
| `setsid`/`nohup`/`DETACHED_PROCESS` wrappers | `ProcessStartInfo.StartDetached = true`; `Process.StartAndForget(...)` **(signature)** for a discarded `Process.Start` | |
| `CREATE_SUSPENDED` + `ResumeThread` P/Invoke | `StartSuspended = true`, then `process.SafeHandle.Resume()` | Windows and macOS only. |
| `CreateProcess` P/Invoke with `STARTUPINFO` handles; redirect-then-copy into a file | `ProcessStartInfo.StandardOutputHandle` (etc.) with an open `SafeFileHandle`; `InheritedHandles` to limit inheritance | |
| `OpenProcess`/`TerminateProcess`/`WaitForSingleObject` P/Invoke | `SafeProcessHandle.Open/TryOpen/Kill/Signal/WaitForExitAsync` | |

## Streams, text and encoding

| Signal | Replace with |
| --- | --- |
| `new MemoryStream(Encoding.UTF8.GetBytes(str))` | `new StringStream(str, Encoding.UTF8)` |
| `new MemoryStream(memory.ToArray())`, `MemoryMarshal.TryGetArray` + `MemoryStream`, hand-written `ReadOnlyMemoryStream` classes, CommunityToolkit `AsStream()` | `new ReadOnlyMemoryStream(memory)`; `WritableMemoryStream` over `Memory<byte>` (fixed size) |
| `sequence.ToArray()` then `MemoryStream` | `new ReadOnlySequenceStream(sequence)` |
| `Convert.ToBase64String`/`FromBase64String` in hot paths; `Encoding.UTF8.GetBytes(Convert.ToBase64String(x))` | `Base64.EncodeToString`, `EncodeToUtf8`, `DecodeFromChars`, `DecodeFromUtf8` (`System.Buffers.Text`) **(signature)**. Only where allocation matters; `Convert` is fine elsewhere. |
| `try { strictUtf8.GetString(b) } catch (DecoderFallbackException)` to find bad bytes; surrogate validation loops | `Utf8.IndexOfInvalidSubsequence(bytes)`, `Utf16.IndexOfInvalidSubsequence(chars)`, `Utf16.IsValid(chars)` |
| `char.ToUpperInvariant(a) == char.ToUpperInvariant(b)`, `a.ToString().Equals(b.ToString(), OrdinalIgnoreCase)` | `a.Equals(b, StringComparison.OrdinalIgnoreCase)` |
| `s.Contains(rune.ToString())`, `char.ConvertFromUtf32(cp)` as a search or split argument | `Rune` overloads of `Contains`, `StartsWith`, `EndsWith`, `IndexOf`, `LastIndexOf`, `Replace`, `Split`, `Trim*`; `TextInfo.ToUpper(Rune)` |
| `var d = new StringBuilder(sb.ToString()); sb.Clear();` to hand content over | `StringBuilder.MoveChunks(sb)` |
| `\r?\n` in patterns, `Replace("\r\n", "\n")` before matching, `[^\r\n]` in place of `.` | `RegexOptions.AnyNewLine` (changes `^`, `$`, `.`; check every match still means the same) |
| CBOR chunks concatenated before `new CborReader(` | Incremental reader: `CborReaderOptions { ConformanceMode = CborConformanceMode.Lax }`, `isFinalBlock: false`, `NeedsMoreData`, `SlideData` (Lax mode only) |
| `[StringSyntax("C#")]` literals | `StringSyntaxAttribute.CSharp`, `.FSharp`, `.VisualBasic` |
| `"video/mp4"` and other video MIME literals | `MediaTypeNames.Video.Mp4`, `.Mpeg`, `.Ogg`, `.QuickTime`, `.WebM` |
| `uri.Scheme == "data"` | `Uri.UriSchemeData` |

## System.Text.Json

| Signal | Replace with | Fit |
| --- | --- | --- |
| A list of `[JsonDerivedType(typeof(X), "x")]` on an abstract base whose derived types are all in the assembly | Make the base `closed` (see `csharp-15.md`) and use `[JsonPolymorphic(InferClosedTypePolymorphism = true)]` or the options/source-gen equivalent | Inferred discriminators won't match hand-written ones: a wire change, **propose**. Only valid on a `closed` type; explicit `[JsonDerivedType]` replaces the inferred list. |
| `[JsonDerivedType(typeof(Derived<int>))]`, `[JsonDerivedType(typeof(Derived<string>))]`... | `[JsonDerivedType(typeof(Derived<>), "derived")]` | Must resolve to exactly one closed type, else SYSLIB1229. |
| Custom `JsonConverter` that switches on `reader.TokenType` to read "int or string"; `OneOf` with custom converters | C# 15 `union` (serialized as the active case, no discriminator); `[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]` when cases are objects | Two object cases need a classifier; the structural one reads only root property names. A wire change if the old converter wrapped values: **propose**. |
| Hand-rolled JSON Lines: `await foreach` + `Serialize` + `"\n"` | `JsonSerializer.SerializeAsyncEnumerable(streamOrPipe, items, topLevelValues: true)` | Always LF, ignores `WriteIndented`. |
| `(JsonTypeInfo<T>)options.GetTypeInfo(typeof(T))` | `options.GetTypeInfo<T>()` / `TryGetTypeInfo<T>(out var info)` | Apply. |
| Custom `PascalCaseNamingPolicy` class | `JsonNamingPolicy.PascalCase` | Apply. |
| Many `[JsonPropertyName("snake_case")]` on one type, or separate options instances per naming style | `[JsonNamingPolicy(...)]` on the type or member | Precedence: `[JsonPropertyName]` > member policy > type policy > options. |
| `[JsonIgnore(Condition = WhenWritingNull)]` on every member of a type | Type-level `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` | `Always` isn't allowed at type level. |
| `JsonConverterFactory` whose only job is `MakeGenericType` | `[JsonConverter(typeof(OptionConverter<>))]` | Arity must match. |
| Public setters or constructors that exist only for the source generator; converters written because the generator rejected private members | `[JsonInclude]` on non-public members; non-public `[JsonConstructor]` | Apply where the visibility was widened only for serialization. |
| Mutable `Dictionary` kept for `[JsonExtensionData]`; `HashSet<T>` exposed only for serialization | `IReadOnlyDictionary<string, JsonElement>` / `JsonObject` extension data; `IReadOnlySet<T>` | |
| Converters for `BFloat16`, `Decimal32/64/128`; F# `FSharp.SystemTextJson` | Built in | F# DU support is reflection only (no source gen, trimming or AOT) and the wire format may differ: **propose**. |
| `new Utf8JsonWriter(...)` per document when only options differ | `writer.Reset(stream, newOptions)` after `Flush()` | Hot paths only. |

## Collections, LINQ and numerics

| Signal | Replace with | Fit |
| --- | --- | --- |
| `GroupJoin(...).SelectMany(g => g.DefaultIfEmpty(), ...)`; query syntax `join ... into g from x in g.DefaultIfEmpty()` | `LeftJoin(inner, outerKey, innerKey)` / `RightJoin`; returns tuples, missing side is `default` | Also on `Queryable` and `AsyncEnumerable`. MoreLINQ's `LeftJoin` may clash; qualify or drop MoreLINQ. For EF queries, check the generated SQL. |
| Full outer join written as a left join `Union`/`Concat` a right join | `FullJoin(...)` | EF Core 11 translates it to `FULL JOIN`. |
| `.Join(inner, o => o.K, i => i.K, (o, i) => (o, i))` or `new { o, i }` that only pairs | `.Join(inner, o => o.K, i => i.K)` returning tuples (also `GroupJoin`) | |
| `class FooByIdComparer : IEqualityComparer<Foo>` comparing one key | `EqualityComparer<Foo>.Create(f => f.Id)` | Delete the class when it's the only use. |
| `bits.Cast<bool>().Count(b => b)` | `bits.PopCount()` | |
| `new BitArray(span.ToArray())` | `new BitArray(span)` | |
| `(byte)rng.Next(...)`, `(float)rng.NextDouble()`, `NextBytes` + `BitConverter` for `ulong`/`UInt128` | `Random.Shared.NextInteger<T>(...)`, `NextBinaryFloat<T>()` | Seeded sequences change: leave seeded, reproducible generators alone. |
| `BitConverter.DoubleToInt64Bits(x).ToString("X16")`, `"R"`/`"G17"` for exact round-trip | `x.ToString("X")` + `double.Parse(hex, NumberStyles.HexFloat)` | Only where exact bits matter and the format is internal. |
| `Substring` to a delimiter then `int.Parse` in hand-written parsers | `INumberBase<T>.TryParsePartial` **(signature)** | Hot parsers only. |
| `System.Numerics.Complex` used with `float` data, MathNet `Complex32` | `Complex<float>` **(signature)** | |
| Manual BFloat16 bit-shifting | `BitConverter.BFloat16ToUInt16Bits`, `UInt16BitsToBFloat16`, `GetBytes(BFloat16)` | |
| `Sse2.UnpackLow/High`, `AdvSimd.Zip1/Zip2`, `Vector128.Shuffle` with constant indices, `Vector128.Create(1, 2, 4, 8)` tables | `Vector128.Zip/ZipLower/ZipUpper/Unzip*`, `Concat*`, `Reverse`, `CreateGeometricSequence`/`CreateAlternatingSequence` | **Report** unless benchmarks show parity. |
| Third-party IEEE decimal types | `Decimal32`, `Decimal64`, `Decimal128` | Not a replacement for `System.Decimal`. |
| `AnonymousPipeServerStream` used only to get raw handles; `pipe2`/`CreatePipe`/`GetFileType` P/Invoke | `SafeFileHandle.CreateAnonymousPipe(...)`, `SafeFileHandle.Type` | |
| `FileStream`/`PipeStream` wrapping a pipe only because `RandomAccess` rejected it | `RandomAccess.Read/Write` on the handle | |

## Compression and archives

| Signal | Replace with | Fit |
| --- | --- | --- |
| `GZipStream`/`DeflateStream`/`ZLibStream` over a `MemoryStream` to compress one buffer | `GZipEncoder`/`DeflateEncoder`/`ZLibEncoder` (and decoders) with `Compress(src, dst, out consumed, out written, isFinalBlock: true)`; `Reset()` to reuse | Hot paths only; check `OperationStatus`. |
| `ZstdSharp`, `ZstdNet` | `ZstandardStream`, `ZstandardEncoder` in `System.IO.Compression` | **Propose**: dependency swap; check compression level parity. |
| `DotNetZip`, `SharpZipLib`, `SharpCompress` used only for password-protected ZIPs | `ZipArchiveEntry.Open(password)`, `ZipEncryptionMethod` **(signature)** | **Propose**; confirm the encryption method the consumers need is supported. |
| Reading local header bytes to find an entry's compression method | `ZipArchiveEntry.CompressionMethod` | |
| `TarWriter` loop written only to choose `TarEntryFormat.Gnu`/`Ustar`; shelling out to `tar` | `TarFile.CreateFromDirectory[Async](..., format: TarEntryFormat.Gnu)` | |

## Networking and HTTP

| Signal | Replace with | Fit |
| --- | --- | --- |
| Custom `HttpContent` that gzips; `GZipStream` into `ByteArrayContent` plus `ContentEncoding.Add("gzip")` | `GZipCompressedContent`, `BrotliCompressedContent`, `ZstandardCompressedContent` **(signature)** | Only where the server accepts compressed request bodies. |
| `DnsClient` (`LookupClient.QueryAsync(..., QueryType.SRV)`), `nslookup`/`dig` shell-outs, `DnsQuery_W` | `Dns.ResolveSrvAsync`, `ResolveMxAsync`, `ResolveTxtAsync`, `ResolveCNameAsync`, `ResolvePtrAsync`, `ResolveNsAsync` → `DnsResult<T>`; `DnsResolver` for custom servers **(signature)** | **Propose**: dependency swap. |
| `PooledConnectionLifetime` set only to force DNS refresh; handlers recreated on a timer | `SocketsHttpHandler.ShouldEvictConnection` **(signature)** | **Report** unless the eviction rule is clear. |
| `SemaphoreSlim`/`DelegatingHandler` throttling the first burst to an HTTP/2 server | `SocketsHttpHandler.InitialHttp2MaxConcurrentStreams` | |

## Cryptography

| Signal | Replace with | Fit |
| --- | --- | --- |
| BouncyCastle `X25519Agreement`, `NSec` `KeyAgreementAlgorithm.X25519`, libsodium `ScalarMult` | `X25519DiffieHellman.GenerateKey()`, `DeriveRawSecretAgreement(other)` | **Propose**; Windows, Linux, macOS only. |
| BouncyCastle `AesWrapEngine`/`AesWrapPadEngine`, hand-rolled RFC 3394 over ECB | `aes.EncryptKeyWrap`/`DecryptKeyWrap`; `EncryptKeyWrapPadded`/`DecryptKeyWrapPadded` (RFC 5649) | Unpadded needs a multiple of 8 bytes. Verify against existing wrapped keys before switching. |
| Hand-rolled constant-time compare of a span against one byte | `CryptographicOperations.FixedTimeEquals(span, value)` | |
| `CompositeMLKem`, `Hpke` (SYSLIB5006, SYSLIB5009), `TlsContext`/`TlsSession` (SYSLIB5007) | — | Experimental: **report** only. |

## Extensions: configuration, options, caching, logging, validation

| Signal | Replace with | Fit |
| --- | --- | --- |
| Computed properties on options classes made non-public, or `[JsonIgnore]` misused, to stop binding | `[ConfigurationIgnore]` | |
| `AddSingleton<IValidateOptions<T>, V>()` with nothing chaining it to `AddOptions<T>()`; `.Validate(o => ...)` resolving services by hand | `.Validate<V>()` on the `OptionsBuilder` (V still registered in DI) | |
| `IValidateOptions<T>` that blocks on I/O; hosted services that exist only to validate options against the network | `IAsyncValidateOptions<T>`, `IAsyncStartupValidator` | Keep a synchronous `Validate` that fails clearly. |
| `ValidationAttribute.IsValid` or `IValidatableObject.Validate` calling `.Result`/`.Wait()`/`.GetAwaiter().GetResult()` | `AsyncValidationAttribute.IsValidAsync(value, context, ct)`, `IAsyncValidatableObject`; callers use `Validator.ValidateObjectAsync`/`TryValidateObjectAsync` | Every caller must switch to the async `Validator` methods, or the throwing sync path fires. ASP.NET Core: see `aspnetcore-11.md`. |
| `string.Format(template, name, min, max)` duplicating attribute placeholders | `attribute.FormatMessage(template, name)` | |
| Polling `MemoryCache.GetCurrentStatistics()` into custom gauges | `MemoryCacheOptions.TrackStatistics = true`; meter `Microsoft.Extensions.Caching.Memory.MemoryCache` | Pass an `IMeterFactory` for per-instance metrics. |
| `HybridCache.GetOrCreateAsync` followed by `SetAsync` to apply a value-dependent TTL | Options-aware `GetOrCreateAsync` factory overloads **(signature)** | |
| Custom `ActivityListener` with `ShouldListenTo`/`Sample` used to enable or disable sources | `services.AddTracing(t => t.EnableTracing(sourceName: ...).DisableTracing(sourceName: ..., operationName: ...))` | Check how it combines with the OpenTelemetry SDK in use before replacing. |
| `[LoggerMessage]` methods taking `object` or `Enum` (boxing); per-type duplicate log methods | Generic `[LoggerMessage]` partial methods with constraints (`where T : struct, Enum`) | `allows ref struct` isn't supported (SYSLIB1011). |

## Reflection

| Signal | Replace with |
| --- | --- |
| `t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)` then `GetGenericArguments()[0]` | `t.GetNullableUnderlyingType()` (also works on `TypeBuilder`, `TypeDelegator`) |
| `Dictionary<Assembly, MetadataLoadContext>` side tables | `MetadataLoadContext.GetLoadContext(asm)` |
| `ctor.DeclaringType.GetGenericArguments()` workaround | `ConstructorInfo.GetGenericArguments()` |
| Hosts patching `Assembly.Location` for shadow-copied assemblies | `AssemblyLoadContext.SetAssemblyLocationOverride(...)` (set once) |
