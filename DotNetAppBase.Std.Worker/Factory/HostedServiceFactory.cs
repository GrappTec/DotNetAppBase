﻿#region License

// Copyright(c) 2020 GrappTec
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using DotNetAppBase.Std.Library;
using DotNetAppBase.Std.Worker.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAppBase.Std.Worker.Factory
{
    public static class HostedServiceFactory
    {
        public static T Create<T>(IServiceProvider provider, string name = null, string settingsSectionName = null) where T : HostedServiceBase, new()
        {
            var service = new T();

            IConfigurationSection settingsSection = null;
            if (XHelper.Strings.IsNotEmptyOrWhiteSpace(settingsSectionName))
            {
                settingsSection = provider.GetService<IConfiguration>().GetSection(settingsSectionName);
            }

            service.Initialize(name ?? service.GetType().FullName, provider.GetService<ILoggerFactory>(), settingsSection);

            return service;
        }

        public static T Initialize<T>(IServiceProvider provider, T service, string name = null, string settingsSectionName = null) where T : HostedServiceBase
        {
            IConfigurationSection settingsSection = null;
            if (XHelper.Strings.IsNotEmptyOrWhiteSpace(settingsSectionName))
            {
                settingsSection = provider.GetService<IConfiguration>().GetSection(settingsSectionName);
            }

            service.Initialize(name ?? service.GetType().FullName, provider.GetService<ILoggerFactory>(), settingsSection);

            return service;
        }
    }
}