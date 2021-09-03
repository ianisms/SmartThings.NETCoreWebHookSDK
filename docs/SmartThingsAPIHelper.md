# ISmartThingsAPIHelper

```ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings.ISmartThingsAPIHelper``` is a helper utility for calling specific methods in the [SmartThings API](https://developer-preview.smartthings.com/api/public).

## Usage

```ISmartThingsAPIHelper``` is injected into your service collection via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method using ```.AddWebhookHandlers()```.

## Supported Methods

The following helper methods are exposed as wrappers around the [SmartThings API](https://developer-preview.smartthings.com/api/public):

### RefreshTokensAsync

```csharp
Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp);
```

Given ```InstalledAppInstance installedApp```, refreshes the app tokens.  Called internally to keep tokens alive.

### SubscribeToDeviceEventAsync

```csharp
Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
    dynamic device);
```

Given ```InstalledAppInstance installedApp``` and ```dynamic device```, will add an event subscription for the given device.  Called internally during app installation to subscribe to events for the configured devices.

### ClearSubscriptionsAsync

```csharp
Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledAppInstance installedApp);
```

Given ```InstalledAppInstance installedApp```, removes all device event subscriptions.  Called internally during uninstallation.

### GetDeviceDetailsAsync

```csharp
Task<dynamic> GetDeviceDetailsAsync(InstalledAppInstance installedApp,
    string deviceId);
```

Given ```InstalledAppInstance installedApp``` and ```string deviceId```, will get the details for a desired device. Used internally during installation.

### GetDeviceStatusAsync

```csharp
Task<dynamic> GetDeviceStatusAsync(InstalledAppInstance installedApp,
    string deviceId);
```

Given ```InstalledAppInstance installedApp``` and ```string deviceId```, will get the current status for a desired device.

### DeviceCommandAsync

```csharp
Task DeviceCommandAsync(InstalledAppInstance installedApp,
    string deviceId,
    dynamic command);
```

Given ```InstalledAppInstance installedApp```, ```string deviceId```, and ```dynamic command```, will execute the given command on the desired device.

### GetLocationAsync

```csharp
Task<Location> GetLocationAsync(InstalledAppInstance installedApp,
    string locationId);
```

Given ```InstalledAppInstance installedApp``` and ```string locationId```, will get the desired location. Used internally for command execution, etc.

### SendNotificationAsync

```csharp
Task SendNotificationAsync(InstalledAppInstance installedApp,
    string msg,
    string title);
```

Given ```InstalledAppInstance installedApp```, ```string msg```, and optional ```string title```, will send a notification via the SmartThings app.

>**NOTE**: ```SendNotificationAsync``` uses an undocumented API and therefore might not function as expected.
