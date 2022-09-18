// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class WebAppDirectorySettings : WebAppManagerSettingsBase
    {
        private string _webAppDirectory;

        public string WebAppDirectory
        {
            get
            {
                return _webAppDirectory;
            }
            set
            {
                _webAppDirectory = value;
                OnPropertyChange("WebAppDirectory");
            }
        }
    }
}
