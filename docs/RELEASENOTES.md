# Release Notes

## General

- Support for .NET Core 3.0 is underway.  Initial tests with my own smart app went great.
   - Started a [branch for .NET Core 3.0](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/3.0).  I'll be putting most of my attention here.
     - As an FYI, support for 3.0 in Azure Functions is in beta.  [More Info](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm)
     - Once support for Azure Functions is fully released, this will be updated and become master.

## Releases

### 3.0.3.0 and 2.2.9.0 - 29 October 2019

- Strange packaging issue

### 3.0.2.0 and 2.2.8.0 - 29 October 2019

- Fixed an issue with ```PresenceSensorFromDynamic, presenceSensorNamePattern```.  If used, it now must be passed as a string (was not working at all before).  The value can be set / gathered however you see fit.  For example, it could be set in the config and gathered via your ```InstallUpdateHandler```.

### 3.0.1.0 - 25 October 2019

- Azure Functions working with 3.0.  Please look at [this post on developing Azure Functions with NET Core 3.0](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm)

### 2.2.7.0 - 25 October 2019

- Fix for DI issue in Azure Functions Apps.

### 2.2.6.0 - 25 October 2019

- A bit of code cleanup.
- Reved to beta.
- Fixed some package issues.
- Added symbols package.

### 2.2.2.0 - 24 October 2019

- Lots of bug fixes around token management and hosting in AZ Functions.  This forced a split in how we add token management to AZ Functions vs ASP.NET Core.  [Full details](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#installed-app-token-management-utils).
- Breaking change to avoid a naming conflict.  The ```InstalledApp``` model has been renamed to ```InstalledAppInstance```.  It was either that or rename the InstalledApp namespace.  I took the path of least resistance.
- Tons of code cleanliness and CA supressions for things like exception text globalization (perhaps we will add a resource manager for the strings later).
- Updated the samples to use the nuget package and they are fully working now.
- Created separate solutions for the SDK and the samples.

### 2.2.1.0 - 21 October 2019

- Massive updates after working on a SmartApp using the SDK.  README and samples completely updated.
- Huge perf improvements using mostly fire-and-forget async calls to ensure the responses get back to Samsung ASAP.