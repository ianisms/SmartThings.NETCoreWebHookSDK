# ConfigWebhookHandler

During the [CONFIGURATION Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/configuration.html), the ```ConfigWebhookHandler``` you add via DI will send desired config screens to the UI to allow for app configuration.  Your ```ConfigWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler``` and override the two abstract methods ```dynamic Initialize(dynamic request)``` and ```dynamic Page(dynamic request)```.

## Initialize Response Example

Return a ```Newtonsoft.Json.Linq dynamic JObject```:

```csharp
dynamic response = JObject.Parse(@"{
    'configurationData': {
        'initialize': {
            'id': 'app',
            'name': 'The Great Welcomer',
            'permissions': ['r:devices:*'],
            'firstPageId': '1'
        }
    }
}");
```

## Single-Page

### Single-Page Response Example

Return a ```Newtonsoft.Json.Linq dynamic JObject``` respresenting your desired configuration screens:

```csharp
private static readonly dynamic pageOneResponse = JObject.Parse(@"
{
    'configurationData': {
        'page': {
            'pageId': '1',
            'name': 'Configure My App',
            'nextPageId': null,
            'previousPageId': null,
            'complete': true,
            'sections' : [
                {
                    'name': 'basics',
                    'settings' : [
                        {
                            'id': 'isAppEnabled',
                            'name': 'Enabled App?',
                            'description': 'Easy toggle to enable/disable the app',
                            'type': 'BOOLEAN',
                            'required': true,
                            'defaultValue': true,
                            'multiple': false
                        },
                        {
                            'id': 'switches',
                            'name': 'Which Light Switch(es)?',
                            'description': 'The switch(es) to turn on/off on arrival.',
                            'type': 'DEVICE',
                            'required': false,
                            'multiple': true,
                            'capabilities': ['switch'],
                            'permissions': ['r', 'x']
                        }
                    ]
                }
            ]
        }
    }
}");

### Single-Page Full Example

```csharp
public class MyConfigWebhookHandler : ConfigWebhookHandler
{
    public MyConfigWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        : base(logger)
    {
    }

    private static readonly dynamic initResponse = JObject.Parse(@"
    {
        'configurationData': {
            'initialize': {
                'id': 'app',
                'name': 'My App',
                'permissions': ['r:devices:*','r:locations:*'],
                'firstPageId': '1'
            }
        }
    }");

    private static readonly dynamic pageOneResponse = JObject.Parse(@"
    {
        'configurationData': {
            'page': {
                'pageId': '1',
                'name': 'Configure My App',
                'nextPageId': null,
                'previousPageId': null,
                'complete': true,
                'sections' : [
                    {
                        'name': 'basics',
                        'settings' : [
                            {
                                'id': 'isAppEnabled',
                                'name': 'Enabled App?',
                                'description': 'Easy toggle to enable/disable the app',
                                'type': 'BOOLEAN',
                                'required': true,
                                'defaultValue': true,
                                'multiple': false
                            },
                            {
                                'id': 'switches',
                                'name': 'Which Light Switch(es)?',
                                'description': 'The switch(es) to turn on/off on arrival.',
                                'type': 'DEVICE',
                                'required': false,
                                'multiple': true,
                                'capabilities': ['switch'],
                                'permissions': ['r', 'x']
                            }
                        ]
                    }
                ]
            }
        }
    }");

    public override dynamic Initialize(dynamic request)
    {
        return initResponse;
    }

    public override dynamic Page(dynamic request)
    {
        var pageId = request.configurationData.pageId.Value;

        return pageId switch
        {
            "1" => pageOneResponse,
            _ => throw new InvalidOperationException($"Unknown pageId: {request.configurationData.pageId.Value}"),
        };
    }
}
```

## Multi-Page

If you are using a multi-page confguration, you should look at the ```pageId``` property in thhe ```dynamic request``` parameter and return the request page configuration being sure to set ```'nextPageId'``` and ```'previousPageId'``` accordingly.  Set ```'complete'``` to ```true``` on the final page and ```false``` on all other pages.

### Multi-Page Full Example

```csharp
private static readonly dynamic pageOneResponse = JObject.Parse(@"
{
    'configurationData': {
        'page': {
            'pageId': '1',
            'name': 'Basics',
            'nextPageId': '2',
            'previousPageId': null,
            'complete': false,
            'sections' : [
                {
                    'name': 'Basics',
                    'settings' : [
                        {
                            'id': 'isAppEnabled',
                            'name': 'Enabled App?',
                            'description': 'Easy toggle to enable/disable the app',
                            'type': 'BOOLEAN',
                            'required': true,
                            'defaultValue': true,
                            'multiple': false
                        },
                        {
                            'id': 'locks',
                            'name': 'Which Lock(s)?',
                            'description': 'The lock(s) that will be unlocked after arrival.',
                            'type': 'DEVICE',
                            'required': false,
                            'multiple': true,
                            'capabilities': ['lock'],
                            'permissions': ['r', 'x']
                        },
                    ]
                }
            ]
        }
    }
}");

private static readonly dynamic pageTwoResponse = JObject.Parse(@"
{
    'configurationData': {
        'page': {
            'pageId': '2',
            'name': 'Motion and Switches',
            'nextPageId': '3',
            'previousPageId': '1',
            'complete': false,
            'sections' : [
                {
                    'name': 'Motion',
                    'settings' : [
                        {
                            'id': 'motionSensors',
                            'name': 'Which Motion Sensor(s)?',
                            'description': 'The motion sensor(s) that will trigger the app on presence + motion.  If empty, the presence sesnor alone will trigger the app.',
                            'type': 'DEVICE',
                            'required': false,
                            'multiple': true,
                            'capabilities': ['motionSensor'],
                            'permissions': ['r']
                        }
                    ]
                },
                {
                    'name': 'Switches',
                    'settings' : [
                        {
                            'id': 'switches',
                            'name': 'Which Light Switch(es)?',
                            'description': 'The switch(es) to turn on/off on arrival.',
                            'type': 'DEVICE',
                            'required': false,
                            'multiple': true,
                            'capabilities': ['switch'],
                            'permissions': ['r', 'x']
                        }
                    ],
                }
            ]
        }
    }
}");

public override dynamic Page(dynamic request)
{
    var pageId = request.configurationData.pageId.Value;

    return pageId switch
    {
        "1" => pageOneResponse,
        "2" => pageTwoResponse,
        _ => throw new InvalidOperationException($"Unknown pageId: {request.configurationData.pageId.Value}"),
    };
}
```
