// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
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
