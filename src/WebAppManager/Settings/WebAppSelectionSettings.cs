// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.Collections.ObjectModel;

namespace Webserver.Api.Gui.Settings
{
    public class WebAppSelectionSettings : WebAppManagerSettingsBase
    {
        private string _selectedWebApp;

        private string _webAppName;

        public string SelectedWebApp
        {
            get
            {
                return _selectedWebApp;
            }
            set
            {
                _selectedWebApp = value;
                OnPropertyChange("SelectedWebApp");
            }
        }
        public string WebAppName
        {
            get
            {
                return _webAppName;
            }
            set
            {
                _webAppName = value;
                OnPropertyChange("WebAppName");
            }
        }

        private ObservableCollection<string> _possibleWebAppList;
        public ObservableCollection<string> PossibleWebAppList
        {
            get
            {
                return _possibleWebAppList;
            }
            set
            {
                _possibleWebAppList = value;
                OnPropertyChange("PossibleWebAppList");
            }
        }
    }
}
