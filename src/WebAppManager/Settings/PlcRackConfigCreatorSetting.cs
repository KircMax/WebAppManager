// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
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
