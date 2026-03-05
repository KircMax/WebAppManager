// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.FileParser;
using Siemens.Simatic.S7.Webserver.API.Services.RequestHandling;
using Siemens.Simatic.S7.Webserver.API.Services.WebApp;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using Webserver.Api.Gui.Pages;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs;

namespace Webserver.Api.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SaveSettingsFilePath;
        private bool _keepLogViewerOpenOnClose;

        public static readonly DependencyProperty ApplicationSettingsProperty =
            DependencyProperty.Register("ApplicationSettings",
                typeof(WebAppManagerSettings),
                typeof(MainWindow));

        public WebAppManagerSettings ApplicationSettings
        {
            get
            {
                return (WebAppManagerSettings)GetValue(ApplicationSettingsProperty);
            }
            set
            {
                SetValue(ApplicationSettingsProperty, value);
            }
        }


        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue",
                typeof(ProgressBarValue),
                typeof(MainWindow));


        public ProgressBarValue ProgressBarValue
        {
            get
            {
                //return (int)MyProgressBar.ProgressBarValue;
                //return (ProgressBarValue)GetValue(ProgressBarValueProperty);
                return MyProgressBar?.ProgressBarValue;
            }
            set
            {
                //MyProgressBar.ProgressBarValue = value;
                //SetValue(ProgressBarValueProperty, value);
                if (MyProgressBar != null)
                {
                    MyProgressBar.ProgressBarValue = value;
                }
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

        public static LogViewer LogViewer { get; set; }

        public static InMemoryLogSaver ApplicationLogger { get; set; }


        public MainWindow()
        {
            this.Title = $"WebApplicationManager @ {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            InitSettings();

            InitializeComponent();

            InitControlSettings();

            DataContext = this;
            if (LogViewer == null)
            {
                LogViewer = new LogViewer();
                LogViewer.Show();
            }

            if (ApplicationLogger == null)
            {
                // Get log level from settings or use default
                var logLevel = GetLogLevelFromSettings();
                ApplicationLogger = new InMemoryLogSaver(logLevel);
            }

            ServiceFactory = new ApiStandardServiceFactory(ApplicationLogger);

            // Ensure cleanup on window closing
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Stop and dispose timer
            if (_continuousDeploymentTimer != null)
            {
                _continuousDeploymentTimer.Stop();
                _continuousDeploymentTimer.Tick -= ContinuousDeploymentTimer_Tick;
                _continuousDeploymentTimer = null;
            }

            // Close LogViewer if it exists and this is a real application exit
            if (!_keepLogViewerOpenOnClose && LogViewer != null && !LogViewer.IsClosed)
            {
                LogViewer.Close();
            }
        }

        /// <summary>
        /// Parse log level from settings string to LogLevel enum
        /// </summary>
        /// <returns>LogLevel enum value</returns>
        private LogLevel GetLogLevelFromSettings()
        {
            var logLevelString = ApplicationSettings?.LogLevel;

            // Handle null or empty settings
            if (string.IsNullOrEmpty(logLevelString))
            {
#if DEBUG
                return LogLevel.Debug;
#else
                return LogLevel.Information;
#endif
            }

            // Try to parse the string to LogLevel enum
            if (Enum.TryParse<LogLevel>(logLevelString, true, out var logLevel))
            {
                return logLevel;
            }

            // Fallback to default if parsing fails
#if DEBUG
            return LogLevel.Debug;
#else
            return LogLevel.Information;
#endif
        }

        /// <summary>
        /// Handle log level selection change
        /// </summary>
        private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var newLogLevel = selectedItem.Content.ToString();

                // Update settings and save
                if (ApplicationSettings != null && ApplicationLogger != null)
                {
                    ApplicationSettings.LogLevel = newLogLevel;
                    SaveSettingsToJsonFile(SaveSettingsFilePath);

                    // Update the logger level directly instead of creating new instance
                    var logLevel = GetLogLevelFromSettings();
                    ApplicationLogger.Level = logLevel;

                    LogMessage($"Log level changed to: {newLogLevel}", true);
                }
            }
        }

        private void InitControlSettings()
        {
            WebAppDeploySelectionSettingsControl.Settings = this.ApplicationSettings.WebAppDeploySelectionSettings;
            PlcRackSelectionSettingsControl.Settings = this.ApplicationSettings.RackSelectionSettings;
            MyProgressBar.ProgressBarValue = this.ProgressBarValue;
            this.PlcRackSelectionSettingsControl.SelectionSettingsAvailableItemsChanged += AvailableItemsPlcRackChanged;
            this.WebAppDeploySelectionSettingsControl.SelectionSettingsAvailableItemsChanged += AvailableItemsWebAppDeployChanged;

            // Set the selected log level in the ComboBox
            if (LogLevelComboBox != null && !string.IsNullOrEmpty(ApplicationSettings.LogLevel))
            {
                foreach (ComboBoxItem item in LogLevelComboBox.Items)
                {
                    if (item.Content.ToString() == ApplicationSettings.LogLevel)
                    {
                        LogLevelComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void AvailableItemsWebAppDeployChanged(object sender, SelectionSettingsAvailableItemsChangedArgs e)
        {
            this.WebAppDeploySelectionSettingsControl.AvailableItemsSelect.Items.Refresh();
            SaveSettingsToJsonFile(SaveSettingsFilePath);
        }

        private void AvailableItemsPlcRackChanged(object sender, SelectionSettingsAvailableItemsChangedArgs e)
        {
            this.PlcRackSelectionSettingsControl.AvailableItemsSelect.Items.Refresh();
            SaveSettingsToJsonFile(SaveSettingsFilePath);
        }

        private void InitSettings()
        {
            ApplicationSettings = new WebAppManagerSettings();
            string SettingsDirectory = System.IO.Path.Combine(CurrentExeDir.FullName, StandardValues.SettingsDirectoryName);
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }
            string path = System.IO.Path.Combine(SettingsDirectory, StandardValues.StandardSaveFileName);
            if (File.Exists(path))
            {
                string configFile = File.ReadAllText(path);
                ApplicationSettings = JsonConvert.DeserializeObject<WebAppManagerSettings>(configFile);

                // Ensure LogLevel is set (for backward compatibility with old config files)
                if (string.IsNullOrEmpty(ApplicationSettings.LogLevel))
                {
#if DEBUG
                    ApplicationSettings.LogLevel = "Debug";
#else
                    ApplicationSettings.LogLevel = "Information";
#endif
                    SaveSettingsToJsonFile(path);
                }
            }
            else
            {
                var settingsString = JsonConvert.SerializeObject(ApplicationSettings,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                using (StreamWriter sw = File.CreateText(path))
                {

                    sw.Write(settingsString);
                }
            }
            ProgressBarValue = new ProgressBarValue(1);
            SaveSettingsFilePath = path;
        }

        public bool RunWithLoginDialog = true;
        public bool RunWithCertificateCallbackDialog = true;

        public HashSet<X509Certificate2> TemporarilyTrustedCertificates = new HashSet<X509Certificate2>();
        public Dictionary<string, NetworkCredential> CachedCredentials = new Dictionary<string, NetworkCredential>();

        public IApiServiceFactory ServiceFactory;

        private NetworkCredential GetCredentials(string plc)
        {
            string password = "";
            string username = "";
            if (CachedCredentials.ContainsKey(plc))
            {
                username = CachedCredentials[plc].UserName;
                password = CachedCredentials[plc].Password;
            }
            else
            {
                var dialog = new LoginDialog();
                dialog.PlcIpOrDnsNameInput.Text += plc;
                if (RunWithLoginDialog)
                {
                    if (dialog.ShowDialog() == true)
                    {
                        password = dialog.PasswordNameTextBox.Password;
                        username = dialog.UserNameTextBox.Text == "" ? "Anonymous" : dialog.UserNameTextBox.Text;
                        bool cache = dialog.CacheCredentialsCheckBox.IsChecked == true;
                        if (cache)
                        {
                            CachedCredentials.Add(plc, new NetworkCredential(username, password));
                        }
                    }
                    else
                    {
                        username = "Anonymous";
                        password = "";
                    }
                }
                else
                {
                    username = "Anonymous";
                    password = "";
                }
            }
            return new NetworkCredential(username, password);
        }

        private async Task<ApiHttpClientRequestHandler> GetRequestHandlerAsync(string plc, NetworkCredential credentials)
        {
            ApiHttpClientRequestHandler requestHandler = null;
            try
            {
                requestHandler = (ApiHttpClientRequestHandler)await ServiceFactory.GetApiHttpClientRequestHandlerAsync(plc, credentials.UserName, credentials.Password);
            }
            catch (HttpRequestException ex) when (ex.InnerException is WebException webEx)
            {
                // Check if this is specifically an SSL/TLS certificate error
                bool isCertificateError = ex.InnerException.Message.Contains("trust relationship") ||
                                         ex.InnerException.Message.Contains("Vertrauensstellung") ||
                                         ex.InnerException.Message.Contains("SSL") ||
                                         ex.InnerException.Message.Contains("TLS");

                if (isCertificateError)
                {
                    MessageBoxResult result = MessageBoxResult.Yes;
                    if (RunWithCertificateCallbackDialog)
                    {
                        result = System.Windows.MessageBox.Show($"The plc {plc} certificate was not considered trusted! Do you want to connect anyways?", "ERR_CERT_AUTHORITY_INVALID", MessageBoxButton.YesNo);
                    }

                    if (result == MessageBoxResult.Yes)
                    {
                        PlcsToTrust.Add(plc);
                        requestHandler = (ApiHttpClientRequestHandler)await ServiceFactory.GetApiHttpClientRequestHandlerAsync(plc, credentials.UserName, credentials.Password);
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            return requestHandler;
        }

        private async Task CheckPermissionsAsync(string plc, ApiHttpClientRequestHandler requestHandler, NetworkCredential credentials)
        {
            var permissions = await requestHandler.ApiGetPermissionsAsync();
            var permissionsString = string.Join(", ", permissions.Result.Select(el => el.ToString()));
            LogMessage($"Permissions for {plc} with user: {credentials.UserName} {permissionsString}");
            var manageUserPagesRight = "manage_user_pages";
            if (!permissions.Result.Any(el => el.Name == manageUserPagesRight))
            {
                // all good
                System.Windows.MessageBox.Show($"User does not seem to have right to {manageUserPagesRight}, rights found: {Environment.NewLine}{permissionsString}");
                CachedCredentials.Remove(plc);
            }
        }

        private async Task<string> DeployOnceAsync(bool showMessageDeployed = true)
        {
            PlcsToTrust = new List<string>();

            // Initialize progress
            await UpdateProgressAsync(0, "Initializing deployment...");

            this.Cursor = System.Windows.Input.Cursors.Wait;
            SaveSettingsToJsonFile(SaveSettingsFilePath);

            await UpdateProgressAsync(1, "Loading applications...");
            List<ApiWebAppData> applicationsToDeploy = new List<ApiWebAppData>();
            foreach (var entry in this.ApplicationSettings.WebAppDeploySelectionSettings.SelectedItems)
            {
                var pathToApplication = this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                FileInfo fileInfo = new FileInfo(pathToApplication);
                var configParser = new ApiWebAppConfigParser(fileInfo.Directory.FullName, fileInfo.Name, new ApiWebAppResourceBuilder(), false);
                var app = configParser.Parse();
                applicationsToDeploy.Add(app);
            }

            await UpdateProgressAsync(2, "Connecting to PLCs...");
            var deployers = new List<(string plc, ApiWebAppDeployer deployer, ApiHttpClientRequestHandler requestHandler)>();
            List<string> plcsToDeployTo = new List<string>();
            StringBuilder message = new StringBuilder();
            ServicePointManager.ServerCertificateValidationCallback += Certificate_Validation_Callback;

            foreach (var entry in this.ApplicationSettings.RackSelectionSettings.SelectedItems)
            {
                var pathToRackConfiguration = this.ApplicationSettings.RackSelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                var content = File.ReadAllText(pathToRackConfiguration);
                var rack = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(content);
                foreach (var plc in rack.RackPlcs)
                {
                    try
                    {
                        if (!plcsToDeployTo.Any(el => el == plc))
                        {
                            plcsToDeployTo.Add(plc);
                            var credentials = GetCredentials(plc);
                            ApiHttpClientRequestHandler requestHandler = null;
                            requestHandler = await GetRequestHandlerAsync(plc, credentials);
                            await CheckPermissionsAsync(plc, requestHandler, credentials);
                            var deployer = (ApiWebAppDeployer)ServiceFactory.GetApiWebAppDeployer(requestHandler);
                            deployers.Add((plc, deployer, requestHandler));
                        }
                    }
                    catch (Exception ex)
                    {
                        message.AppendLine(ex.GetType() + ex.Message + " has occured!");
                        System.Windows.MessageBox.Show(message.ToString());
                    }
                }
            }

            await UpdateProgressAsync(3, "Starting deployment...");
            Stopwatch overallwatch = new Stopwatch();
            overallwatch.Start();
            bool deploySuccess = false;
            try
            {
                int overallAmount = applicationsToDeploy.Count * deployers.Count;
                if (overallAmount == 0)
                {
                    overallAmount++;
                }
                int progressAmount = 0;
                int completedDeployments = 0;

                foreach (var app in applicationsToDeploy)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (var deployer in deployers)
                    {
                        var myTask = Task.Run(async () =>
                        {
                            var started = DateTime.Now;
                            try
                            {
                                LogMessage($"Start deploy app {app.Name} to {deployer.plc}");
                                await deployer.deployer.DeployOrUpdateAsync(app);
                                LogMessage($"Successfully deployed app {app.Name} to {deployer.plc} in {DateTime.Now - started}");
                            }
                            catch (Exception ex)
                            {
                                lock (message)
                                {
                                    message.AppendLine($"DeployOrUpdate failed for {app.Name} to {deployer.plc} with {Environment.NewLine}{ex.GetType()}:{ex.Message}{Environment.NewLine}");
                                    var currentException = ex.InnerException;
                                    while (currentException != null)
                                    {
                                        message.AppendLine($"Inner: {currentException.GetType()}:{currentException.Message}");
                                        currentException = currentException.InnerException;
                                    }
                                }
                            }
                            Interlocked.Increment(ref progressAmount);

                            // Update progress after each deployment
                            var currentCompleted = Interlocked.Increment(ref completedDeployments);
                            var percentComplete = 20 + ((currentCompleted * 70) / overallAmount);
                            await UpdateProgressAsync(percentComplete,
                                $"Deploying: {app.Name} ({currentCompleted}/{overallAmount})");
                        });
                        tasks.Add(myTask);
                    }

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        message.AppendLine($"One or more deployments failed: {ex.Message}");
                    }
                }

                await UpdateProgressAsync(90, "Finishing up...");

                if (string.IsNullOrEmpty(message.ToString()) && applicationsToDeploy.Count > 0 && deployers.Count > 0)
                {
                    message.AppendLine($"Successfully deployed all WebApplications in {overallwatch.Elapsed}");
                    deploySuccess = true;
                }
                else
                {
                    if (applicationsToDeploy.Count == 0)
                    {
                        message.AppendLine($"No application to Deploy.");
                    }
                    if (deployers.Count == 0)
                    {
                        message.AppendLine($"{message} No PLC to deploy in.");
                    }
                }
            }
            finally
            {
                await UpdateProgressAsync(95, "Logging out...");
                foreach (var handler in deployers)
                {
                    try
                    {
                        await handler.requestHandler.ApiLogoutAsync();
                    }
                    catch (Exception ex)
                    {
                        var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                        message.AppendLine($"Logout request for {handler.plc} failed: {ex.GetType()}:{ex.Message} - Inner: {innerMessage}");
                    }
                }
            }

            this.Cursor = System.Windows.Input.Cursors.Arrow;
            var messageString = message.ToString().Trim();
            if (showMessageDeployed && deploySuccess || !deploySuccess)
            {
                //System.Windows.MessageBox.Show(messageString);
                ;
            }

            var messages = ApplicationLogger?.LogMessages?.ToList() ?? new List<string>();
            foreach (var logMessage in messages)
            {
                LogMessage(logMessage, false);
            }
            if (ApplicationLogger != null)
            {
                ApplicationLogger.LogMessages = new List<string>();
            }
            LogMessage(messageString, true);

            await UpdateProgressAsync(100, deploySuccess ? "Deployment completed successfully!" : "Deployment completed with errors");
            return messageString;
        }

        /// <summary>
        /// Update progress bar with percentage and status text on UI thread
        /// </summary>
        private async Task UpdateProgressAsync(int percentage, string statusText)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                ProgressBarValue = new ProgressBarValue(Math.Min(percentage, 100), statusText);
                if (MyProgressBar != null)
                {
                    MyProgressBar.ProgressBarValue = ProgressBarValue;
                }
            });
        }

        private async void StartDeploymentBtnAndCreateJsonConfigFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDeploymentButtonsEnabled(false);
                await DeployOnceAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"Deployment failed: {ex.Message}", true);
                System.Windows.MessageBox.Show($"Deployment failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetDeploymentButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Enable/disable deployment buttons to prevent concurrent operations
        /// </summary>
        private void SetDeploymentButtonsEnabled(bool enabled)
        {
            if (StartDeploymentBtn != null)
                StartDeploymentBtn.IsEnabled = enabled;
            if (StartContinuousDeploymentBtn != null)
                StartContinuousDeploymentBtn.IsEnabled = enabled;
            if (StartDeleteBtn != null)
                StartDeleteBtn.IsEnabled = enabled;
        }

        #region File
        private void SaveSettingsAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (var diag = new System.Windows.Forms.SaveFileDialog())
            {
                diag.Filter = "Json Files|*.json";
                diag.InitialDirectory = System.IO.Path.GetDirectoryName(SaveSettingsFilePath);
                var res = diag.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    string file = diag.FileName;
                    string message = SaveSettingsToJsonFile(file);
                    System.Windows.MessageBox.Show(this, message);
                }
            }
        }
        private void SaveSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = SaveSettingsToJsonFile(SaveSettingsFilePath);
            System.Windows.MessageBox.Show(this, message);
        }

        private void LoadSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (var diag = new System.Windows.Forms.OpenFileDialog())
            {
                diag.Filter = "Json Files|*.json";
                diag.InitialDirectory = System.IO.Path.GetDirectoryName(SaveSettingsFilePath);
                diag.Multiselect = false;
                var res = diag.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    string configFile = File.ReadAllText(diag.FileName);
                    string message = "";
                    try
                    {
                        ApplicationSettings = JsonConvert.DeserializeObject<WebAppManagerSettings>(configFile);
                        message = $"Successfully loaded settings from {configFile}";
                        SaveSettingsFilePath = configFile;

                        // Update logger level after loading settings
                        if (ApplicationLogger != null)
                        {
                            var logLevel = GetLogLevelFromSettings();
                            ApplicationLogger.Level = logLevel;
                        }

                        // Update UI controls with new settings
                        if (LogLevelComboBox != null && !string.IsNullOrEmpty(ApplicationSettings.LogLevel))
                        {
                            foreach (ComboBoxItem item in LogLevelComboBox.Items)
                            {
                                if (item.Content.ToString() == ApplicationSettings.LogLevel)
                                {
                                    LogLevelComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        message = $"Could not load settings from {configFile}  {Environment.NewLine}{ex.GetType()}{ex.Message}";
                    }
                    System.Windows.MessageBox.Show(this, message);
                }
            }
        }

        #endregion

        private string SaveSettingsToJsonFile(string filePath)
        {
            string message = "";
            try
            {
                //this.ApplicationSettings.SaveToXml(new FileInfo(filePath));
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.Write(JsonConvert.SerializeObject(this.ApplicationSettings,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }));
                }
                message = $"Settings successfully saved to '{filePath}'";

            }
            catch (Exception ex)
            {

                message = $"Settings could not be saved: {Environment.NewLine}{ex.Message}";
            }
            return message;
        }

        private void WebAppConfigCreatorWindowCreator_Click(object sender, RoutedEventArgs e)
        {
            WebAppConfigCreatorWindow webAppConfigCreatorWindow = new WebAppConfigCreatorWindow();
            //webAppConfigCreatorWindow.Owner = this;
            SetWindowScreen(webAppConfigCreatorWindow, GetWindowScreen(App.Current.MainWindow));
            _keepLogViewerOpenOnClose = true;
            webAppConfigCreatorWindow.Show();
            this.Close();
        }

        private void PlcRackConfigCreatorWindow_Click(object sender, RoutedEventArgs e)
        {
            PlcRackConfigCreatorWindow plcRackConfigCreatorWindow = new PlcRackConfigCreatorWindow();
            //plcRackConfigCreatorWindow.Owner = this;
            SetWindowScreen(plcRackConfigCreatorWindow, GetWindowScreen(App.Current.MainWindow));
            _keepLogViewerOpenOnClose = true;
            plcRackConfigCreatorWindow.Show();
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
                var margin = StandardValues.DefaultWindowMargin / dpiScale.DpiScaleX;
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
                LogMessage($"SetWindowScreen failed: {ex.Message}. Using fallback positioning.", true);
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

        private void PlcRackConfigAdder_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            using (var diag = new OpenFileDialog())
            {
                diag.ShowHelp = true;
                var result = diag.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedPath = result;
                    var selectedFiles = diag.FileNames;
                    foreach (var selected in selectedFiles)
                    {
                        try
                        {
                            var rackContent = File.ReadAllText(selected);
                            var rack = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(rackContent);
                            if (!string.IsNullOrEmpty(rack.SelectedRack))
                            {
                                // rack is consistent!
                                if (!this.ApplicationSettings.RackSelectionSettings.AvailableItems.Any(el => el.Value == rack.SelectedRack))
                                {
                                    this.ApplicationSettings.RackSelectionSettings.AvailableItems.Add(selected, rack.SelectedRack);
                                    message += "successfully added file" + selected;
                                }
                                else
                                {
                                    message += $"A Rack with the name: {rack.SelectedRack} already exists!";
                                }
                            }
                            else
                            {
                                message += $"Rack {rack?.SelectedRack} does not have a valid configuration;";
                            }
                        }
                        catch (Exception ex)
                        {
                            message += ex.GetType() + ex.Message + "for file" + selected;
                        }
                    }
                    SaveSettingsToJsonFile(SaveSettingsFilePath);
                    this.PlcRackSelectionSettingsControl.AvailableItemsSelect.Items.Refresh();
                }
            }
            if (!string.IsNullOrEmpty(message))
                System.Windows.MessageBox.Show(message);
        }

        private void WebAppConfigAdder_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            using (var diag = new OpenFileDialog())
            {
                diag.ShowHelp = true;
                var result = diag.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedPath = result;
                    var selectedFiles = diag.FileNames;
                    foreach (var selected in selectedFiles)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(selected);
                            ApiWebAppConfigParser parser = new ApiWebAppConfigParser(fileInfo.Directory.FullName, fileInfo.Name, new ApiWebAppResourceBuilder(), false);
                            var app = parser.Parse();
                            // app is consistent!
                            var saveSetting = new ApiWebAppDataSaveSetting();
                            var saver = new ApiWebAppDataSaver(saveSetting);
                            saver.CheckConsistency_Saveable(app);
                            if (!this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.Any(el => el.Value == app.Name))
                            {
                                this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.Add(selected, app.Name);
                                message += "successfully added file" + selected;
                            }
                            else
                            {
                                message += $"An App with the name: {app.Name} already exists!";
                            }

                        }
                        catch (Exception ex)
                        {
                            message += ex.GetType() + ex.Message + "for file" + selected;
                        }
                    }
                    SaveSettingsToJsonFile(SaveSettingsFilePath);
                    this.WebAppDeploySelectionSettingsControl.AvailableItemsSelect.Items.Refresh();
                }
            }
            if (!string.IsNullOrEmpty(message))
                System.Windows.MessageBox.Show(message);
        }

        private void ShowHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string fileName = System.IO.Path.Combine(CurrentExeDir.FullName, "README.html");
            Process.Start(fileName);
        }

        private List<string> PlcsToTrust = new List<string>();

        bool Certificate_Validation_Callback(object mysender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (!RunWithCertificateCallbackDialog)
            {
                return true;
            }
            if (mysender is System.Net.HttpWebRequest)
            {
                var mySenderRequest = mysender as HttpWebRequest;
                var host = mySenderRequest.Address.Host;
                var cert = new X509Certificate2(certificate);
                if (PlcsToTrust.Contains(host))
                {
                    TemporarilyTrustedCertificates.Add(cert);
                }
                bool certIsTemporarilyTrusted = TemporarilyTrustedCertificates.Contains(cert);
                return certIsTemporarilyTrusted;
            }
            return false;
        }

        private DispatcherTimer _continuousDeploymentTimer;

        private int currentIndex = 0;

        private async void ContinuousDeploymentTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var result = await DeployOnceAsync(false);
                LogMessage(result, true);
            }
            catch (Exception ex)
            {
                LogMessage($"Continuous deployment error: {ex.Message}", true);
            }
        }

        private static object LogLock = new object();

        private List<string> messagesToBeLogged = new List<string>();

        private void LogMessage(string message, bool performLog = false)
        {
            if (performLog)
            {
                lock (LogLock)
                {
                    foreach (var messageToBeLogged in messagesToBeLogged)
                    {
                        LogViewer?.LogEntries?.Add(new LogEntry() { DateTime = DateTime.Now, Index = currentIndex, Message = messageToBeLogged });
                        currentIndex++;
                    }

                    // Thread-safe access to ApplicationLogger.LogMessages
                    var logMessages = ApplicationLogger?.LogMessages?.ToList() ?? new List<string>();
                    foreach (var logMessage in logMessages)
                    {
                        LogViewer?.LogEntries?.Add(new LogEntry() { Index = currentIndex, Message = logMessage });
                        currentIndex++;
                    }

                    if (ApplicationLogger != null)
                    {
                        ApplicationLogger.LogMessages = new List<string>();
                    }

                    messagesToBeLogged = new List<string>();
                    LogViewer?.LogEntries?.Add(new LogEntry() { DateTime = DateTime.Now, Index = currentIndex, Message = message });
                    currentIndex++;
                }
            }
            else
            {
                lock (LogLock)
                {
                    messagesToBeLogged.Add(message);
                }
            }
        }

        private async void StartContinuousDeploymentBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDeploymentButtonsEnabled(false);
                await DeployOnceAsync();
                if (_continuousDeploymentTimer != null)
                {
                    _continuousDeploymentTimer.Stop();
                    _continuousDeploymentTimer.Tick -= ContinuousDeploymentTimer_Tick;
                }
                _continuousDeploymentTimer = new DispatcherTimer();
                _continuousDeploymentTimer.Interval = StandardValues.ContinuousDeploymentInterval;
                _continuousDeploymentTimer.Tick += ContinuousDeploymentTimer_Tick;
                _continuousDeploymentTimer.Start();
                LogMessage($"Started continuous deployment in interval: {_continuousDeploymentTimer.Interval}!", true);

                // Enable stop button, keep deploy buttons disabled during continuous deployment
                if (StopContinuousDeploymentBtn != null)
                    StopContinuousDeploymentBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to start continuous deployment: {ex.Message}", true);
                System.Windows.MessageBox.Show($"Failed to start continuous deployment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetDeploymentButtonsEnabled(true);
            }
        }

        private async void StopContinuousDeploymentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_continuousDeploymentTimer != null)
            {
                _continuousDeploymentTimer.Stop();
                _continuousDeploymentTimer.Tick -= ContinuousDeploymentTimer_Tick;
                _continuousDeploymentTimer = null;
            }
            LogMessage("Stopped continuous deployment!", true);
            SetDeploymentButtonsEnabled(true);
        }

        private async void StartDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDeploymentButtonsEnabled(false);
                this.Cursor = System.Windows.Input.Cursors.Wait;
                await UpdateProgressAsync(0, "Starting delete operation...");
                List<ApiWebAppData> applicationsToDelete = new List<ApiWebAppData>();
                foreach (var entry in this.ApplicationSettings.WebAppDeploySelectionSettings.SelectedItems)
                {
                    var pathToApplication = this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                    FileInfo fileInfo = new FileInfo(pathToApplication);
                    var configParser = new ApiWebAppConfigParser(fileInfo.Directory.FullName, fileInfo.Name, new ApiWebAppResourceBuilder(), false);
                    var app = configParser.Parse();
                    applicationsToDelete.Add(app);
                }

                await UpdateProgressAsync(10, "Connecting to PLCs...");
                List<ApiHttpClientRequestHandler> handlers = new List<ApiHttpClientRequestHandler>();
                List<string> plcsToDeleteWebAppsFrom = new List<string>();
                List<Task> tasks = new List<Task>();
                StringBuilder message = new StringBuilder();
                PlcsToTrust = new List<string>();
                ServicePointManager.ServerCertificateValidationCallback += Certificate_Validation_Callback;

                foreach (var entry in this.ApplicationSettings.RackSelectionSettings.SelectedItems)
                {
                    var pathToRackConfiguration = this.ApplicationSettings.RackSelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                    var content = File.ReadAllText(pathToRackConfiguration);
                    var rack = JsonConvert.DeserializeObject<PlcRackConfigCreatorControlSettings>(content);
                    foreach (var plc in rack.RackPlcs)
                    {
                        try
                        {
                            if (!plcsToDeleteWebAppsFrom.Any(el => el == plc))
                            {
                                plcsToDeleteWebAppsFrom.Add(plc);
                                var credentials = GetCredentials(plc);
                                ApiHttpClientRequestHandler requestHandler = null;
                                requestHandler = await GetRequestHandlerAsync(plc, credentials);
                                await CheckPermissionsAsync(plc, requestHandler, credentials);
                                handlers.Add(requestHandler);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = ex.GetType() + ex.Message + " has occured! " +
                                      "Check if the web server is activated on the plc";
                            message.AppendLine(errorMsg);
                            System.Windows.MessageBox.Show(errorMsg);
                        }
                    }
                }

                Stopwatch overallwatch = new Stopwatch();
                overallwatch.Start();

                int totalOperations = applicationsToDelete.Count * handlers.Count;
                int completedOperations = 0;

                foreach (var app in applicationsToDelete)
                {
                    foreach (var handler in handlers)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var stopwatch = new Stopwatch();
                                stopwatch.Start();
                                await handler.WebAppDeleteAsync(app);
                                stopwatch.Stop();
                                LogMessage($"Successfully deleted app {app.Name} in {stopwatch.Elapsed}");

                                var current = Interlocked.Increment(ref completedOperations);
                                var percentComplete = 20 + ((current * 70) / Math.Max(totalOperations, 1));
                                await UpdateProgressAsync(percentComplete, $"Deleting: {app.Name} ({current}/{totalOperations})");
                            }
                            catch (Exception ex)
                            {
                                lock (message)
                                {
                                    message.Append($"Delete App failed for {app.Name} with {Environment.NewLine}{ex.GetType()}:{ex.Message}");
                                    var currentException = ex.InnerException;
                                    while (currentException != null)
                                    {
                                        message.Append($"{Environment.NewLine}Inner: {currentException.GetType()}:{currentException.Message}");
                                        currentException = currentException.InnerException;
                                    }
                                    message.AppendLine();
                                }
                            }
                        }));
                    }
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    message.AppendLine($"One or more delete operations failed: {ex.Message}");
                }

                overallwatch.Stop();

                await UpdateProgressAsync(95, "Finalizing...");

                if (string.IsNullOrEmpty(message.ToString()) && handlers.Count > 0)
                {
                    message.AppendLine($"Successfully deleted all WebApplications in {overallwatch.Elapsed}");
                }
                else
                {
                    if (applicationsToDelete.Count == 0)
                    {
                        message.AppendLine($"No application to Delete.");
                    }

                    if (handlers.Count == 0)
                    {
                        message.AppendLine($"{message} No PLC.");
                    }
                }

                var logMessages = ApplicationLogger?.LogMessages?.ToList() ?? new List<string>();
                foreach (var logMessage in logMessages)
                {
                    LogMessage(logMessage, false);
                }
                if (ApplicationLogger != null)
                {
                    ApplicationLogger.LogMessages = new List<string>();
                }
                LogMessage(message.ToString(), true);

                await UpdateProgressAsync(100, "Delete operation completed");
            }
            catch (Exception ex)
            {
                LogMessage($"Delete operation failed: {ex.Message}", true);
                await UpdateProgressAsync(0, "Delete operation failed");
                System.Windows.MessageBox.Show($"Delete operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                SetDeploymentButtonsEnabled(true);
            }
        }
    }
}
