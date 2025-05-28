// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.FileParser;
using Siemens.Simatic.S7.Webserver.API.Services.RequestHandling;
using Siemens.Simatic.S7.Webserver.API.Services.WebApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Webserver.Api.Gui.CustomControls;
using Webserver.Api.Gui.Pages;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents;
using Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Webserver.Api.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string SaveSettingsFilePath;


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

        public MainWindow()
        {
            this.Title = $"WebApplicationManager @ {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            InitSettings();

            InitializeComponent();

            DataContext = this;

            InitControlSettings();

        }

        private void InitControlSettings()
        {
            WebAppDeploySelectionSettingsControl.Settings = this.ApplicationSettings.WebAppDeploySelectionSettings;
            PlcRackSelectionSettingsControl.Settings = this.ApplicationSettings.RackSelectionSettings;
            this.PlcRackSelectionSettingsControl.SelectionSettingsAvailableItemsChanged += AvailableItemsPlcRackChanged;
            this.WebAppDeploySelectionSettingsControl.SelectionSettingsAvailableItemsChanged += AvailableItemsWebAppDeployChanged;
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
            SaveSettingsFilePath = path;
        }

        private async void StartDeploymentBtnAndCreateJsonConfigFile_Click(object sender, RoutedEventArgs e)
        {
            var serviceFactory = new ApiStandardServiceFactory();
            this.Cursor = System.Windows.Input.Cursors.Wait;
            SaveSettingsToJsonFile(SaveSettingsFilePath);
            List<ApiWebAppData> applicationsToDeploy = new List<ApiWebAppData>();
            foreach (var entry in this.ApplicationSettings.WebAppDeploySelectionSettings.SelectedItems)
            {
                var pathToApplication = this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                FileInfo fileInfo = new FileInfo(pathToApplication);
                var configParser = new ApiWebAppConfigParser(fileInfo.Directory.FullName, fileInfo.Name, new ApiWebAppResourceBuilder(), false);
                var app = configParser.Parse();
                applicationsToDeploy.Add(app);
            }
            List<ApiHttpClientRequestHandler> handlers = new List<ApiHttpClientRequestHandler>();
            List<ApiWebAppDeployer> deployers = new List<ApiWebAppDeployer>();
            var watches = new List<Stopwatch>();
            List<string> plcsToDeployTo = new List<string>();
            List<Task> tasks = new List<Task>();
            List<string> plcsToTrust = new List<string>();
            var message = "";
            ServicePointManager.ServerCertificateValidationCallback += (mysender, certificate, chain, sslPolicyErrors) =>
            {
                if (mysender is System.Net.HttpWebRequest)
                {
                    var mySenderRequest = mysender as HttpWebRequest;
                    var host = mySenderRequest.Address.Host;
                    return (plcsToTrust.Contains(host));
                }
                return false;
            };
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
                            var dialog = new LoginDialog();
                            string password = "";
                            string username = "";
                            dialog.PlcIpOrDnsNameInput.Text += plc;
                            if (dialog.ShowDialog() == true)
                            {
                                password = dialog.PasswordNameTextBox.Password;
                                username = dialog.UserNameTextBox.Text == "" ? "Everybody" : dialog.UserNameTextBox.Text;
                            }
                            else
                            {
                                username = "Everybody";
                                password = "";
                            }
                            ApiHttpClientRequestHandler requestHandler = null;
                            try
                            {
                                requestHandler = (ApiHttpClientRequestHandler)await serviceFactory.GetApiHttpClientRequestHandlerAsync(plc, username, password);
                            }
                            catch (HttpRequestException ex)
                            {
                                if (ex.InnerException is WebException)
                                {
                                    var assumeItsExpectedWebException = true; // created this bool since the exception message is languagedependant
                                    if (ex.InnerException.Message == "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."
                                        ||ex.Message== "Die zugrunde liegende Verbindung wurde geschlossen: Für den geschützten SSL/TLS-Kanal konnte keine Vertrauensstellung hergestellt werden.."
                                        || assumeItsExpectedWebException)
                                    {
                                        var result = System.Windows.MessageBox.Show("The plc certificate was not considered trusted! Do you want to connect anyways?", "ERR_CERT_AUTHORITY_INVALID", MessageBoxButton.YesNo);
                                        switch (result)
                                        {
                                            case MessageBoxResult.Yes:
                                                plcsToTrust.Add(plc);
                                                break;
                                        }
                                        requestHandler = (ApiHttpClientRequestHandler)await serviceFactory.GetApiHttpClientRequestHandlerAsync(plc, username, password);
                                    }
                                    else
                                    {
                                        throw ex;
                                    }
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                            handlers.Add(requestHandler);
                            var deployer = (ApiWebAppDeployer)serviceFactory.GetApiWebAppDeployer(requestHandler);
                            deployers.Add(deployer);
                        }
                    }
                    catch (Exception ex)
                    {
                        message = ex.GetType() + ex.Message + " has occured!";
                        System.Windows.MessageBox.Show(message);
                    }
                }
            }
            Stopwatch overallwatch = new Stopwatch();
            overallwatch.Start();
            foreach (var app in applicationsToDeploy)
            {
                foreach (var depl in deployers)
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        watches.Add(stopwatch);
                        stopwatch.Start();
                        tasks.Add(Task.Run(async () =>
                        {
                            await depl.DeployOrUpdateAsync(app);
                        }));
                        stopwatch.Stop();
                        Console.WriteLine($"Successfully deployed app {app.Name} in {stopwatch.Elapsed}");
                    }
                    catch (Exception ex)
                    {
                        message += $"DeployOrUpdate failed for {app.Name} with {Environment.NewLine}{ex.GetType()}:{ex.Message}";
                    }
                }
                if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(10)))
                {
                    message += "could not successfully deploy all apps!";
                }
                else
                {
                    foreach(var handler in handlers)
                    {
                        await handler.ApiLogoutAsync();
                    }
                }
            }
            if (string.IsNullOrEmpty(message) && applicationsToDeploy.Count > 0 && deployers.Count > 0)
            {
                message = $"Successfully deployed all WebApplications in {overallwatch.Elapsed}";
            }
            else
            {
                if (applicationsToDeploy.Count == 0)
                {
                    message = $"No application to Deploy.";
                }

                if (deployers.Count == 0)
                {
                    message = $"{message} No PLC to deploy in.";
                }
            }
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            System.Windows.MessageBox.Show(message.Trim());
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
            webAppConfigCreatorWindow.Show();
            this.Close();
        }

        private void PlcRackConfigCreatorWindow_Click(object sender, RoutedEventArgs e)
        {
            PlcRackConfigCreatorWindow plcRackConfigCreatorWindow = new PlcRackConfigCreatorWindow();
            //plcRackConfigCreatorWindow.Owner = this;
            SetWindowScreen(plcRackConfigCreatorWindow, GetWindowScreen(App.Current.MainWindow));
            plcRackConfigCreatorWindow.Show();
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
                            if(!string.IsNullOrEmpty(rack.SelectedRack))
                            {
                                // rack is consistent!
                                if(!this.ApplicationSettings.RackSelectionSettings.AvailableItems.Any(el => el.Value == rack.SelectedRack))
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
            if(!string.IsNullOrEmpty(message))
                System.Windows.MessageBox.Show(message);
        }

        private void ShowHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string fileName = System.IO.Path.Combine(CurrentExeDir.FullName, "README.html");
            Process.Start(fileName);
        }

        private async void StartDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsToJsonFile(SaveSettingsFilePath);
            var serviceFactory = new ApiStandardServiceFactory();
            List<ApiWebAppData> applicationsToDelete = new List<ApiWebAppData>();
            foreach (var entry in this.ApplicationSettings.WebAppDeploySelectionSettings.SelectedItems)
            {
                var pathToApplication = this.ApplicationSettings.WebAppDeploySelectionSettings.AvailableItems.First(el => el.Value == entry).Key;
                FileInfo fileInfo = new FileInfo(pathToApplication);
                var configParser = new ApiWebAppConfigParser(fileInfo.Directory.FullName, fileInfo.Name, new ApiWebAppResourceBuilder(), false);
                var app = configParser.Parse();
                applicationsToDelete.Add(app);
            }
            List<ApiHttpClientRequestHandler> handlers = new List<ApiHttpClientRequestHandler>();
            var watches = new List<Stopwatch>();
            List<string> plcsToDeleteWebAppsFrom = new List<string>();
            List<Task> tasks = new List<Task>();
            List<string> plcsToTrust = new List<string>();
            var message = "";
            ServicePointManager.ServerCertificateValidationCallback += (mysender, certificate, chain, sslPolicyErrors) =>
            {
                if (mysender is System.Net.HttpWebRequest)
                {
                    var mySenderRequest = mysender as HttpWebRequest;
                    var host = mySenderRequest.Address.Host;
                    return (plcsToTrust.Contains(host));
                }
                return false;
            };
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
                            var dialog = new LoginDialog();
                            string password = "";
                            string username = "";
                            dialog.PlcIpOrDnsNameInput.Text += plc;
                            if (dialog.ShowDialog() == true)
                            {
                                password = dialog.PasswordNameTextBox.Password;
                                username = dialog.UserNameTextBox.Text == "" ? "Everybody" : dialog.UserNameTextBox.Text;
                            }
                            else
                            {
                                username = "Everybody";
                                password = "";
                            }

                            ApiHttpClientRequestHandler requestHandler = null;
                            try
                            {
                                requestHandler = (ApiHttpClientRequestHandler)await serviceFactory.GetApiHttpClientRequestHandlerAsync(plc, username, password);
                            }
                            catch (HttpRequestException ex)
                            {
                                if (ex.InnerException is WebException)
                                {
                                    var assumeItsExpectedWebException = true; // created this bool since the exception message is languagedependant
                                    if (ex.InnerException.Message == "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."
                                        || ex.Message == "Die zugrunde liegende Verbindung wurde geschlossen: Für den geschützten SSL/TLS-Kanal konnte keine Vertrauensstellung hergestellt werden.."
                                        || assumeItsExpectedWebException)
                                    {
                                        var result = System.Windows.MessageBox.Show("The plc certificate was not considered trusted! Do you want to connect anyways?", "ERR_CERT_AUTHORITY_INVALID", MessageBoxButton.YesNo);
                                        switch (result)
                                        {
                                            case MessageBoxResult.Yes:
                                                plcsToTrust.Add(plc);
                                                break;
                                        }
                                        requestHandler = (ApiHttpClientRequestHandler)await serviceFactory.GetApiHttpClientRequestHandlerAsync(plc, username, password);
                                    }
                                    else
                                    {
                                        throw ex;
                                    }
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                            handlers.Add(requestHandler);
                        }
                    }
                    catch (Exception ex)
                    {
                        message = ex.GetType() + ex.Message + " has occured! " +
                                  "Check if the web server is activated on the plc";
                        System.Windows.MessageBox.Show(message);
                    }
                }
            }
            Stopwatch overallwatch = new Stopwatch();
            overallwatch.Start();
            foreach (var app in applicationsToDelete)
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        watches.Add(stopwatch);
                        stopwatch.Start();
                        tasks.Add(Task.Run(async () =>
                        {
                            await handler.WebAppDeleteAsync(app);
                        }));
                        stopwatch.Stop();
                        Console.WriteLine($"Successfully deleted app {app.Name} in {stopwatch.Elapsed}");
                    }
                    catch (Exception ex)
                    {
                        message += $"Delete App failed for {app.Name} with {Environment.NewLine}{ex.GetType()}:{ex.Message}";
                    }
                }
                if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(10)))
                {
                    message += "could not successfully deploy all apps!";
                }
            }

            if (string.IsNullOrEmpty(message) && handlers.Count > 0)
            {
                message = $"Successfully deployed all WebApplications in {overallwatch.Elapsed}";
            }
            else
            {
                if (applicationsToDelete.Count == 0)
                {
                    message = $"No application to Delete.";
                }

                if (handlers.Count == 0)
                {
                    message = $"{message} No PLC.";
                }
            }
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            if (!string.IsNullOrEmpty(message))
                System.Windows.MessageBox.Show(message.Trim());
        }
    }
}
