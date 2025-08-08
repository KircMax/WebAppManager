// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
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
            if (screen == null || window == null)
            {
                return;
            }

            try
            {
                if (!window.IsLoaded)
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }

                var workingArea = screen.WorkingArea;
                
                // Get DPI scaling factor
                var dpiScale = GetDpiScale(window);
                
                // Calculate scaled coordinates
                var scaledLeft = workingArea.Left / dpiScale.DpiScaleX;
                var scaledTop = workingArea.Top / dpiScale.DpiScaleY;
                var scaledWidth = workingArea.Width / dpiScale.DpiScaleX;
                var scaledHeight = workingArea.Height / dpiScale.DpiScaleY;
                
                // Ensure window size is reasonable and fits on screen
                var windowWidth = window.Width;
                var windowHeight = window.Height;
                
                // If window size is not set or NaN, use a default size
                if (double.IsNaN(windowWidth) || windowWidth <= 0)
                {
                    windowWidth = Math.Min(800, scaledWidth * 0.8);
                    window.Width = windowWidth;
                }
                
                if (double.IsNaN(windowHeight) || windowHeight <= 0)
                {
                    windowHeight = Math.Min(600, scaledHeight * 0.8);
                    window.Height = windowHeight;
                }
                
                // Center the window on the target screen, with some margin from edges
                var margin = 50 / dpiScale.DpiScaleX; // 50 pixel margin, scaled
                var left = scaledLeft + margin;
                var top = scaledTop + margin;
                
                // Ensure the window fits completely on the screen
                if (left + windowWidth > scaledLeft + scaledWidth)
                {
                    left = scaledLeft + scaledWidth - windowWidth - margin;
                }
                
                if (top + windowHeight > scaledTop + scaledHeight)
                {
                    top = scaledTop + scaledHeight - windowHeight - margin;
                }
                
                // Final bounds check
                left = Math.Max(scaledLeft, left);
                top = Math.Max(scaledTop, top);
                
                window.Left = left;
                window.Top = top;
            }
            catch (Exception ex)
            {
                // Fallback to center screen if anything goes wrong
                System.Windows.MessageBox.Show($"SetWindowScreen failed: {ex.Message}. Using fallback positioning.");
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        
        private (double DpiScaleX, double DpiScaleY) GetDpiScale(Window window)
        {
            try
            {
                var source = PresentationSource.FromVisual(window);
                if (source?.CompositionTarget != null)
                {
                    return (source.CompositionTarget.TransformToDevice.M11, 
                           source.CompositionTarget.TransformToDevice.M22);
                }
            }
            catch
            {
                // Fallback if we can't get DPI info
            }
            
            // Default DPI scaling (96 DPI = 1.0 scale)
            return (1.0, 1.0);
        }

        public Screen GetWindowScreen(Window window)
        {
            return Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle);
        }
    }
}
