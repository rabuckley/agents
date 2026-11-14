# Test-quality principles research

Primary sources checked 2026-11-14. This note summarizes what the sources say about what makes a test worth keeping. The final section is an inference for this repository. Not reached: Beck's Medium essay (HTTP 403; its "trade a property only for one of greater value" line is known here only from search snippets, so it is not relied on), and the body of Khorikov's book chapter 4 (only the headings were readable, so the pillar wording below comes from his blog).

## What the sources state

### Sensitive to behaviour

- **Beck: Behavioral.** "Tests should be sensitive to changes in the behavior of the code under test. If the behavior changes, the test result should change." [Test Desiderata](https://testdesiderata.com/)
- **Khorikov: catching regressions.** A valuable test "has a high chance of catching a regression bug". He excludes trivial code: such tests "just don't provide a sensible chance of finding a regression." [Unit tests value proposition (2016)](https://enterprisecraftsmanship.com/posts/unit-test-value-proposition/). The book names the pillar "protection against regressions". [Manning ch. 4](https://livebook.manning.com/book/unit-testing/chapter-4)
- **Meszaros: Safety Net.** "Missing tests are like holes in the safety net. Incomplete assertions are like broken strands." [Goals of Test Automation](http://xunitpatterns.com/Goals%20of%20Test%20Automation.html)
- **Google TotT: choose values that can fail.** A test can pass because its value "happens to match" a default. Use non-default values, and "different values for each input", so the code cannot "reuse a single input or switch their order." [Choosing Values for Robust Tests (2026)](http://testing.googleblog.com/2026/06/choosing-values-for-robust-tests.html)
- **Coverage is not sensitivity.** Coverage "only measures that a line was invoked, not what happened as a result". [SWE at Google ch. 11, "A Note on Code Coverage"](https://abseil.io/resources/swe-book/html/ch11.html). It "does not check that your tests are actually able to detect faults". [PIT](https://pitest.org/)
- **Mutation testing makes the check operational.** It inserts "small faults into programs and measur[es] the ability of the test suite to detect them". [Petrović & Ivanković, State of Mutation Testing at Google (ICSE SEIP 2018)](https://research.google/pubs/state-of-mutation-testing-at-google/). Limit: at scale only filtered, review-time mutants are actionable. [Practical Mutation Testing at Scale (2021)](https://arxiv.org/abs/2102.11378)
- **TigerStyle: test the negative space.** "Tests must test exhaustively, not only with valid data but also with invalid data, and as valid data becomes invalid". The reason given: most catastrophic failures in the cited study "could have been prevented by simple testing of error handling code." [TIGER_STYLE.md, "Testing and Simulation"](https://raw.githubusercontent.com/tigerbeetle/tigerbeetle/main/docs/TIGER_STYLE.md)

### Insensitive to structure

- **Beck: Structure-insensitive.** "Tests should not change their result if the structure of the code changes." [Test Desiderata](https://testdesiderata.com/)
- **Google: test via public APIs.** "Write tests that invoke the system being tested in the same way its users would". Reason: "change that breaks a test might also break a user." Limit: defining the public API "is more art than science". A helper class serving one or two classes is tested through them. [SWE ch. 12, "Test via Public APIs"](https://abseil.io/resources/swe-book/html/ch12.html)
- **Google: unchanging tests.** After a test is written, it "never needs to change unless the requirements of the system under test change". Pure refactorings, new features and bug fixes should leave existing tests alone. [SWE ch. 12, "Strive for Unchanging Tests"](https://abseil.io/resources/swe-book/html/ch12.html). Setup may change when a dependency is added, "but the actual test itself typically shouldn't need to change". [TotT: Test Behavior, Not Implementation (2013)](https://testing.googleblog.com/2013/08/testing-on-toilet-test-behavior-not.html)
- **Google: state over interactions.** "Interaction tests check how a system arrived at its result, whereas usually you should care only what the result is." [SWE ch. 12](https://abseil.io/resources/swe-book/html/ch12.html). Interaction testing is appropriate when "you cannot perform state testing" or when "differences in the number or order of calls to a function would cause undesired behavior". Do it "only for functions that are state-changing", since checking non-state-changing calls "is usually redundant". Avoid "overspecifying which functions and arguments are validated." [SWE ch. 13](https://abseil.io/resources/swe-book/html/ch13.html). Testing implementation details is sometimes right, for example to show that a read comes from a cache, "but this should be less common". [TotT 2013](https://testing.googleblog.com/2013/08/testing-on-toilet-test-behavior-not.html)
- **Khorikov: false positives come from coupling.** "A false positive is a situation where your test suite raises a false alarm". The remedy is "decoupling your tests from the SUT's implementation details as much as possible." [Value proposition](https://enterprisecraftsmanship.com/posts/unit-test-value-proposition/). Mock only *unmanaged* out-of-process dependencies. "Communications with managed dependencies are implementation details", while "the way your system talks to the external world forms the observable behavior". [When to mock (2020)](https://enterprisecraftsmanship.com/posts/when-to-mock/). Test only the public API. A complex private method should be extracted into its own class. [Unit testing private methods (2017)](https://enterprisecraftsmanship.com/posts/unit-testing-private-methods/)
- **Meszaros: Fragile Test.** A test that "fails to compile or run when the SUT is changed in ways that do not affect the part the test is exercising". Two of its causes: Overspecified Software, where tests "describe how the software should do something, not what it should achieve"; and Sensitive Equality, where objects are compared as strings. [Fragile Test](http://xunitpatterns.com/Fragile%20Test.html)
- **Fowler: two schools.** Mockist tests check "the outbound calls of the SUT" and couple to implementation. Fowler stays "a old fashioned classic TDDer". [Mocks Aren't Stubs](https://martinfowler.com/articles/mocksArentStubs.html). What counts as a unit is "situational". [UnitTest](https://martinfowler.com/bliki/UnitTest.html)
- **Fidelity of doubles.** Preference order: real implementation, then fake, then stub, then interaction testing. A fake must be faithful "from the perspective of the test" and needs its own tests. [SWE ch. 13](https://abseil.io/resources/swe-book/html/ch13.html). A mock's stubbed behaviour "may diverge from the behavior of the real implementation". A mock is still useful for "a code path that is hard to trigger". [TotT: Increase Test Fidelity By Avoiding Mocks (2024)](http://testing.googleblog.com/2024/02/increase-test-fidelity-by-avoiding-mocks.html). Warning sign: "mocking out more than one or two classes". [TotT: Don't Overuse Mocks (2013)](https://testing.googleblog.com/2013/05/testing-on-toilet-dont-overuse-mocks.html)
- **matklad (TigerBeetle): test features, not code.** "Test at the boundaries", because tests "make it costlier to delete (or change)" the code they touch. A single `check` helper absorbs churn in the API's signature. [How to Test](https://matklad.github.io/2021/05/31/how-to-test.html)

### Expected values from an independent source

- **Google TotT: change-detector tests.** A test that is "a transformation of the same information in the code under test" breaks "in response to any change to the production code, without verifying correct behavior". "A correct or incorrect program is equally likely to pass a test that is a derivative of the code under test." Such tests give "negative value" and "should be re-written or deleted." [Change-Detector Tests Considered Harmful (2015)](https://testing.googleblog.com/2015/01/testing-on-toilet-change-detector-tests.html)
- **Hughes: "Avoid replicating your code in your tests."** A replica is "expensive" and "of low value, because there is a grave risk that misconceptions in the implementation will be replicated in the test code." Limit: a model-based oracle works when the model is simpler. If "the model resembles the implementation so closely that the same bugs are likely in each", use metamorphic properties instead. [How to Specify It! §2, §5.3 (TFP 2019)](https://research.chalmers.se/publication/517894)
- **Meszaros: Production Logic in Test.** An expected value computed with the SUT's own logic. [Conditional Test Logic](http://xunitpatterns.com/Conditional%20Test%20Logic.html). Google: "stick to straight-line code over clever logic". [SWE ch. 12, "Don't Put Logic in Tests"](https://abseil.io/resources/swe-book/html/ch12.html)

### Deterministic and hermetic

- **Beck:** Deterministic means "if nothing changes, the test result shouldn't change". Isolated means the same result "regardless of the order in which they are run". [Test Desiderata](https://testdesiderata.com/)
- **Fowler:** non-deterministic tests "are useless" and "a virulent infection". The remedies he lists: isolate tests; "never use bare sleeps… use a callback or polling"; "always wrap the system clock"; use test doubles plus contract tests for remote services. [Eradicating Non-Determinism in Tests](https://martinfowler.com/articles/nonDeterminism.html)
- **Google:** "as you approach 1% flakiness, the tests begin to lose value". A test "should contain all of the information necessary to set up, execute, and tear down its environment." [SWE ch. 11](https://abseil.io/resources/swe-book/html/ch11.html). Meszaros lists the causes of an Erratic Test: Interacting Tests, Resource Leakage, Resource Optimism, Unrepeatable Test, Nondeterministic Test. [Erratic Test](http://xunitpatterns.com/Erratic%20Test.html)
- **IO is what makes tests impure.** "Purity corresponds to the amount of generalized IO the test is doing"; "Ruthlessly optimize purity", and "let the tests have their natural extent". [matklad: Unit and Integration Tests (2022)](https://matklad.github.io/2022/07/04/unit-and-integration-tests.html). TigerBeetle has the program "run at its own pace" rather than react to external events directly. It also warns that "a fuzzer can prove only the presence of bugs". [TIGER_STYLE.md, "IO"](https://raw.githubusercontent.com/tigerbeetle/tigerbeetle/main/docs/TIGER_STYLE.md). Bernhardt uses simple values "as the boundaries between components". [Boundaries](https://www.destroyallsoftware.com/talks/boundaries)

### Clear on reading and on failure

- **Beck:** Readable means "invoking the motivation for writing this particular test". Specific means "if a test fails, the cause of the failure should be obvious." [Test Desiderata](https://testdesiderata.com/)
- **Google:** a clear test is one "whose purpose for existing and reason for failing is immediately clear". Its body holds "all of the information a reader needs" and nothing distracting. The rule is DAMP, not DRY, though "DAMP is not a replacement for DRY". Write a test per behaviour, not per method. A failure message should give "the desired outcome, the actual outcome, and any relevant parameters." [SWE ch. 12](https://abseil.io/resources/swe-book/html/ch12.html)
- **Meszaros:** Obscure Test, caused by Mystery Guest ("not able to see the cause and effect between fixture and verification logic"), Irrelevant Information, General Fixture or Eager Test. [Obscure Test](http://xunitpatterns.com/Obscure%20Test.html). Assertion Roulette: "hard to tell which of several assertions… caused a test failure". [Assertion Roulette](http://xunitpatterns.com/Assertion%20Roulette.html). Defect Localization: small tests "pinpoint the bug". [Goals](http://xunitpatterns.com/Goals%20of%20Test%20Automation.html)

### Worth its cost, with explicit trade-offs

- **Beck:** "Some properties interfere with each other. Making tests more predictive of production behavior makes them slower." "Sometimes… properties only seem to interfere". [Test Desiderata](https://testdesiderata.com/). Khorikov: "It's impossible to maximize one of them without damaging the others." [Value proposition](https://enterprisecraftsmanship.com/posts/unit-test-value-proposition/)
- **Duplication and gaps.** "Push your tests as far down the test pyramid as you can" and remove higher-level duplicates. Keep them "if a higher-level test gives you more confidence". [Vocke, Practical Test Pyramid](https://martinfowler.com/articles/practical-test-pyramid.html). On gaps: "test everything that you don't want to break". [SWE ch. 11, "The Beyoncé Rule"](https://abseil.io/resources/swe-book/html/ch11.html)
- **Properties and examples.** Property tools test "against a specification, rather than a set of examples". "Validity properties miss many bugs (five of eight)", while "every bug is found by at least one postcondition, metamorphic property, and model-based property" (§5.1). [Hughes](https://research.chalmers.se/publication/517894). Enumerate the whole input space when it is small. Expect/snapshot tests update their expected value in place. [matklad: How to Test](https://matklad.github.io/2021/05/31/how-to-test.html); [TigerBeetle snaptest (`SNAP_UPDATE=1`)](https://raw.githubusercontent.com/tigerbeetle/tigerbeetle/main/src/stdx/testing/snaptest.zig)

## Where sources conflict or trade off

- **Fidelity against speed and determinism.** Real collaborators raise fidelity, but they "can be slow, non-deterministic, or difficult to instantiate" (TotT 2024). Beck's Predictive and Fast pull the same way. Composability is his way out.
- **Exact whole-output comparison.** Meszaros (Sensitive Equality) and Google ch. 11 ("over-specify expected outcomes") count it as fragile. matklad and TigerBeetle use expect tests as a default, because updating them costs one command. That makes the fragility cheap but weakens independence: the new expected value is the code's own output unless someone reviews the diff.
- **Mocks.** Fowler treats mockist testing as a legitimate school while preferring the classical one. Google and Khorikov restrict interaction checks to calls that are the contract: state-changing calls, calls whose count or order matters, and unmanaged external systems.
- **Helpers.** DAMP (Google) and matklad's `check` helper are compatible. A helper that absorbs churn in the interface aids structure-insensitivity. A helper that hides a relevant value breaks clarity.
- **Localization against minimality.** Meszaros's Defect Localization and Beck's Specific justify keeping tests that a broader test subsumes. The Test Pyramid's no-duplication rule pushes the other way.

## Inference for this repository

The five principles the sources agree on:

1. **Fails when the claimed behaviour breaks.** Sources: Beck Behavioral, Khorikov regressions, Meszaros broken strands, TotT values. The check: name a plausible wrong implementation (a hand mutant) that the test would let through.
2. **Survives changes that keep the behaviour.** Sources: Beck Structure-insensitive, Google unchanging tests, Khorikov false positives. Assert the outcomes callers rely on. A call counts as an outcome only when it is the contract: state-changing, count- or order-sensitive, or crossing to an unmanaged external system.
3. **Gets its expected values from outside the implementation.** Sources: the change-detector post, Hughes, Meszaros.
4. **Gives the same result every run.** Sources: Beck, Fowler, Google, matklad's purity. Remedies sit at the IO boundary.
5. **Clear on reading and on failure.** Sources: Beck Readable and Specific, Google clear tests, Meszaros Obscure Test.

Above them sits one rule: the properties trade off, so a test that gives one up must buy something worth more (Beck, Khorikov). That rule is what decides deletion and gaps.

The skill's yardstick already restates principles 1 and 2 in Beck's own words. His pair "Behavioral / Structure-insensitive" is crisper than "the behaviour it claims… the interface it tests".

How the current criteria map:

| # | Criterion | Principle | Backing |
| --- | --- | --- | --- |
| 1 | No unstable seams | 2 | Google public APIs (helper tested via its users), Khorikov, Vocke. The sources' remedy for an expensive helper is extraction, which the skill dropped. |
| 2 | State over interaction | 2 | Strong. Google's "only for functions that are state-changing" is clearer than "a query for data". |
| 3 | Assert what the test claims | 1 (under), 2 (over) | Under: Meszaros, TotT values. Over: Fragile Test, ch. 13. No source states the exact-form carve-out. It is derived from "the same way its users would". |
| 4 | Deletion with proof | cost rule | Change-detector "re-written or deleted"; Vocke; Meszaros/Beck for the localization exception. The subsumption evidence bar is local. |
| 5 | High-value gaps only | 1, cost rule | Beyoncé rule; TigerStyle error paths; coverage-is-not-quality. The "consequential scenario" threshold is local, from evals 09 and 13. |
| 6 | Legible in isolation | 5 | Strong: Google complete/concise and DAMP; Meszaros Mystery Guest. |
| 7 | Determinism and isolation | 4 | Strong: Fowler remedies map one-to-one. |
| 8 | Property/table/snapshot | 1, 5 | Hughes, matklad, TotT parameterization. The "roughly five" threshold is local (eval 06). "Snapshot never the default" conflicts with matklad/TigerBeetle. |
| 9 | Failure diagnostics | 5 | Strong: Google failure messages, Beck Specific, Assertion Roulette. |
| 10 | Independent expected values | 3 | Strong. The change-detector line "a correct or incorrect program is equally likely to pass" is clearer than the skill's long paragraph. |

No criterion lacks backing. What no source backs is the eval-derived machinery: the evidence bar, the five-example trigger and the gap threshold. Those correct observed Claude failures (evals README, "What the baseline cut"). They should stay as operational rules under the principles, not be re-argued as principles.

Source principles the skill lacks:

- **Fidelity of doubles.** Fakes need their own tests, and stubbed behaviour can diverge from the real thing. This is the missing reason behind case 07's hold, and it fits principle 1.
- **Speed and purity.** Beck Fast, Khorikov fast feedback, matklad purity. Arguably safe to omit: the baseline already weighs test level soundly.
- **Value choice.** The TotT rule on default and duplicate values is a cheaper, concrete form of criterion 3's "wrong result that would pass". It would have flagged case 05's no-op sweep.
- **Mutation testing.** Naming it as the operational form of principle 1 gives the evidence artifact a recognised name.
