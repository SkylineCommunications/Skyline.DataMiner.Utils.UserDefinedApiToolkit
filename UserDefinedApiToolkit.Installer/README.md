# Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer

Helper nuget package to more easily install User Defined Api's created with the toolkit.

## Central package project integration

The installer package must be referenced by a DataMiner package project:

```xml
<PropertyGroup>
	<DataMinerType>Package</DataMinerType>
</PropertyGroup>
```

Add the installer package to the central DataMiner package project and list the UDAPI projects
that should be built and included:

```xml
<ItemGroup>
	<UdapiProject Include="..\Orders API\Orders API.csproj" />
	<UdapiProject Include="..\Users API\Users API.csproj" />
</ItemGroup>
```

The installer runs `dotnet build` for each listed project, using the package build's configuration
and platform. Each child build restores its own dependencies. The installer then generates the
`<ProjectName>.udapi.json` files and copies them to `SetupContent\UDAPI` before package creation.
A failed UDAPI project stops package generation.

`UdapiProject` supports the standard MSBuild wildcard and exclusion syntax. For example, to
include every project below an API directory while excluding a nested installer project:

```xml
<ItemGroup>
	<UdapiProject Include="..\Apis\**\*.csproj"
				 Exclude="..\Apis\UdapiInstaller\**\*" />
</ItemGroup>
```

Every matched project is built and must be a UDAPI project with the corresponding script XML file.
Keep the central package project outside the wildcard scope, or exclude it, to avoid including the
package project in its own build.

The installer checks for matching script XML files before building and verifies that the projects
import the toolkit targets after the child builds. Use a NuGet configuration visible to the child
projects; other command-line MSBuild overrides on the package build aren't forwarded to `dotnet build`.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).
