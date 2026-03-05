// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.Windows;
using System.Windows.Controls;
using Webserver.Api.Gui.Settings;

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
            if (this.Settings.StateRadioBtn == WebAppState.Enabled)
            {
                this.WebAppEnabledButton.IsChecked = true;
            }
            else if (this.Settings.StateRadioBtn == WebAppState.Disabled)
            {
                this.WebAppDisabledButton.IsChecked = true;
            }
            if (this.Settings.TypeRadioBtn == WebAppType.User)
            {
                this.WebAppStateUserButton.IsChecked = true;
            }
            else if (this.Settings.TypeRadioBtn == WebAppType.VoT)
            {
                this.WebAppStateVoTButton.IsChecked = true;
            }
            if (this.Settings.RedirectRadioBtn == WebAppRedirectMode.Redirect)
            {
                this.WebAppRedirectModeRedirectButton.IsChecked = true;
            }
            else if (this.Settings.RedirectRadioBtn == WebAppRedirectMode.Forward)
            {
                this.WebAppRedirectModeForwardButton.IsChecked = true;
            }
            else if (this.Settings.RedirectRadioBtn == WebAppRedirectMode.None)
            {
                this.WebAppRedirectModeNoneButton.IsChecked = true;
            }
        }
    }
}
