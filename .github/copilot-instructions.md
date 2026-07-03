# Copilot Instructions

## What this is

A DataMiner SDK (`Skyline.DataMiner.Utils.UserDefinedApiToolkit`) that lets DataMiner Automation
scripts implement REST APIs using an ASP.NET-Core-like, attribute/controller-based model
(`[ApiController]`, `[Route]`, `[HttpGet]`/`[HttpPost]`/`[HttpPut]`/`[HttpDelete]`, `[FromBody]`,
`[FromQuery]`). See `README.md` for the end-user API and usage examples — read it before changing
public-facing behavior (attributes, builder methods, return helpers).

## Build / test

- Target framework: `net48`. Solution file: `Skyline.DataMiner.Utils.UserDefinedApiToolkit.slnx`.
- Build: `dotnet build .\Skyline.DataMiner.Utils.UserDefinedApiToolkit.slnx -c Release`
- Run all tests: `dotnet test .\UserDefinedApiToolkit.Tests\UserDefinedApiToolkit.Tests.csproj`
- Run a single test/class: add `--filter "FullyQualifiedName~ControllerTests"` (or a specific
  test name) to the `dotnet test` command above.
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
   parameter type has a matching `IInputConverter` and builds the DI container
   (`Microsoft.Extensions.DependencyInjection`).
2. Each `[ApiController]` method decorated with an `Http*Attribute` becomes a `RouteHandlerInfo`
   (controller type, HTTP method, route template, `MethodInfo`, parameters).
3. On each `Run(engine, apiTriggerInput)` call, a new DI scope is created, `IAccessor<IEngine>`
   and `IAccessor<IConnection>` are populated for that scope (see `DI/IAccessor.cs`), a fresh
   `ApiContext` (request/response/converters) is built, `Routes/RouteSelector.cs` picks the
   matching `RouteHandlerInfo`, the controller instance is resolved from DI, and the action is
   invoked.
4. Actions return an `IApiResult` (`Results/ObjectResult.cs`, `StatusCodeResult.cs`, helpers in
   `Controllers/ControllerBase.ReturnHelpers.cs` like `Ok()`/`NotFound()`/`StatusCode()`).
   `result.ExecuteResult(apiContext)` writes the final `ApiTriggerOutput`.
5. Body/query parameter binding and result serialization go through `Converters/` — default is
   `NewtonsoftConverter` (JSON via Newtonsoft.Json); `StringConverter` handles plain strings.
   New parameter/return types require a matching `IInputConverter`/`IOutputConverter`, or `Build()`
   throws `InvalidOperationException`.
6. `ControllerBase` exposes `Request`/`ApiTriggerOutput`/`DefaultInputConverter`/
   `DefaultOutputConverter` sourced from its injected `ApiContext`.

`UserDefinedApiToolkit.Build` is a separate MSBuild-task project (`OpenApiTask`, `OpenApi/*`) that
reflects over a built controller assembly to generate an OpenAPI 3.0 spec (yaml/json) when a
consuming project sets `<GenerateOpenApi>True</GenerateOpenApi>` — keep its route/type-inspection
logic (`TypeHelper.cs`, `OpenApi/ComponentRegistry.cs`, `OpenApi/PathBuilder.cs`) in sync with any
new attributes or converters added to the main library.

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
