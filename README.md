# SmartThings.NETCoreWebHookSDK

## Description

Currently just a first pass with 2 samples, one for ASP NET Core Web API and one for an Azure Functions HttpTrigger.  Both samples allow for a very basic WebHook based SmartApp that uses a single-page configuration to display a single section with a boolean toggle and a list of switches.  The samples also show how to subscribe to / handle events for the switches.

I will expand on the docs/README and tune the functionality once I port my favorite groovy based SmartApp using this SDK.  Of course I am also happy to take on contributors.

- [Release Notes](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/docs/RELEASENOTES.md)
- [Documentation](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/)
- [Issues / Feature Suggestions](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/issues)

### Latest NUGET

[2.2.8-beta](https://www.nuget.org/packages/SmartThings.NETCoreWebHookSDK/2.2.8-beta) for .NET Core 2.2 apps.

```batch
dotnet add package SmartThings.NETCoreWebHookSDK --version 2.2.8-beta
```

[3.0.2-beta](https://www.nuget.org/packages/SmartThings.NETCoreWebHookSDK/3.0.2-beta) for .NET Core 3.0 apps.

```batch
dotnet add package SmartThings.NETCoreWebHookSDK --version 3.0.2-beta
```
