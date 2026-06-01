# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 2.0.0 / 2026-06-01
### Breaking
- The `Mutable{Record}` → `{Record}` conversion is now an **explicit** operator instead of implicit. This prevents hidden allocations (e.g. `record == mutable` silently built a record). Update call sites to an explicit cast `(Record)mutable`, or — preferably — call `mutable.Build()` / `mutable.ToImmutable()`. The `{Record}` → `Mutable{Record}` direction stays implicit.

### Added
- `MUTTY003` diagnostic: records nested inside another type are reported (and skipped) instead of generating broken code.
- Chainable `With{Property}` fluent setters on generated wrappers, e.g. `student.Produce(m => m.WithName("Jane").WithAge(30))`.
- Support for plain array (`T[]`) and read-only collection (`IReadOnlyList<T>` / `IReadOnlyCollection<T>`) properties: arrays are defensively copied; read-only collections are exposed as a mutable `List<T>`.
- `ToImmutable()` instance method on generated wrappers as a discoverable alias for `Build()`.
- `partial void OnBeforeBuild()` hook on generated wrappers, called at the start of `Build()`, as a validation/normalisation seam.
- BenchmarkDotNet harness (`Mutty.Benchmarks`) measuring cold vs incremental generator runs.
- Incremental-generator caching regression test asserting the record-model transform is cached on unrelated edits.

### Changed
- **Performance:** rebuilt the incremental pipeline around value-equatable models (`RecordModel`, `PropertyModel`, `EquatableArray<T>`) instead of flowing Roslyn symbols and `Collect()`-ing them. The generator now caches correctly and only regenerates records that actually changed.
- Merged the three generators into a single `MuttyGenerator` using `ForAttributeWithMetadataName`.
- Lowered the `Microsoft.CodeAnalysis.CSharp` floor from 5.0.0 to 4.8.0 so the generator runs on a much broader range of SDKs and Visual Studio versions.
- Generated wrappers now include properties inherited from base records.

### Fixed
- Nested mutable type names are namespace-qualified, so a record nesting another `[MutableGeneration]` record from a different namespace no longer fails with CS0246.
- Generic records now report a clear diagnostic (`MUTTY002`) instead of generating a malformed wrapper.
- Wrapping a record whose `ImmutableArray<T>` property is `default` no longer throws; it yields an empty list.
- Generated files now emit the `using` directives they need, so they compile when the consumer has `ImplicitUsings` disabled.
- `ImmutableDictionary`, `ImmutableSortedDictionary`, `ImmutableHashSet`, `ImmutableSortedSet`, `ImmutableQueue` and `ImmutableStack` properties now generate code that compiles and round-trips (previously CS1929).
- A nested record property is only treated as a mutable wrapper when that record is `[MutableGeneration]`-annotated; un-annotated nested records no longer cause CS0246.
- Packaging: the analyzer DLL is no longer double-shipped to `lib/`, and the package no longer carries an unused `Microsoft.CodeAnalysis.AnalyzerUtilities` dependency.
- Re-enabled the end-to-end compilation tests (rewritten against the real generated API) and CI now runs on pull requests to `main`/`develop`.

## 1.0.50 / 2026-02-20
### Fixed
- Fixed codegen to not add 'Mutable' prefix to built-in types in generic collections (#66)
- Fixed CI build by removing orphaned System.Formats.Asn1 reference (#45)
- Fixed OctoVersion.Tool runtime mismatch for .NET 10 (#67)

### Changed
- Updated NUnit from 4.3.2 to 4.5.0
- Updated NUnit3TestAdapter from 4.6.0 to 5.2.0
- Updated Roslynator.* from 4.12.10 to 4.15.0
- Updated Snapshooter.Xunit from 0.14.1 to 0.15.0
- Updated Spectre.Console from 0.49.1 to 0.54.0
- Updated FluentAssertions.Analyzers from 0.33.0 to 0.34.1
- Updated DotNet.ReproducibleBuilds from 1.2.25 to 1.2.39
- Updated Microsoft.CodeCoverage and Microsoft.NET.Test.Sdk to 17.14.1
- Updated coverlet.collector from 6.0.3 to 6.0.4
- Updated GitHub Actions: actions/checkout to v6, actions/cache to v5
- Updated GitHub Actions: actions/configure-pages to v5, actions/upload-pages-artifact to v4

## 1.0.* / 2025-04-10
### Added
- Added CHANGELOG
- Initial Release of the package