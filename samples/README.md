### Description

The samples allow for a basic webhook based automation that uses a single-page configuration to display a single section with a boolean toggle and a list of switches.  The samples also show how to subscribe to / handle events for the switches.

### File Structure

- [ASPNetCoreWebAPI](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples/ASPNetCoreWebAPI)
  - ASP.NET Core App with a single controller that adds the common webhook handlers in [Startup.cs](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/samples/ASPNetCoreWebAPI/Startup.cs)
- [AzureFunctionsApp](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples/AzureFunctionsApp)
  - Azure Functions app  with a single controller that adds the common webhook handlers in [FunctionsAppStartup.cs](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/samples/AzureFunctionsApp/FunctionsAppStartup.cs)
- [MyWebhookLib](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples/MyWebhookLib)
  - Common lib that has the common app logic for both apps.  Both apps use [AddMyWebhookService()](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/samples/MyWebhookLib/Extensions/ServiceCollectionExtensions.cs) to add all of the requisite webhook handlers and the common [MyService](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/samples/MyWebhookLib/Services/MyService.cs) to their service collections.
- [scripts](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples/scripts)
  - [startngrok.cmd](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/samples/scripts/startngrok.cmd) starts an ngrok tunnel to localhost on port 5000.
