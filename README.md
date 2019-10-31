# SmartThings.NETCoreWebHookSDK

## Description
SDK for building SmartThings webhook automations using .NET Core 2.2 or .NET Core 3.0.

## Samples
There is a sample ASP NET Core and a sample Azure Functions app sharing a common library for the core service injected via DI.  The samples allow for a basic webhook based automation that uses a single-page configuration to display a single section with a boolean toggle and a list of switches.  The samples also show how to subscribe to / handle events for the switches.

For .NET Core 2.2, take a look at the [master branch](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples).

For .NET Core 3.0, take a look at the [3.0 branch](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/3.0/samples).

## Other Details
- [Release Notes](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/docs/RELEASENOTES.md)
- [Documentation](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/)
- [Issues / Feature Suggestions](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/issues)

### Latest NUGET

[2.2.9-beta](https://www.nuget.org/packages/SmartThings.NETCoreWebHookSDK/2.2.9-beta) for .NET Core 2.2 apps.

```batch
dotnet add package SmartThings.NETCoreWebHookSDK --version 2.2.9-beta
```

[3.0.3-beta](https://www.nuget.org/packages/SmartThings.NETCoreWebHookSDK/3.0.3-beta) for .NET Core 3.0 apps.

```batch
dotnet add package SmartThings.NETCoreWebHookSDK --version 3.0.3-beta
```
