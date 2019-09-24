using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;

namespace ASPNetCoreWebAPI.WebhookHandlers
{
    public class ConfigWebhookHandler : 
        ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler
    {
        public ConfigWebhookHandler(ILogger<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler> logger) : base(logger)
        {
        }

        public override ConfigResponse Initialize()
        {
            var response = new ConfigInitResponse()
            {
                ConfigData = new ConfigInitResponseConfigData()
                {
                    InitData = new ConfigInitResponseData
                    {
                        Name = "My App Name",
                        Id = "app",
                        Permissions = new string[]
                        {
                            "r:devices:*"
                        },
                        FirstPageId = "1"
                    }
                }
            };

            return response;
        }

        public override ConfigResponse Page()
        {
            var response = new ConfigPageResponse()
            {
                ConfigData = new ConfigPageResponseConfigData()
                {
                    Page = new ConfigPage()
                    {
                        PageId = "1",
                        Name = "Configure My App",
                        IsComplete = true,
                        Sections = new ConfigSection[]
                        {
                            new ConfigSection()
                            {
                                Name = "Basics",
                                Settings = new ConfigSetting[]
                                {
                                    new ConfigSetting()
                                    {
                                        Id = "AppEnabled",
                                        Name = "Enable App?",
                                        Type = ConfigSetting.SettingsType.Boolean,
                                        IsRequired = true,
                                        IsMultiple = false,
                                        DefaultValue = "true"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return response;
        }
    }
}
