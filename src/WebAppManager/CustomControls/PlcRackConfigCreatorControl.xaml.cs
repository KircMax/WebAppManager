// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Interaction logic for PlcRackConfigCreatorControl.xaml
    /// </summary>
    public partial class PlcRackConfigCreatorControl : UserControl
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
                string configFile = File.ReadAllText(rackFilePath);
                var rack = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(configFile);
                string guiString = "";
                rack.RackPlcs.ForEach(el => guiString += el + ",");
                Settings.FileName = rack.FileName;
                Settings.RackPlcsGui = guiString;
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
            MessageBox.Show(message);
        }

        private void Save_Rack_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            try
            {
                Settings.Save(System.IO.Path.Combine(SettingsDirectory, Settings.FileName));
                message += $"saved Rack {Settings.SelectedRack} successfully to {Settings.FileName}!";
            }
            catch(Exception ex)
            {
                message += $"could not save rack {Settings.SelectedRack} to {Environment.NewLine}{Settings.FileName} successfully!{Environment.NewLine}{ex.GetType()}{ex.Message}";
            }
            MessageBox.Show(message);
        }
    }
}
