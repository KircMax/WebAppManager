// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Webserver.Api.Gui.Settings;

namespace Webserver.Api.Gui.Pages
{
    /// <summary>
    /// Interaction logic for PlcRackConfigCreatorWindow.xaml
    /// </summary>
    public partial class PlcRackConfigCreatorWindow : Window
    {
        private bool isClosingManually = false;
        public static readonly DependencyProperty SettingsProperty =
           DependencyProperty.Register("Settings",
               typeof(PlcRackConfigCreatorSetting),
               typeof(PlcRackConfigCreatorWindow));


        public PlcRackConfigCreatorSetting Settings
        {
            get
            {
                return (PlcRackConfigCreatorSetting)GetValue(SettingsProperty);
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

        private string SettingsDirectory;
        private string webAppManagerDirectoryPath;
        private string webAppManagerSettingsPath;

        public PlcRackConfigCreatorWindow()
        {
            InitSettings();
            InitializeComponent();
            InitControls();
        }

        public void InitSettings()
        {
            Settings = new PlcRackConfigCreatorSetting();
            Settings.PlcRackConfigCreatorControlSettings = new PlcRackConfigCreatorControlSettings();
            var mainSettingsDirectory =
            SettingsDirectory = System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName, StandardValues.RackConfigDirectoryName);
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }
            webAppManagerDirectoryPath = System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName);
            webAppManagerSettingsPath = System.IO.Path.Combine(webAppManagerDirectoryPath, StandardValues.StandardSaveFileName);
            if (!File.Exists(webAppManagerSettingsPath))
            {
                throw new FileNotFoundException($"WebAppManagerSettings at {Environment.NewLine}{webAppManagerSettingsPath}{Environment.NewLine}not found!");
            }
            string configFile = File.ReadAllText(webAppManagerSettingsPath);
            var webAppManagerSettings = JsonConvert.DeserializeObject<WebAppManagerSettings>(configFile);
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (var key in webAppManagerSettings.RackSelectionSettings.AvailableItems.Keys)
            {
                if (!File.Exists(key))
                {
                    System.Windows.MessageBox.Show($"File not found: {key}");
                }
                else
                {
                    var configcreatorSettingFileContent = File.ReadAllText(key);
                    var config = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(configcreatorSettingFileContent);
                    this.Settings.PlcRackConfigCreatorControlSettings.RackConfigurations[key] = config;
                }
            }
        }

        private void InitControls()
        {
            this.PlcRackConfigCreatorControl.Settings = Settings.PlcRackConfigCreatorControlSettings;
            this.PlcRackConfigCreatorControl.UpdateRackNames();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Only perform our custom closing if not already being handled
            if (!isClosingManually)
            {
                e.Cancel = true; // Cancel the default closing

                // Use Dispatcher to safely create and show the new window
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Create and show the main window first
                        MainWindow window = new MainWindow();
                        SetWindowScreen(window, GetWindowScreen(App.Current.MainWindow));
                        window.Show();

                        // Then mark as closing and close this window
                        isClosingManually = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error during window transition: {ex.Message}");
                    }
                }));
            }

            base.OnClosing(e);
        }

        // Modify your existing back button method to use the same pattern
        private void BackToMainApplication_Click(object sender, RoutedEventArgs e)
        {
            isClosingManually = true; // Set flag to prevent recursion

            MainWindow window = new MainWindow();
            SetWindowScreen(window, GetWindowScreen(App.Current.MainWindow));
            window.Show();
            this.Close();
        }

        public void SetWindowScreen(Window window, Screen screen)
        {
            if (screen != null)
            {
                if (!window.IsLoaded)
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }

                var workingArea = screen.WorkingArea;
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
            }
        }

        public Screen GetWindowScreen(Window window)
        {
            return Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle);
        }

    }
}
