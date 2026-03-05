// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.Windows;
using System.Windows.Controls;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents;

namespace Webserver.Api.Gui.CustomControls
{
    /// <summary>
    /// Interaction logic for WebAppDirectorySettingsControl.xaml
    /// </summary>
    public partial class WebAppDirectorySettingsControl : UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(WebAppDirectorySettings),
                typeof(WebAppDirectorySettingsControl));
        public WebAppDirectorySettings Settings
        {
            get
            {
                return (WebAppDirectorySettings)GetValue(SettingsProperty);
            }
            set
            {
                SetValue(SettingsProperty, value);
            }
        }

        public event WebAppDirectorySettingsChanged WebAppDirectorySettingsChanged;

        public WebAppDirectorySettingsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SelectWebAppDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var diag = new System.Windows.Forms.FolderBrowserDialog())
            {
                diag.ShowNewFolderButton = true;
                var result = diag.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedPath = diag.SelectedPath;
                    Settings.WebAppDirectory = selectedPath;
                    WebAppDirectorySettingsChanged?.Invoke(this, new WebAppManagerEvents.WebAppMangagerEventArgs.WebAppDirectorySettingsChangedArgs() { newDirectory = selectedPath });
                }
            }
        }


    }
}
