# Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build

## About

This project provides the `OpenApiTask` MSBuild task used by
[Skyline.DataMiner.Utils.UserDefinedApiToolkit](https://www.nuget.org/packages/Skyline.DataMiner.Utils.UserDefinedApiToolkit)
to generate an OpenAPI 3.0 specification from a compiled controller assembly.

It reflects over the built assembly (via `MetadataLoadContext`) to discover `[ApiController]` classes
and their `[HttpGet]`/`[HttpPost]`/`[HttpPut]`/`[HttpPatch]`/`[HttpDelete]` actions, and produces an `openapi.yaml`
(or `.json`) document describing routes, parameters, and response schemas.

> **Note**
> This project is **not** published as a standalone NuGet package (`GeneratePackageOnBuild` is
> disabled). Its build output is bundled into the `tasks/` folder of the main
> `Skyline.DataMiner.Utils.UserDefinedApiToolkit` package and wired up automatically via that
> package's `build\*.targets` file. Consumers only need to reference the main package and set
> `<GenerateOpenApi>True</GenerateOpenApi>` in their project — see its README for details.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

<!-- Uncomment below and add more info to provide more information about how to use this package. -->
<!-- ## Getting Started -->
