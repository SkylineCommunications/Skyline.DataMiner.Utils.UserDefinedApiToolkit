# Feature: Path Variables (Route Parameters)

## Goal

Support ASP.NET-Core-style path variables in the UserDefinedApiToolkit, e.g.:

```csharp
[Route("items")]
public class ItemsController : ControllerBase
{
    [HttpGet("{id}")]
    public IApiResult GetById(int id) { ... }

    [HttpGet("{id}/details")]
    public IApiResult GetDetails([FromRoute(Name = "id")] int itemId) { ... }
}
```

## Agreed design decisions (from grilling session)

1. **Template combination** — `[Route]` stays class-level (mandatory, as today). `Http*` attributes
   (`HttpGetAttribute`, etc.) gain a new optional positional constructor:
   `public HttpGetAttribute(string template = "")`. The method template is appended to the
   controller route to form the full combined route (e.g. `items` + `{id}` → `items/{id}`).
2. **Parameter binding**:
   - Implicit: an unattributed method parameter whose name matches a `{placeholder}` in the
     combined template binds automatically from the route.
   - Explicit: new `[FromRoute]` attribute, with a `Name` property for overrides
     (`[FromRoute(Name = "id")]`) when the C# parameter name differs from the placeholder name.
   - **`[FromQuery]` also gets a `Name` property**, for the same override use case with query
     strings (e.g. `[FromQuery(Name = "q")]`). Binding logic (`HandleQueryParam` and the new route
     equivalent) uses `Name` when set, else falls back to the parameter's own name.
3. **Route matching / ranking** — Replace the current exact-string match in
   `RouteHandlerInfo.GetRank` with segment-aware matching. Literal segment matches outrank
   `{placeholder}` segment matches (e.g. `items/count` beats `items/{id}` for a request to
   `items/count`). Ties still throw `AmbiguousRouteException` (existing behavior, unchanged).
4. **Type conversion** — Route parameter values reuse the existing `StringValueConverter` (same
   converter used today for query parameters), including `InvalidParameterException` on failed
   conversion.
5. **Explicitly out of scope for this iteration** (do not implement, just leave extensible):
   - Route constraints (`{id:int}` template syntax) — typing comes purely from the C# parameter
     type, not the template.
   - Catch-all/wildcard segments (`{*path}`).
6. **Validation** — Eager, at `UserDefinedApiBuilder.Build()` time (consistent with existing
   `IInputConverter` validation there): every `{placeholder}` in a combined template must have a
   matching parameter (implicit name match, or `[FromRoute(Name=...)]`), and every `[FromRoute]`
   parameter must reference a placeholder that actually exists in the template. Mismatches throw a
   new `InvalidRouteException` immediately, not at request time.
7. **OpenAPI generator** (`UserDefinedApiToolkit.Build`) — `ControllerUnit`/`PathBuilder` must
   combine controller + method templates into the generated OpenAPI path (currently only the
   controller-level route is used). `OperationProvider` must detect path parameters **implicitly**
   by matching parameter names (or `[FromRoute(Name=...)]`) against template placeholders — not
   only via the explicit `[FromRoute]` attribute — reusing the existing (currently dead)
   `ParameterLocation.Path` mapping in `GetParameterLocation`.

## Note on ordering

The attribute surface (Phase 1) is implemented **before** the failing tests (Phase 0), not after,
because the test fixtures reference new attributes (`[FromRoute]`, `Name` properties,
`HttpGet("{id}")` template ctor) that must exist to compile. Phase 1 is purely additive/structural
(defaults preserve current behavior), so it doesn't skip the red→green spirit — the actual
matching/binding/validation logic still goes red in Phase 0 and green incrementally after.

Also: never run the `ApiChanges`/`PublicChanges` snapshot test during this work — the repo owner
runs and accepts it manually at the end of the feature.

## Workflow: TDD, red → green

Write the failing tests first for the whole feature, confirm they fail (red), then implement
production code incrementally so tests flip to green one by one. Do **not** write production code
before its corresponding test exists and fails for the right reason (missing feature, not a
compile error/typo).

## Progress tracking

Use the `todos` SQL table (already tracked in this session) as the authoritative checklist;
this file is the narrative/reference companion. Update both together.

### Phase 0 — Test scaffolding (red)
- [x] Add test fixture controllers under `UserDefinedApiToolkit.Tests/Runtime/PathVariables/TestFiles/`
      with path-variable routes:
  - `Controller_PathVariables.cs` — implicit `{id}` binding, explicit `[FromRoute(Name=...)]`
    override, literal `count` route (vs `{id}`) for precedence testing, `[FromQuery(Name=...)]`
    override combined with a route parameter.
  - `Controller_PathVariables_MissingParameter.cs` — `{id}` placeholder with no bound parameter
    (expected to fail eager `Build()` validation once implemented).
  - `Controller_PathVariables_UnmatchedFromRoute.cs` — `[FromRoute(Name="id")]` referencing a
    placeholder that doesn't exist in the template (expected to fail eager `Build()` validation).
  - Verified: test project builds cleanly with these fixtures (they only use the Phase 1 attribute
    surface, no behavior implemented yet).
- [x] Add `RouteHandlerInfoTests`/`RouteSelectorTests` (or extend `ControllerTests.cs`) covering:
  - combined template building (controller + method)
  - implicit placeholder-to-parameter binding
  - `[FromRoute(Name=...)]` override binding
  - `[FromQuery(Name=...)]` override binding
  - type conversion failures (`InvalidParameterException`)
  - rank/precedence: literal segment beats placeholder segment
  - eager `Build()` validation failures (`InvalidRouteException`) for placeholder/parameter
    mismatches
  _(Implemented as `UserDefinedApiToolkit.Tests/Runtime/PathVariables/PathVariableTests.cs`. Added
  a minimal `InvalidRouteException` stub — mirroring `InvalidControllerException` — purely so the
  assertions compile; no throwing logic implemented yet.)_
  **Confirmed red:** 8/8 new tests fail — 6 via `NoRouteException` (route combination/matching not
  implemented) and 2 via "no exception was thrown" (eager validation not implemented). Right
  reasons, not compile errors.
- [x] Add `UserDefinedApiToolkit.Tests/Build/` tests for OpenAPI generation: combined path in
      `doc.Paths`, implicit path-parameter detection, `Name`-override reflected in the spec.
      _(Added `TestFiles/PathVariableController.cs` fixture; extended `ControllerUnitTests.cs` with
      a `GetRoute(MethodInfo)` combined-template overload (stubbed to compile, ignores the method
      template for now); extended `OperationProviderTests.cs` for implicit/explicit path-parameter
      detection; added new `PathBuilderTests.cs` verifying per-method combined path entries.)_
      **Confirmed red:** 5/13 new+existing tests in this area fail for the right behavioral reasons
      (stubbed `GetRoute` ignores method template, `GetParameterLocation` doesn't yet detect
      implicit/`[FromRoute]` path params, `PathBuilder` still groups all methods under one
      controller-level path). Not compile errors.
- [x] Run full test suite, confirm new tests fail for the right reason (missing types/attributes,
      not existing behavior regressions).
      **Result:** 13 failed / 79 passed / 92 total (`ApiChanges` excluded). The 13 failures are
      exactly the new path-variable tests (8 in `PathVariableTests.cs`, 5 across
      `ControllerUnitTests`/`OperationProviderTests`/`PathBuilderTests`); all 79 pre-existing tests
      still pass — clean, isolated red state, zero regressions. Ready to start Phase 2.

### Phase 1 — Attribute surface
- [x] Add `template` constructor param (`string template = ""`) to `HttpMethodAttribute` and all
      derived attributes (`HttpGetAttribute`, `HttpPostAttribute`, `HttpPutAttribute`,
      `HttpDeleteAttribute`, `HttpPatchAttribute`, and any others).
      _(`HttpPatchAttribute` is currently fully commented out/disabled in the repo — left as-is,
      out of scope.)_
- [x] Add new `FromRouteAttribute` (parameter-targeted, `Name` property).
- [x] Add `Name` property to existing `FromQueryAttribute`.
- [x] Verified: build succeeds, 78/78 tests pass (`ApiChanges` excluded), no regressions from the
      additive/backward-compatible changes.

### Phase 2 — Route template model
- [x] Introduce a route template parser/model (e.g. `RouteTemplate`) that splits a combined
      template into segments, identifying literal vs `{placeholder}` segments.
      _(Added `Routes/RouteSegment.cs` (literal/placeholder segment) and `Routes/RouteTemplate.cs`
      (`Parse`, `Combine`, `PlaceholderNames`). `RouteHandlerInfo` exposes a lazily-parsed
      `Template` property for Phase 3 to consume.)_
- [x] Update `UserDefinedApiBuilder.AddController` to combine controller `[Route]` + method
      `[Http*]` template into the full route passed to `RouteHandlerInfo`.
      **Verified:** build succeeds; full suite now 12 failed / 80 passed (was 13/79) — the
      literal-only route test (`Get_LiteralSegmentTakesPrecedenceOverPlaceholder`) already flipped
      green from template combination alone (exact string match, no placeholders involved). All
      other path-variable tests still correctly red pending Phase 3+.
- [x] Removed the redundant `RouteHandlerInfo.Route` string property — `Template` (eagerly parsed
      in the constructor) is now the single source of truth; the temporary string-equality check
      in `GetRank` reads `Template.Raw` instead. **Verified:** build succeeds, full suite still
      12 failed / 80 passed — no regression.

### Phase 3 — Matching & ranking
- [x] Rework `RouteHandlerInfo.GetRank` (or extract a segment-matching helper) to match the
      incoming request path against the route template segment-by-segment, extracting route
      values, and scoring literal matches higher than placeholder matches.
      _(Added `TryMatchSegments`/`SplitSegments` helpers: segment-count must match exactly,
      literal segments require an exact ordinal match, placeholder segments match any value and
      are captured by name. `GetRank` weights `literalMatches * 100` so a fully-literal route
      always outranks a placeholder route for the same request, regardless of query/body scoring.
      Type conversion is intentionally NOT attempted during ranking — only during binding — so an
      unconvertible route value doesn't cause a false "no match", per
      `Get_WithInvalidRouteParameter_ThrowsInvalidParameterException`.)_
- [x] Ensure `RouteSelector` ambiguity behavior is unchanged (same tie-breaking exception).
      _(Untouched — `RouteSelector` still just groups by `GetRank` and throws
      `AmbiguousRouteException` on ties.)_

### Phase 4 — Parameter binding
- [x] Update `RouteHandlerInfo.Invoke` to bind `[FromRoute]`-attributed and implicit
      placeholder-matching parameters from the extracted route values, converting via
      `StringValueConverter` (mirroring `HandleQueryParam`).
      _(Added `HandleRouteParam`; `Invoke` now checks, in order: framework params, `[FromBody]`,
      explicit `[FromRoute(Name=...)]` (falls back to parameter name), implicit route-placeholder
      match by parameter name, explicit `[FromQuery(Name=...)]`, DI, then query fallback.)_
- [x] Update `HandleQueryParam` (and any binding helper) to respect `[FromQuery(Name=...)]`.
      _(`HandleQueryParam` now takes the resolved query name as a parameter; `GetRank`'s query
      scoring branch also uses the `[FromQuery(Name=...)]` override when present.)_
      **Verified:** build succeeds; full suite now 7 failed / 85 passed (was 12/80) — all 6
      `PathVariableTests` runtime binding/matching/precedence/conversion tests pass. Remaining 7
      failures are exactly the Phase 5 (2, eager validation) and Phase 6 (5, OpenAPI) items.

### Phase 5 — Eager validation
- [x] Add `InvalidRouteException` (under `Exceptions/`).
      _(Already scaffolded as a compile-enabling stub in Phase 0; no changes needed — it mirrors
      `InvalidControllerException`.)_
- [x] Add validation pass in `UserDefinedApiBuilder.Build()`: every placeholder has a bound
      parameter and vice versa, across all registered routes.
      _(Added `ValidateRouteParameters(RouteHandlerInfo)`, called for every handler in `Build()`
      right after the existing input-converter validation. Checks, per handler: (1) every
      `[FromRoute(Name=...)]` parameter's resolved name exists among the template's placeholders;
      (2) every placeholder has a bound parameter — implicit name match, or an explicit
      `[FromRoute(Name=...)]` match. Mirrors the same "is this parameter route-bound" semantics
      used by `GetRank`/`Invoke`.)_
      **Verified:** build succeeds; full suite now 5 failed / 87 passed (was 7/85) — both
      `Build_WithPlaceholderMissingBoundParameter_ThrowsInvalidRouteException` and
      `Build_WithFromRouteReferencingUnknownPlaceholder_ThrowsInvalidRouteException` pass. Only
      the 5 Phase 6 (OpenAPI) tests remain red.

### Phase 6 — OpenAPI generator
- [x] Update `ControllerUnit`/relevant model to expose the method-level template alongside the
      controller route.
      _(Replaced the Phase-0 stub: `ControllerUnit.GetRoute(MethodInfo)` now reads the method's
      `[Http*]` template via reflection-only `CustomAttributeData` (matching any of
      `HttpGet/Post/Put/Delete/Patch/Head/OptionsAttribute` by name, consistent with
      `OperationProvider.TryGetHttpMethod`) and combines it with `GetRoute()` via a local
      `CombineRoutes` helper — duplicated logic, not shared with the main assembly's
      `Routes.RouteTemplate.Combine`, since `UserDefinedApiToolkit.Build` intentionally has no
      project reference to the main assembly (works via `MetadataLoadContext`-style reflection
      only).)_
- [x] Update `PathBuilder` to build the combined path per operation (not just the controller
      route) when adding to `doc.Paths`.
      _(`HandleController` now computes `path` per-method via `unit.GetRoute(method)` inside the
      loop (was: once per controller, using only `unit.GetRoute()`), so methods with different
      templates land in separate `doc.Paths` entries; methods sharing a path still merge into the
      same `OpenApiPathItem`.)_
- [x] Update `OperationProvider.GetParameters`/`GetParameterLocation` to detect path parameters
      implicitly via template placeholder matching (with or without `[FromRoute]`), respecting
      `Name` overrides for both `[FromRoute]` and `[FromQuery]`.
      _(Added `TypeHelper.GetRoutePlaceholders`/`GetNamedArgumentValue`; replaced
      `GetParameterLocation` with `GetParameterLocationAndName`, which resolves both the OpenAPI
      parameter `In` location and its `Name` — using the attribute's `Name` override when present,
      falling back to the parameter name, and falling back further to implicit placeholder-name
      matching for unattributed parameters.)_
      **Verified:** build succeeds; full suite is now **0 failed / 92 passed** — all path-variable
      feature tests green, zero regressions across the whole feature.

### Phase 7 — Docs & cleanup
- [x] Update `README.md` (main project) with path variable usage examples.
      _(Added a new "Path variables" section: template combination, implicit vs `[FromRoute(Name=
      ...)]` binding, literal-over-placeholder precedence, `StringValueConverter` reuse/
      `InvalidParameterException`, eager `InvalidRouteException` validation, `[FromQuery(Name=...)]`
      symmetry, and the explicitly out-of-scope constraints/wildcards. Also updated the "Attribute
      routing"/"Parameter binding" feature table rows to mention path variables/`[FromRoute]`.)_
- [x] Update `.github/copilot-instructions.md` architecture section if the request flow changed
      materially (route template parsing/matching step).
      _(Updated the "What this is" summary and the "Architecture" request-flow list: `Build()`
      now also mentions eager route validation, `RouteHandlerInfo` now describes the combined
      `Template`, `GetRank`'s segment-aware matching is described, and step 5 now covers route
      parameter binding. Added a note that `UserDefinedApiToolkit.Build` intentionally duplicates
      route-combination/placeholder-parsing logic rather than referencing the main assembly.)_
- [x] Full test suite green; run targeted `dotnet test --filter` for touched areas first, then the
      full suite before calling the feature done.
      **Verified:** 0 failed / 92 passed (`ApiChanges` excluded, per the user's instruction to run
      it manually at the end of the feature).

## Feature status: implementation complete

All 7 phases (0–6) plus docs (Phase 7) are done; full suite is green (92/92, `ApiChanges`
excluded). A full public-API XML documentation pass was also completed (not limited to the
path-variables feature): all public types/members across attributes, exceptions, results,
converters, `ApiContext`, `ControllerBase`, `UserDefinedApiBuilder`, and `DI/` were reviewed and
documented with `<summary>`/`<param>`/`<returns>`/`<exception>`/`<see cref>` comments. Verified
with a Release build (0 `CS1574` broken-cref warnings) and a full test re-run (92/92 passed).

`pathvar-refactor-review` is now also done (14/14): `RouteHandlerInfo.GetRank`/`Invoke`'s parameter
classification was deduplicated into a shared `ClassifyParameter` helper (`ParameterBindingSource`
enum + `ParameterBinding` struct), used by both methods so scoring and binding can never disagree.
This also fixed two real bugs found during the review:
- `GetRank` had no notion of framework-provided (`ApiContext`/`IEngine`/`IConnection`/
  `IServiceProvider`) or DI-resolvable parameters, so a route with such an unattributed parameter
  could never be selected (incorrectly ranked `-1`/excluded). `GetRank` now takes an
  `IServiceProvider` (threaded through `RouteSelector.SelectRoute`/`UserDefinedApi.Run` via the
  per-request DI scope) and classifies these consistently with `Invoke`.
- `Invoke` had a copy-paste bug: two consecutive `typeof(IEngine)` checks meant an unattributed
  `IConnection` method parameter was never framework-bound (dead code) and fell through to a
  failing query lookup. Fixed to resolve `IConnection` via `IAccessor<IConnection>`.
Regression tests added in `UserDefinedApiToolkit.Tests/Runtime/FrameworkDependency/` cover both
fixes. Full suite: 94/94 passed (`ApiChanges` excluded).

Remaining, not blocking:
- The repo owner still needs to run `ApiChanges`/`PublicChanges` manually and accept the updated
  public-API snapshot (new attributes/exception/public surface, plus new XML doc comments) before
  merging.

## Deferred / noted for later (not in this iteration)
- Route constraints (`{id:int}` syntax).
- Catch-all/wildcard segments (`{*path}`).
