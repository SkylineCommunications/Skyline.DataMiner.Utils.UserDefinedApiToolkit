# Skyline.DataMiner.Utils.UserDefinedApiToolkit.Installer

## About

Helper nuget package to more easily install User Defined Api's created with the toolkit.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## Central package project integration

Add the installer package to the central DataMiner package project and list the UDAPI projects
that should be built and included:

```xml
<ItemGroup>
	<UdapiProject Include="..\Orders API\Orders API.csproj" />
	<UdapiProject Include="..\Users API\Users API.csproj" />
</ItemGroup>
```

The package builds the listed projects before the package project, generates their
`<ProjectName>.udapi.json` files, and copies them to `SetupContent\UDAPI`. Independent UDAPI
projects are built in parallel. A failed UDAPI project stops package generation.

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

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

<!-- Uncomment below and add more info to provide more information about how to use this package. -->
<!-- ## Getting Started -->
