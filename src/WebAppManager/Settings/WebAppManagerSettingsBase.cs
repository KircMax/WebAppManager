// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;

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
