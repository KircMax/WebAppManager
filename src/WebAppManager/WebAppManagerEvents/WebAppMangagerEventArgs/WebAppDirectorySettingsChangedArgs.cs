// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs
{
    public class WebAppDirectorySettingsChangedArgs : EventArgs
    {
        public string newDirectory { get; set; }
    }
}
