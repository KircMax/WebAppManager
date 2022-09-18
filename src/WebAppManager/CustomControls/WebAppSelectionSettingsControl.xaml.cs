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
