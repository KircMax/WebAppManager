// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class WebAppManagerSettingsBase : NotifyPropertyChangeBase
    {
        private bool _isValid;

        [JsonIgnore]
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChange("IsValid");
            }
        }

    }
}
