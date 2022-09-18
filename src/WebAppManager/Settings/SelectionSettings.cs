// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class SelectionSettings : WebAppManagerSettingsBase
    {
        private ObservableCollection<string> _selectedItems;
        private Dictionary<string, string> _availableItems;

        public ObservableCollection<string> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
                OnPropertyChange("SelectedItems");
            }
        }
        
        public Dictionary<string, string> AvailableItems
        {
            get { return _availableItems; }
            set 
            {
                _availableItems = value;
                OnPropertyChange("AvailableItems");
                OnPropertyChange("AvailableItems.Values");
                OnPropertyChange("AvailableItems.Keys");
            }
        }

        public SelectionSettings()
        {
            SelectedItems = new ObservableCollection<string>();
            AvailableItems = new Dictionary<string, string>();
        }



    }
}
