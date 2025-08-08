// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Webserver.Api.Gui.Settings;

namespace Webserver.Api.Gui.CustomControls
{
    /// <summary>
    /// Interaction logic for PlcRackConfigCreatorControl.xaml
    /// </summary>
    public partial class PlcRackConfigCreatorControl : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(PlcRackConfigCreatorControlSettings),
                typeof(PlcRackConfigCreatorControl));

        private string SettingsDirectory;
        private string webAppManagerDirectoryPath;
        private string webAppManagerSettingsPath;

        public PlcRackConfigCreatorControlSettings Settings
        {
            get
            {
                return (PlcRackConfigCreatorControlSettings)GetValue(SettingsProperty);
            }
            set
            {
                SetValue(SettingsProperty, value);
            }
        }
        /// <summary>
        /// Current Executing Directory
        /// </summary>
        public static DirectoryInfo CurrentExeDir
        {
            get
            {
                string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return (new FileInfo(dllPath)).Directory;
            }
        }

        public PlcRackConfigCreatorControl()
        {
            InitSettings();
            InitializeComponent();
        }

        private void InitSettings()
        {
            Settings = new PlcRackConfigCreatorControlSettings();
            SettingsDirectory = System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName, StandardValues.RackConfigDirectoryName);
            webAppManagerDirectoryPath = System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName);
            webAppManagerSettingsPath = System.IO.Path.Combine(webAppManagerDirectoryPath, StandardValues.StandardSaveFileName);
        }

        private void SelectRackToConfigure_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var rackName = e.AddedItems[0].ToString();
                var rackFilePath = Settings.RackConfigurations.First(el => el.Value.SelectedRack == rackName).Key;
                if (!File.Exists(rackFilePath))
                {
                    System.Windows.MessageBox.Show($"File not found: {rackFilePath}");
                }
                else
                {
                    string configFile = File.ReadAllText(rackFilePath);
                    var rack = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(configFile);
                    string guiString = "";
                    rack.RackPlcs.ForEach(el => guiString += el + ",");
                    Settings.FileName = rack.FileName;
                    Settings.RackPlcsGui = guiString;
                }
            }
        }

        public void UpdateRackNames()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (var rack in Settings.RackConfigurations.Values)
            {
                list.Add(rack.SelectedRack);
            }
            Settings.RackNameList = list;
        }

        private void AddRackConfiguration_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            if (!Settings.RackConfigurations.Any(el => el.Value.SelectedRack == Settings.NewRackName) && !(string.IsNullOrEmpty(Settings.NewRackName)))
            {
                string filename = $"RackConfiguration{Settings.RackConfigurations.Count}.json";
                var filePath = System.IO.Path.Combine(SettingsDirectory, filename);
                var configToAdd = new PlcRackConfigCreatorControlSettings()
                {
                    SelectedRack = Settings.NewRackName,
                    RackPlcs = new List<string>(),
                    FileName = filename
                };
                Settings.RackConfigurations[filePath] = configToAdd;
                configToAdd.Save(filePath);
                string configFile = File.ReadAllText(webAppManagerSettingsPath);
                var webAppManagerSettings = JsonConvert.DeserializeObject<WebAppManagerSettings>(configFile);
                webAppManagerSettings.RackSelectionSettings.AvailableItems[System.IO.Path.Combine(SettingsDirectory, filename)] = Settings.NewRackName;
                webAppManagerSettings.Save(webAppManagerDirectoryPath);
                UpdateRackNames();
                message += $"Added Rack {Settings.NewRackName}";
                Settings.NewRackName = "";
            }
            else
            {
                message += "Either the configuration already exists or the name is invalid!";
            }
            System.Windows.MessageBox.Show(message);
        }

        private void Save_Rack_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            try
            {
                Settings.Save(System.IO.Path.Combine(SettingsDirectory, Settings.FileName));
                message += $"saved Rack {Settings.SelectedRack} successfully to {Settings.FileName}!";
            }
            catch (Exception ex)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show(
                        $"could not save rack {Settings.SelectedRack} " +
                        $"{Environment.NewLine}{Settings.FileName} successfully!" +
                        $"{Environment.NewLine}{ex.GetType()}{ex.Message}",
                        "Save Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }
    }
}
