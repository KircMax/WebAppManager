// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class WebAppConfigCreatorSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private WebAppDirectorySettings _webAppDirectorySetting;
        private WebAppSelectionSettings _webAppSelectionSetting;
        private WebAppConfigurationSettings _webAppConfigurationSetting;        


        public WebAppDirectorySettings WebAppDirectorySettings
        {
            get
            {
                return _webAppDirectorySetting;
            }
            set
            {
                _webAppDirectorySetting = value;
                OnPropertyChange("WebAppDirectorySetting");
            }
        }

        public WebAppSelectionSettings WebAppSelectionSettings
        {
            get
            {
                return _webAppSelectionSetting;
            }
            set
            {
                _webAppSelectionSetting = value;
                OnPropertyChange("WebAppSelectionSetting");
            }
        }

        public WebAppConfigurationSettings WebAppConfigurationSettings
        {
            get
            {
                return _webAppConfigurationSetting;
            }
            set
            {
                _webAppConfigurationSetting = value;
                OnPropertyChange("WebAppConfigurationSettings");
            }
        }

        

        public WebAppConfigCreatorSettings()
        {
            WebAppDirectorySettings = new WebAppDirectorySettings()
            {
            };
            WebAppSelectionSettings = new WebAppSelectionSettings()
            {
            };
            WebAppConfigurationSettings = new WebAppConfigurationSettings();
        }

        protected void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
