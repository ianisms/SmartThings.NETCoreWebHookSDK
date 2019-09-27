using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class ConfigWebhookHandler :
        ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler
    {
        public ConfigWebhookHandler(ILogger<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler> logger) : base(logger)
        {
        }

        public override dynamic Initialize(dynamic request)
        {
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

            return response;
        }

        public override dynamic Page(dynamic request)
        {
            dynamic response = JObject.Parse(@"{
                'configurationData': {
                    'page': {
                        'pageId': '1',
                        'name': 'Configure The Great Welcomer',
                        'nextPageId': null,
                        'previousPageId': null,
                        'complete': true,
                        'sections' : [
                            {
                                'name': 'basics',
                                'settings' : [
                                    {
                                        'id': 'appEnabled',
                                        'name': 'Enabled App?',
                                        'description': 'Easy toggle to enable/disable the app',
                                        'type': 'BOOLEAN',
                                        'required': true,
                                        'defaultValue': true,
                                        'multiple': false
                                    },                                    
                                    {
                                        'id': 'switches',
                                        'name': 'Light Switches',
                                        'description': 'The switches for the app to turn on/off',
                                        'type': 'DEVICE',
                                        'required': true,
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

            return response;
        }
    }
}