#region Copyright
// <copyright file="MyConfigWebhookHandler.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
#endregion

using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace MyWebhookLib.WebhookHandlers
{
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
}