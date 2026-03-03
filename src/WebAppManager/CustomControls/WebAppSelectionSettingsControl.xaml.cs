// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.Windows;
using System.Windows.Controls;
using Webserver.Api.Gui.Settings;

namespace Webserver.Api.Gui.CustomControls
{
    /// <summary>
    /// Interaction logic for WebAppSelectionSettingControl.xaml
    /// </summary>
    public partial class WebAppSelectionSettingsControl : UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(WebAppSelectionSettings),
                typeof(WebAppSelectionSettingsControl));


        public WebAppSelectionSettings Settings
        {
            get
            {
                return (WebAppSelectionSettings)GetValue(SettingsProperty);
            }
            set
            {
                SetValue(SettingsProperty, value);
            }
        }

        public SelectionChangedEventHandler SelectionChangedEventHandler;

        public WebAppSelectionSettingsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void WebAppSelectionSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedEventHandler?.Invoke(sender, e);
        }
    }
}
