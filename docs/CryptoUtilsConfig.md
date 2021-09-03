

# CryptoUtilsConfig

```CryptoUtilsConfig``` is used to configure the ```ICryptoUtils``` implementation that is used to verify the signature on the incoming requests using SmartThings x.509 cert as per the [HTTP signature verification spec](https://developer-preview.smartthings.com/docs/connected-services/hosting/webhook-smartapp#authorizing-calls-from-smartthings). The ```CryptoUtilsConfig``` can be configured to override the base url for the SmartThings cert, ```SmartThingsCertUriRoot```.  If no change is desired, you can leave the configuration out of your app settings.

## Example Config

```
  "CryptoUtilsConfig": {
    "SmartThingsCertUriRoot": "https://key.smartthings.com"
  },
```
