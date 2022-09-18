// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Webserver.Api.Gui.Settings
{
    public class WebAppManagerSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SelectionSettings _webAppDeploySelectionSettings;

        public SelectionSettings WebAppDeploySelectionSettings
        {
            get
            {
                return _webAppDeploySelectionSettings;
            }
            set
            {
                _webAppDeploySelectionSettings = value;
                OnPropertyChange("WebAppDeploySelectionSettings");
            }
        }

        private SelectionSettings _rackSelectionSettings;

        public SelectionSettings RackSelectionSettings
        {
            get
            {
                return _rackSelectionSettings;
            }
            set
            {
                _rackSelectionSettings = value;
                OnPropertyChange("RackSelectionSettings");
            }
        }

        public WebAppManagerSettings()
        {
            WebAppDeploySelectionSettings = new SelectionSettings();
            RackSelectionSettings = new SelectionSettings();
        }

        public void Save(string directoryToSaveTo)
        {
            string path = directoryToSaveTo;
            if (!directoryToSaveTo.EndsWith(StandardValues.StandardSaveFileName))
            {
                path = Path.Combine(directoryToSaveTo, StandardValues.StandardSaveFileName);
            }
            var settingsString = JsonConvert.SerializeObject(this,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
            using (StreamWriter sw = File.CreateText(path))
            {

                sw.Write(settingsString);
            }
        }

        protected void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
