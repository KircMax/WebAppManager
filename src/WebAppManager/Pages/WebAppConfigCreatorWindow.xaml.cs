// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services.FileParser;
using Siemens.Simatic.S7.Webserver.API.Services.WebApp;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs;
namespace Webserver.Api.Gui.Pages
{
    /// <summary>
    /// Interaction logic for WebAppConfigCreatorWindow.xaml
    /// </summary>
    public partial class WebAppConfigCreatorWindow : Window
    {
        // added flag to avoid constant loop on close
        private bool isClosingManually = false;
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings",
                typeof(WebAppConfigCreatorSettings),
                typeof(WebAppConfigCreatorWindow));

        public WebAppConfigCreatorSettings Settings
        {
            get
            {
                return (WebAppConfigCreatorSettings)GetValue(SettingsProperty);
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

        public WebAppConfigCreatorWindow()
        {
            this.Title = $"WebAppConfigCreator @ {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            InitSettings();

            InitializeComponent();

            DataContext = this;

            InitControlSettings();

            this.WebAppDirectorySettingsControl.WebAppDirectorySettingsChanged += OnWebAppDirectorySettingsChanged;
            this.WebAppSelectionSettingsControl.SelectionChangedEventHandler += OnWebAppSelectionSettingsChanged;
            OnWebAppDirectorySettingsChanged(this, new WebAppDirectorySettingsChangedArgs() { newDirectory = Settings.WebAppDirectorySettings.WebAppDirectory });
        }

        private void InitControlSettings()
        {
            this.WebAppDirectorySettingsControl.Settings = this.Settings.WebAppDirectorySettings;
            this.WebAppSelectionSettingsControl.Settings = this.Settings.WebAppSelectionSettings;
            this.WebAppConfigurationSettingsControl.Settings = this.Settings.WebAppConfigurationSettings;
        }

        public void InitSettings()
        {
            this.Settings = new WebAppConfigCreatorSettings();
        }

        private void OnWebAppDirectorySettingsChanged(object sender, WebAppDirectorySettingsChangedArgs e)
        {
            if(e.newDirectory != null)
            {
                ObservableCollection<string> list = new ObservableCollection<string>();
                foreach (var dir in Directory.GetDirectories(e.newDirectory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                    list.Add(directoryInfo.Name);
                }
                this.Settings.WebAppSelectionSettings.PossibleWebAppList = list;
            }
        }
        private void OnWebAppSelectionSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var directoryName = e.AddedItems[0].ToString();
                string webAppPath = System.IO.Path.Combine(this.Settings.WebAppDirectorySettings.WebAppDirectory, directoryName);
                var parser = new ApiWebAppConfigParser(webAppPath, StandardValues.StandardWebAppConfigSaveFileName, new ApiWebAppResourceBuilder(), false);
                var webAppData = new ApiWebAppData();
                if(!File.Exists(System.IO.Path.Combine(webAppPath, StandardValues.StandardWebAppConfigSaveFileName)))
                {
                    webAppData = new ApiWebAppData()
                    {
                        Name = directoryName,
                        PathToWebAppDirectory = webAppPath,
                        ProtectedResources = new List<string>(),
                        DirectoriesToIgnoreForUpload = new List<string>(),
                        FileExtensionsToIgnoreForUpload = new List<string>(),
                        ResourcesToIgnoreForUpload = new List<string>()
                    };
                    var resources = parser.RecursiveGetResources(webAppPath, webAppData);
                    webAppData.ApplicationResources = resources;
                }
                else
                {
                    webAppData = parser.Parse();
                }
                this.Settings.WebAppConfigurationSettings = new WebAppConfigurationSettings(webAppData);
                this.WebAppConfigurationSettingsControl.Settings = this.Settings.WebAppConfigurationSettings;
            }
        }

        private void Save_WebApp_Click(object sender, RoutedEventArgs e)
        {
            var saveSetting = new ApiWebAppDataSaveSetting();

            var saver = new ApiWebAppDataSaver(saveSetting);
            try
            {
                saver.Save(Settings.WebAppConfigurationSettings.WebAppData);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error saving WebApp configuration: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                    this.Close();
                return;
            }
            
            var webAppManagerSettings = new WebAppManagerSettings();
            string path = System.IO.Path.Combine(CurrentExeDir.FullName,StandardValues.SettingsDirectoryName, StandardValues.StandardSaveFileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"WebAppManagerSettings at {Environment.NewLine}{path}{Environment.NewLine}not found!");
            }
            string configFile = File.ReadAllText(path);
            webAppManagerSettings = JsonConvert.DeserializeObject<WebAppManagerSettings>(configFile);
            webAppManagerSettings.WebAppDeploySelectionSettings.AvailableItems[System.IO.Path.Combine(this.Settings.WebAppConfigurationSettings.WebAppData.PathToWebAppDirectory, StandardValues.StandardWebAppConfigSaveFileName)] = this.Settings.WebAppConfigurationSettings.WebAppData.Name;
            webAppManagerSettings.Save(System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName));
            System.Windows.Forms.MessageBox.Show("Saved successfully!");
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
