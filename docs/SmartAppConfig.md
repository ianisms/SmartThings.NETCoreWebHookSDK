# SmartAppConfig

The ```SmartAppConfig``` should be configured with the ```SmartAppClientId``` and ```SmartAppClientSecret``` given to you in the webhook registration on the developer portal and ```PAT`` with your personal access token from the developer portal (used in confirmation phase).  This is used to, among other things, refresh the tokens for your app.

## Example Config

```
"SmartAppConfig": {
    "SmartAppClientId": "<YOURCLIENTID>",
    "SmartAppClientSecret": "<YOURCLIENTSECRET>",
    "PAT": "<YOURPAT>"
},
```
