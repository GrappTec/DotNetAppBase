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
using DotNetAppBase.Std.Library.ComponentModel.Model.Interact.Contracts;
using DotNetAppBase.Std.Library.ComponentModel.Model.Interact.Enums;
using DotNetAppBase.Std.Library.ComponentModel.Model.Present.Attributes.Base;
using DotNetAppBase.Std.Library.ComponentModel.Model.Theme.Enums;

namespace DotNetAppBase.Std.Library.ComponentModel.Model.Present.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class ActionDisplayAttribute : BaseDisplayAttribute, IPresentDisplay, IPresentImageConfig, IInteractionShortcutConfig
    {
        private object _inherit;

        public object Inherit
        {
            get => _inherit;
            set
            {
                if (Equals(_inherit, value))
                {
                    return;
                }

                _inherit = value;

                var fieldInfo = XHelper.Enums.GetFieldInfo(_inherit);

                var dispayConfig = XHelper.Models.GetDisplayData(fieldInfo);
                if (dispayConfig != null)
                {
                    Name ??= dispayConfig.Value.Name;
                    Description ??= dispayConfig.Value.Description;
                }

                var shortcut = XHelper.Reflections.Attributes.Get<IInteractionShortcutConfig>(fieldInfo);
                if (shortcut != null)
                {
                    Alt = shortcut.Alt;
                    Control = shortcut.Control;
                    Shift = shortcut.Shift;
                    Key = shortcut.Key;
                }

                var image = XHelper.Reflections.Attributes.Get<IPresentImageConfig>(fieldInfo);
                if (image != null)
                {
                    Image = image.Image;
                    ImagePath ??= image.ImagePath;
                }
            }
        }

        public bool Alt { get; set; }

        public bool Control { get; set; }

        public EKey Key { get; set; }

        public bool Shift { get; set; }

        string IPresentDisplay.Description => GetDescription();

        string IPresentDisplay.GroupName => GetGroupName();

        string IPresentDisplay.Name => GetName();

        public EActionImage Image { get; set; } = EActionImage.None;

        public string ImagePath { get; set; }
    }
}