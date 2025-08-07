// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class PlcRackConfigCreatorControlSettings : WebAppManagerSettingsBase
    {
        private string _rackName;
        private string _newRackName;
        private List<string> _rackPlcs;
        private string _rackPlcsGui;

        private Dictionary<string, PlcRackConfigCreatorControlSettings> _rackConfigurations;

        [JsonIgnore]
        public Dictionary<string, PlcRackConfigCreatorControlSettings> RackConfigurations
        {
            get
            {
                return _rackConfigurations;
            }
            set
            {
                _rackConfigurations = value;
                OnPropertyChange("RackConfigurations");
            }
        }

        private ObservableCollection<string> _rackNameList;

        [JsonIgnore]
        public ObservableCollection<string> RackNameList
        {
            get
            {
                return _rackNameList;
            }
            set
            {
                _rackNameList = value;
                OnPropertyChange("RackNameList");
            }
        }

        public string SelectedRack
        {
            get
            {
                return _rackName;
            }
            set
            {
                _rackName = value;
                OnPropertyChange("SelectedRack");
            }
        }
        [JsonIgnore]
        public string NewRackName
        {
            get
            {
                return _newRackName;
            }
            set
            {
                _newRackName = value;
                OnPropertyChange("NewRackName");
            }
        }

        public List<string> RackPlcs
        {
            get
            {
                return _rackPlcs;
            }
            set
            {
                _rackPlcs = value;
                OnPropertyChange("RackPlcs");
            }
        }

        [JsonIgnore]
        public string RackPlcsGui
        {
            get
            {
                return _rackPlcsGui;
            }
            set
            {
                _rackPlcsGui = value;
                SetRackPlcs();
                OnPropertyChange("RackPlcsGui");
            }
        }

        public void SetRackPlcs()
        {
            var toSet = RackPlcsGui.Split(',').ToList() ?? new List<string>();
            toSet.RemoveAll(el => el == "");
            RackPlcs = toSet;
        }

        private string _fileName;

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnPropertyChange("FileName");
            }
        }

        public PlcRackConfigCreatorControlSettings()
        {
            RackNameList = new ObservableCollection<string>();
            RackConfigurations = new Dictionary<string, PlcRackConfigCreatorControlSettings>();
        }

        public void Save(string filePath)
        {
            string savePath = filePath;
            if (!filePath.EndsWith(this.FileName))
            {
                savePath = Path.Combine(filePath, this.FileName);
            }
            var settingsString = JsonConvert.SerializeObject(this,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.Write(settingsString);
            }
        }

    }
}
