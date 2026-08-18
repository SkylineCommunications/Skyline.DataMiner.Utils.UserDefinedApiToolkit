# Troubleshooting

## Toolkit configuration

- **`InvalidControllerException` at registration:** the type does not inherit from
  `ControllerBase` or has no valid `[Route]`.
- **`InvalidRouteException` at `Build()`:** a route placeholder and action parameter do not match,
  or a `[FromRoute]` name is not present in the route.
- **No converter found:** register an input/output converter for the relevant type, or replace
  the default converter.

## Request handling

- **`NoRouteException` at runtime:** the HTTP method or request path does not match any action, or
  a required query parameter is missing.
- **`InvalidParameterException` at runtime:** a route, query, or body value cannot be converted
  to the declared parameter type.

## Installer packaging

- **Toolkit target validation fails:** ensure each `UdapiProject` references
  `Skyline.DataMiner.Utils.UserDefinedApiToolkit`.
- **Script XML validation fails:** ensure the Automation script XML file has the same base name and
  is next to the project file.
- **Package type validation fails:** ensure the central installer project contains
  `<DataMinerType>Package</DataMinerType>`.
- **A package project is selected as a UDAPI project:** narrow the wildcard or exclude the central
  package and installer projects.

For build and installer configuration, see [build and packaging](build-and-packaging.md).
