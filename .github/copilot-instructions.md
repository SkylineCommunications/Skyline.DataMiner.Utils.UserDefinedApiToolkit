# Copilot Instructions

## What this is

A DataMiner SDK (`Skyline.DataMiner.Utils.UserDefinedApiToolkit`) that lets DataMiner Automation
scripts implement REST APIs using an ASP.NET-Core-like, attribute/controller-based model
(`[ApiController]`, `[Route]`, `[HttpGet]`/`[HttpPost]`/`[HttpPut]`/`[HttpDelete]` with optional
`{placeholder}` path variable templates, `[FromRoute]`, `[FromBody]`, `[FromQuery]`). See
`README.md` for the end-user API and usage examples — read it before changing public-facing
behavior (attributes, builder methods, return helpers).

## Build / test

- Target framework: `net48`. Solution file: `Skyline.DataMiner.Utils.UserDefinedApiToolkit.slnx`.
- Build: `dotnet build .\Skyline.DataMiner.Utils.UserDefinedApiToolkit.slnx -c Release`
- Run all tests: `dotnet test .\UserDefinedApiToolkit.Tests\UserDefinedApiToolkit.Tests.csproj`
- Run a single test/class: add `--filter "FullyQualifiedName~ControllerTests"` (or a specific
  test name) to the `dotnet test` command above.
- Never run the `ApiChanges`/`PublicChanges` test (`UserDefinedApiToolkit.Tests/API/ApiChanges.cs`)
  during feature development — it's a public-API-surface snapshot test the repo owner runs and
  accepts manually at the end of a feature. Exclude it, e.g.
  `--filter "FullyQualifiedName!~ApiChanges"`.
- Test project uses MSTest + FluentAssertions, targets `PlatformTarget x86` (the main project
  builds `AnyCPU`, so an `MSB3270` architecture-mismatch warning during build/test is expected
  and not a regression).
- StyleCop analyzers and `Skyline.DataMiner.Utils.SecureCoding.Analyzers` run as part of the build
  (via `Directory.Build.props`); expect/allow existing `SA*`/`SLC_SC*` warnings but don't introduce
  new ones in code you touch.
- CI (GitHub Actions `Internal.yml`/`Public.yml`) delegates to the shared
  `SkylineCommunications/_ReusableWorkflows` master workflow — there are no repo-local lint/test
  scripts beyond the `dotnet` commands above.

## Architecture

Request flow (`UserDefinedApi.Run`, called from a DataMiner `OnApiTrigger` entry point):
1. `UserDefinedApiBuilder` (via `UserDefinedApi.CreateBuilder()`) collects controllers
   (`AddController<T>`/`AddControllers()`), DI registrations (`ConfigureServices`,
   `AddRepository<T>`), and input/output converters, then `Build()` validates that every action
   parameter type has a matching `IInputConverter`, validates that every route template
   `{placeholder}` has a matching bound parameter and vice versa (throwing `InvalidRouteException`
   on mismatch — see `Routes/RouteTemplate.cs`), and builds the DI container
   (`Microsoft.Extensions.DependencyInjection`).
2. Each `[ApiController]` method decorated with an `Http*Attribute` becomes a `RouteHandlerInfo`
   (controller type, HTTP method, combined route `Template` — controller `[Route]` + method
   `Http*` template, via `RouteTemplate.Combine` — `MethodInfo`, parameters).
3. On each `Run(engine, apiTriggerInput)` call, a new DI scope is created, `IAccessor<IEngine>`
   and `IAccessor<IConnection>` are populated for that scope (see `DI/IAccessor.cs`), a fresh
   `ApiContext` (request/response/converters) is built, `Routes/RouteSelector.cs` picks the
   matching `RouteHandlerInfo` (via `RouteHandlerInfo.GetRank`, which does segment-aware matching
   of the request path against the route template — literal segments must match exactly,
   `{placeholder}` segments match any value and outrank ties against literal-only alternatives),
   the controller instance is resolved from DI, and the action is invoked.
4. Actions return an `IApiResult` (`Results/ObjectResult.cs`, `StatusCodeResult.cs`, helpers in
   `Controllers/ControllerBase.ReturnHelpers.cs` like `Ok()`/`NotFound()`/`StatusCode()`).
   `result.ExecuteResult(apiContext)` writes the final `ApiTriggerOutput`.
5. Body/query/route parameter binding and result serialization go through `Converters/` — default
   is `NewtonsoftConverter` (JSON via Newtonsoft.Json); `StringConverter` handles plain strings.
   Route parameters bind implicitly (parameter name matches a `{placeholder}`) or explicitly via
   `[FromRoute(Name = ...)]`; both convert through `StringValueConverter`, same as `[FromQuery]`.
   New parameter/return types require a matching `IInputConverter`/`IOutputConverter`, or `Build()`
   throws `InvalidOperationException`.
6. `ControllerBase` exposes `Request`/`ApiTriggerOutput`/`DefaultInputConverter`/
   `DefaultOutputConverter` sourced from its injected `ApiContext`.

`UserDefinedApiToolkit.Build` is a separate MSBuild-task project (`OpenApiTask`, `OpenApi/*`) that
reflects over a built controller assembly to generate an OpenAPI 3.0 spec (yaml/json) when a
consuming project sets `<GenerateOpenApi>True</GenerateOpenApi>` — keep its route/type-inspection
logic (`TypeHelper.cs`, `OpenApi/ComponentRegistry.cs`, `OpenApi/PathBuilder.cs`) in sync with any
new attributes or converters added to the main library. Note this project has **no project
reference** to the main library — it reads controller/method templates and attributes purely via
`CustomAttributeData` reflection (works against a `MetadataLoadContext`-loaded assembly), so route
combination/placeholder-parsing logic (`ControllerUnit.GetRoute(MethodInfo)`,
`TypeHelper.GetRoutePlaceholders`) is intentionally duplicated rather than shared with
`Routes/RouteTemplate.cs` in the main project.

## Conventions

- Namespaces/tabs: 1 file = 1 top-level type, tab-indented, `namespace X { ... }` block style
  (not file-scoped namespaces), `using` statements grouped and inside the namespace block.
- Public builder/attribute APIs throw `ArgumentNullException` for null args and custom exceptions
  (`Exceptions/`, e.g. `InvalidControllerException`) for invalid configuration — follow this
  pattern rather than returning null/silently failing.
- Controller validation (must inherit `ControllerBase`, must have a non-empty `[Route]`) happens
  in `UserDefinedApiBuilder.AddController` — `UserDefinedApiToolkit.Tests/Runtime/ControllerTests.cs`
  drives this via reflection-loaded test fixture controllers under
  `Runtime/GET/TestFiles/*` (named `Empty_Controller_Missing_*`); add new fixtures there when
  extending validation rules.
- `Nullable` is enabled in the main and Build projects but disabled behavior varies per test file;
  match the existing `Nullable`/`ImplicitUsings` setting of the project you're editing.
