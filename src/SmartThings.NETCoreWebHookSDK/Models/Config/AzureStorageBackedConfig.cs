#region Copyright
// <copyright file="AzureStorageBackedConfig.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
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
using FluentValidation;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.Config
{
    public class AzureStorageBackedConfig<T>
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string CacheBlobName { get; set; }
    }

    public class AzureStorageBackedConfigValidator<T> : AbstractValidator<AzureStorageBackedConfig<T>> where T : class
    {
        public AzureStorageBackedConfigValidator()
        {
            RuleFor(context => context.ConnectionString).Must(val => !string.IsNullOrEmpty(val))
                .WithMessage($"ConnectionString must not be null or empty");
            RuleFor(context => context.ContainerName).Must(val => !string.IsNullOrEmpty(val))
                .WithMessage("ContainerName must not be null or empty");
            RuleFor(context => context.CacheBlobName).Must(val => !string.IsNullOrEmpty(val))
                .WithMessage("CacheBlobName must not be null or empty");
        }
    }

    public class AzureStorageBackedConfigWithClientValidator<T> : AbstractValidator<AzureStorageBackedConfig<T>> where T : class
    {
        public AzureStorageBackedConfigWithClientValidator()
        {
            RuleFor(context => context.ContainerName).Must(val => !string.IsNullOrEmpty(val))
                .WithMessage("ContainerName must not be null or empty");
            RuleFor(context => context.CacheBlobName).Must(val => !string.IsNullOrEmpty(val))
                .WithMessage("CacheBlobName must not be null or empty");
        }
    }
}