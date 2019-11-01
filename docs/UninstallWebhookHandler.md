
# UninstallWebhookHandler

During the [UNINSTALL Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#UNINSTALL), the ```UninstallWebhookHandler``` you add via DI will react to the handle the ```uninstallData``` sent in the request.  This ```uninstallData``` contains the ```installedApp.config``` you should use to remove state, etc.  Your ```UninstallWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.UninstallWebhookHandler``` and override the abstract methods ```void HandleUninstallData(dynamic uninstallData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

## Example

```csharp
public class MyUninstallWebhookHandler : UninstallWebhookHandler
{
    private readonly IStateManager<MyState> stateManager;

    public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger,
        IInstalledAppManager installedAppManager,
        IStateManager<MyState> stateManager)
        : base(logger, installedAppManager)
    {
        _ = stateManager ??
            throw new ArgumentNullException(nameof(stateManager));
    }

    public override void ValidateRequest(dynamic request)
    {
        base.ValidateRequest((JObject)request);

        _ = request.uninstallData.installedApp.config.isAppEnabled ??
            throw new InvalidOperationException("request.uninstallData.installedApp.config.isAppEnabled is null");
        _ = request.uninstallData.installedApp.config.presenceSensors ??
            throw new InvalidOperationException("request.uninstallData.installedApp.config.presenceSensors is null");
    }

    public override async Task HandleUninstallDataAsync(dynamic uninstallData)
    {
        var installedAppId = uninstallData.installedApp.InstalledAppId;
        await stateManager.RemoveStateAsync(installedAppId);
    }
}
```
