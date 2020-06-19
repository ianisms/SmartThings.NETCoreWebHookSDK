#region Copyright
// <copyright file="MyConfigWebhookHandler.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of MyWebhookLib
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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