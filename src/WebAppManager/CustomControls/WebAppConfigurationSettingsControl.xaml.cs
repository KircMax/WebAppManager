// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents;

namespace Webserver.Api.Gui.CustomControls
{
    /// <summary>
    /// Interaction logic for WebAppConfigurationSettingsControl.xaml
    /// </summary>
    public partial class WebAppConfigurationSettingsControl : UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(WebAppConfigurationSettings),
                typeof(WebAppConfigurationSettingsControl));
        public WebAppConfigurationSettings Settings
        {
            get
            {
                return (WebAppConfigurationSettings)GetValue(SettingsProperty);
            }
            set
            {
                SetValue(SettingsProperty, value);
            }
        }
        public WebAppConfigurationSettingsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void UpdateRadioButtonValuesGui()
        {
            if(this.Settings.StateRadioBtn == WebAppState.Enabled)
            {
                this.WebAppEnabledButton.IsChecked = true;
            }
            else if(this.Settings.StateRadioBtn == WebAppState.Disabled)
            {
                this.WebAppDisabledButton.IsChecked = true;
            }
            if(this.Settings.TypeRadioBtn == WebAppType.User)
            {
                this.WebAppStateUserButton.IsChecked = true;
            }
            else if(this.Settings.TypeRadioBtn == WebAppType.VoT)
            {
                this.WebAppStateVoTButton.IsChecked = true;
            }
        }
    }
}
