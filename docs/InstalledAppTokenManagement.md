# Installed App Token Management

You must refresh the tokens for your app periodically.  There are two flavors of token manager available to you to refresh your refresh token periodically if it is about to expire.  Your access token should be refreshed by the SDK before every api call if it is about to expire.  More Info on [SmartApp tokens](https://smartthings.developer.samsung.com/docs/auth-and-permissions.html#Using-SmartApp-tokens).

## For ASP.NET Core

Use ```InstalledAppTokenManagerService``` by injecting it into to your service collection via ```services.AddInstalledAppTokenManagerService();```.  The ```InstalledAppTokenManagerService``` (hosted service) will refresh your refresh token every 29.5 minutes if it is about to expire.

## For Azure Functions

Use ```InstalledAppTokenManager``` by injecting it into your service collection via ```services.AddInstalledAppTokenManager();```.  The ```InstalledAppTokenManager``` should be used in a timer trigger to refresh the tokens on your desired schedule.  I reccomend a schedule expression of ```0 */29 * * * *``` (29 minutes).  An example:

```csharp
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureFunctionsApp
{
    public class FunctionsService
    {
        private readonly ILogger<FunctionsService> logger;
        private readonly IInstalledAppTokenManager installedAppTokenManager;

        public FunctionsService(ILogger<FunctionsService> logger,
            IInstalledAppTokenManager installedAppTokenManager)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = installedAppTokenManager ??
                throw new ArgumentNullException(nameof(installedAppTokenManager));

            this.logger = logger;
            this.installedAppTokenManager = installedAppTokenManager;
        }

        /**
         *  For functions apps, we cannot add a hosted service to handle token refresh so we simply
         *  spin up a timer trigger to periodically refresh the tokens.
         *  The schedule expression is for the current refreshToken timeout (30 minutes) with a bit of buffer.
         *  The accessToken will be refreshed before each api request.
         **/
        [FunctionName("InstalledAppTokenRefresh")]
        public async Task InstalledAppTokenRefresh(
            [TimerTrigger("0 */29 * * * *")] TimerInfo timer)
        {
            try
            {
                if (timer.IsPastDue)
                {
                    logger.LogDebug("Timer is running late!");
                }

                await installedAppTokenManager.RefreshAllTokensAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling installedAppTokenManager.RefreshAllTokensAsync");
                throw;
            }
        }
    }
}
```
