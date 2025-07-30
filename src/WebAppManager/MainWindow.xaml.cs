// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.FileParser;
using Siemens.Simatic.S7.Webserver.API.Services.RequestHandling;
using Siemens.Simatic.S7.Webserver.API.Services.WebApp;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Webserver.Api.Gui.Pages;
using Webserver.Api.Gui.Settings;
using Webserver.Api.Gui.WebAppManagerEvents.WebAppMangagerEventArgs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
                if(MyProgressBar != null)
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

        public LogViewer LogViewer { get; set; }

        public MainWindow()
        {
            this.Title = $"WebApplicationManager @ {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            InitSettings();

            InitializeComponent();

            InitControlSettings();

            DataContext = this;
            LogViewer = new LogViewer();
            LogViewer.Show();
            ServiceFactory = new ApiStandardServiceFactory();
        }

        private void InitControlSettings()
        {
            WebAppDeploySelectionSettingsControl.Settings = this.ApplicationSettings.WebAppDeploySelectionSettings;
            PlcRackSelectionSettingsControl.Settings = this.ApplicationSettings.RackSelectionSettings;
            MyProgressBar.ProgressBarValue = this.ProgressBarValue;
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
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException)
                {
                    var assumeItsExpectedWebException = true; // created this bool since the exception message is languagedependant
                    if (ex.InnerException.Message == "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."
                        || ex.Message == "Die zugrunde liegende Verbindung wurde geschlossen: Für den geschützten SSL/TLS-Kanal konnte keine Vertrauensstellung hergestellt werden.."
                        || assumeItsExpectedWebException)
                    {
                        MessageBoxResult result = MessageBoxResult.Yes;
                        if (RunWithCertificateCallbackDialog)
                        {
                            result = System.Windows.MessageBox.Show($"The plc {plc} certificate was not considered trusted! Do you want to connect anyways?", "ERR_CERT_AUTHORITY_INVALID", MessageBoxButton.YesNo);
                        }
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                PlcsToTrust.Add(plc);
                                break;
                        }
                        requestHandler = (ApiHttpClientRequestHandler)await ServiceFactory.GetApiHttpClientRequestHandlerAsync(plc, credentials.UserName, credentials.Password);
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
            ProgressBarValue = new ProgressBarValue(0);
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
            var deployers = new List<(string plc, ApiWebAppDeployer deployer, ApiHttpClientRequestHandler requestHandler)>();
            List<string> plcsToDeployTo = new List<string>();
            List<Task> tasks = new List<Task>();
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
                foreach (var app in applicationsToDeploy)
                {
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
                                message.AppendLine($"DeployOrUpdate failed for {app.Name} to {deployer.plc} with {Environment.NewLine}{ex.GetType()}:{ex.Message}{Environment.NewLine}");
                                var currentException = ex.InnerException;
                                while (currentException != null)
                                {
                                    message.AppendLine($"Inner: {currentException.GetType()}:{currentException.Message}");
                                    currentException = currentException.InnerException;
                                }
                            }
                            progressAmount++;
                        });
                        tasks.Add(myTask);
                    }
                    if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(10)))
                    {
                        message.AppendLine($"could not successfully deploy all apps!");
                    }
                    try
                    {
                        var nextValue = progressAmount * 100 / overallAmount;
                        //LogMessage($"Set progress bar value to {nextValue}!");
                        ProgressBarValue = new ProgressBarValue(nextValue);
                        MyProgressBar.pbStatus.Value = nextValue;
                        //var window = new Window();
                        //window.Show();
                        //window.Close();
                        //var messageBox = new System.Windows.Forms.MessageBox();
                        //var result = System.Windows.MessageBox.Show($"{nextValue}");
                        /*
                         * var mBox = new System.Windows.MessageBox
                        {
                            Owner = this,
                            Content = message,
                            Title = "Message Box Title",
                            Button = MessageBoxButton.OKCancel
                        };*/
                        /*using(var box = new AutoCloseMessageBox())
                        {
                            box.Show();
                        }*/


                    }
                    catch (Exception ex2)
                    {
                        LogMessage($"{ex2.GetType()}: {ex2.Message}");
                    }
                }
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
                foreach (var handler in deployers)
                {
                    try
                    {
                        await handler.requestHandler.ApiLogoutAsync();
                    }
                    catch (Exception ex)
                    {
                        message.AppendLine($"Logout request for {handler.plc} failed. and {Environment.NewLine}{ex.GetType()}:{ex.InnerException.Message} and {Environment.NewLine}{Environment.NewLine}{ex.GetType()}:{ex.InnerException.InnerException.Message}");
                    }
                }
            }
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            // if not successfull deploy or show success deploy message -> show message
            var messageString = message.ToString().Trim();
            if (showMessageDeployed && deploySuccess || !deploySuccess)
            {
                //System.Windows.MessageBox.Show(messageString);
                ;
            }
            LogMessage(messageString, true);
            ProgressBarValue = new ProgressBarValue(100);
            return messageString;
        }

        private async void StartDeploymentBtnAndCreateJsonConfigFile_Click(object sender, RoutedEventArgs e)
        {
            PlcsToTrust = new List<string>();
            await DeployOnceAsync();
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
            PlcsToTrust = new List<string>();
            var result = await DeployOnceAsync(false);
            LogMessage(result, true);
        }

        private static object LogLock = new object();

        private List<string> messagesToBeLogged = new List<string>();

        private void LogMessage(string message, bool performLog = false)
        {
            if(performLog)
            {
                lock (LogLock)
                {
                    foreach(var messageToBeLogged in messagesToBeLogged)
                    {
                        LogViewer?.LogEntries?.Add(new LogEntry() { DateTime = DateTime.Now, Index = currentIndex, Message = messageToBeLogged });
                        currentIndex++;
                    }
                    messagesToBeLogged = new List<string>();
                    LogViewer?.LogEntries?.Add(new LogEntry() { DateTime = DateTime.Now, Index = currentIndex, Message = message });
                    currentIndex++;
                }
            }
            else
            {
                messagesToBeLogged.Add(message);
            }    
        }

        private async void StartContinuousDeploymentBtn_Click(object sender, RoutedEventArgs e)
        {
            StartDeploymentBtnAndCreateJsonConfigFile_Click(sender, null);
            if (_continuousDeploymentTimer != null)
            {
                _continuousDeploymentTimer.Stop();
                _continuousDeploymentTimer.Tick -= ContinuousDeploymentTimer_Tick;
            }
            _continuousDeploymentTimer = new DispatcherTimer();
            _continuousDeploymentTimer.Interval = TimeSpan.FromSeconds(5);
            _continuousDeploymentTimer.Tick += ContinuousDeploymentTimer_Tick;
            _continuousDeploymentTimer.Start();
            LogMessage($"Started continuous deployment in interval: {_continuousDeploymentTimer.Interval}!", true);
        }

        private async void StopContinuousDeploymentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_continuousDeploymentTimer != null)
            {
                _continuousDeploymentTimer.Stop();
                _continuousDeploymentTimer.Tick -= ContinuousDeploymentTimer_Tick;
            }
            LogMessage("Stopped continuous deployment!", true);
        }

        private async void StartDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsToJsonFile(SaveSettingsFilePath);
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
            var message = "";
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
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var stopwatch = new Stopwatch();
                            watches.Add(stopwatch);
                            stopwatch.Start();
                            await handler.WebAppDeleteAsync(app);
                            stopwatch.Stop();
                            LogMessage($"Successfully deleted app {app.Name} in {stopwatch.Elapsed}");
                        }
                        catch (Exception ex)
                        {
                            message += $"Delete App failed for {app.Name} with {Environment.NewLine}{ex.GetType()}:{ex.Message}";
                            var currentException = ex.InnerException;
                            while (currentException != null)
                            {
                                message += $"Inner: {currentException.GetType()}:{currentException.Message}";
                                currentException = currentException.InnerException;
                            }
                        }
                    }));
                }
                if (!Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(10)))
                {
                    message += "could not successfully delete all apps!";
                }
            }

            if (string.IsNullOrEmpty(message) && handlers.Count > 0)
            {
                message = $"Successfully deleted all WebApplications in {overallwatch.Elapsed}";
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
            //if (!string.IsNullOrEmpty(message))
            //System.Windows.MessageBox.Show(message.Trim());
            LogMessage(message, true);
        }
    }
}
