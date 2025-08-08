// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webserver.Api.Gui.Settings
{
    public class PlcRackConfigCreatorSetting : WebAppManagerSettingsBase
    {
        private PlcRackConfigCreatorControlSettings _plcRackConfigCreatorControlSettings;
        public PlcRackConfigCreatorControlSettings PlcRackConfigCreatorControlSettings
        {
            get
            {
                return _plcRackConfigCreatorControlSettings;
            }
            set
            {
                _plcRackConfigCreatorControlSettings = value;
                OnPropertyChange("PlcRackConfigCreatorControlSettings");
            }
        }
    }
}
