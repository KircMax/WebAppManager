// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;

namespace Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs
{
    public class WebAppDirectorySettingsChangedArgs : EventArgs
    {
        public string newDirectory { get; set; }
    }
}
