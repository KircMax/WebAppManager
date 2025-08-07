// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Newtonsoft.Json;
using Siemens.Simatic.S7.Webserver.API.Enums;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.FileParser;
using Siemens.Simatic.S7.Webserver.API.Services.WebApp;
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webserver.Api.Gui.CustomControls;

namespace Webserver.Api.Gui.Settings
{
    
    public class WebAppConfigurationSettings : INotifyPropertyChanged
    {
        private ApiWebAppData _webAppData;
        public ApiWebAppData WebAppData
        {
            get
            {
                return _webAppData;
            }
            set
            {
                _webAppData = value;
                OnPropertyChange("WebAppData");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public string DefaultPage
        {
            get
            {
                return WebAppData.Default_page;
            }
            set
            {
                WebAppData.Default_page = value;
                OnPropertyChange("DefaultPage");
            }
        }
        [JsonIgnore]
        public string NotAuthorizedPage
        {
            get
            {
                return WebAppData.Not_authorized_page;
            }
            set
            {
                WebAppData.Not_authorized_page = value;
                OnPropertyChange("NotAuthorizedPage");
            }
        }
        [JsonIgnore]
        public string NotFoundPage
        {
            get
            {
                return WebAppData.Not_found_page;
            }
            set
            {
                WebAppData.Not_found_page = value;
                OnPropertyChange("NotFoundPage");
            }
        }

        private string _resToIgnForUpl;
        private string _dirToIgnForUpl;
        private string _fileExtToIgnForUpl;
        private string _protectedRes;

        public string ProtectedResourcesGui
        {
            get
            {
                return _protectedRes;
            }
            set
            {
                _protectedRes = value;
                SetProtectedResourcesForUpload(_protectedRes);
                OnPropertyChange("ProtectedResourcesGui");
            }
        }
        
        public string ResourcesToIgnoreForUploadGui
        {
            get
            {
                return _resToIgnForUpl;
            }
            set
            {
                _resToIgnForUpl = value;
                SetResouresToIgnoreForUpload(_resToIgnForUpl);
                OnPropertyChange("ResourcesToIgnoreForUploadGui");
            }
        }
        
        public string DirectoriesToIgnoreForUploadGui
        {
            get
            {
                return _dirToIgnForUpl;
            }
            set
            {
                _dirToIgnForUpl = value;
                SetDirectoriesToIgnoreForUpload(_dirToIgnForUpl);
                OnPropertyChange("DirectoriesToIgnoreForUploadGui");
            }
        }
        
        public string FileExtensionsToIgnoreForUploadGui
        {
            get
            {
                return _fileExtToIgnForUpl;
            }
            set
            {
                _fileExtToIgnForUpl = value;
                SetFileExtensionsToIgnoreForUpload(_fileExtToIgnForUpl);
                OnPropertyChange("FileExtensionsToIgnoreForUploadGui");
            }
        }
        

        private WebAppState _stateRadioBtn;
        public WebAppState StateRadioBtn
        {
            get
            {
                return _stateRadioBtn;
            }
            set
            {
                _stateRadioBtn = value;
                switch(_stateRadioBtn)
                {
                    case WebAppState.Enabled:
                        WebAppData.State = ApiWebAppState.Enabled;
                            break;
                    case WebAppState.Disabled:
                        WebAppData.State = ApiWebAppState.Disabled;
                            break;
                    default:
                        break;
                }
                OnPropertyChange("StateRadioBtn");
            }
        }

        private WebAppRedirectMode _redirectRadioBtn;
        public WebAppRedirectMode RedirectRadioBtn
        {
            get
            {
                return _redirectRadioBtn;
            }
            set
            {
                _redirectRadioBtn = value;
                switch (_redirectRadioBtn)
                {
                    case WebAppRedirectMode.Forward:
                        WebAppData.Redirect_mode = ApiWebAppRedirectMode.Forward;
                        break;
                    case WebAppRedirectMode.Redirect:
                        WebAppData.Redirect_mode = ApiWebAppRedirectMode.Redirect;
                        break;
                    default:
                        break;
                }
                OnPropertyChange("RedirectRadioBtn");
            }
        }

        private WebAppType _typeRadioBtn;
        public WebAppType TypeRadioBtn
        {
            get
            {
                return _typeRadioBtn;
            }
            set
            {
                _typeRadioBtn = value;
                switch (_typeRadioBtn)
                {
                    case WebAppType.User:
                        WebAppData.Type = ApiWebAppType.User;
                        break;
                    case WebAppType.VoT:
                        WebAppData.Type = ApiWebAppType.VoT;
                        break;
                    default:
                        break;
                }
                OnPropertyChange("TypeRadioBtn");
            }
        }



        private ObservableCollection<string> appResNameList;
        
        public ObservableCollection<string> AppResourceNamesList
        {
            get
            {
                return appResNameList;
            }
            set
            {
                appResNameList = value;
                OnPropertyChange("AppResourceNamesList");
            }
        }

        
        public WebAppConfigurationSettings()
        {
            if(WebAppData == null)
            {
                WebAppData = new ApiWebAppData();
            }
        }
        
        public WebAppConfigurationSettings(ApiWebAppData data)
        {
            this.WebAppData = data.ShallowCopy();
            LoadWebAppValuesToGui();
        }
        
       

        public void SetProtectedResourcesForUpload(string protectedResources)
        {
            var toSet = protectedResources.Split(',').ToList() ?? new List<string>();
            toSet.RemoveAll(el => el == "");
            WebAppData.ProtectedResources = toSet;
            UpdateWebAppDataResourcesAndList();
        }

        public void SetResouresToIgnoreForUpload(string resourcesToIgnore)
        {
            var toSet = resourcesToIgnore.Split(',').ToList() ?? new List<string>();
            toSet.RemoveAll(el => el == "");
            WebAppData.ResourcesToIgnoreForUpload = toSet;
            UpdateWebAppDataResourcesAndList();
        }

        public void SetDirectoriesToIgnoreForUpload(string directoriesToIgnore)
        {
            var toSet = directoriesToIgnore.Split(',').ToList() ?? new List<string>();
            toSet.RemoveAll(el => el == "");
            WebAppData.DirectoriesToIgnoreForUpload = toSet;
            UpdateWebAppDataResourcesAndList();
        }

        public void SetFileExtensionsToIgnoreForUpload(string fileExtensionsToIgnore)
        {
            var toSet = fileExtensionsToIgnore.Split(',').ToList() ?? new List<string>();
            toSet.RemoveAll(el => el == "");
            WebAppData.FileExtensionsToIgnoreForUpload = toSet;
            UpdateWebAppDataResourcesAndList();
        }

        private ApiWebAppConfigParser _configParser;
        private ApiWebAppConfigParser ConfigParser
        {
            get
            {
                if(_configParser == null)
                {
                    _configParser = new ApiWebAppConfigParser(this.WebAppData.PathToWebAppDirectory, "WebAppConfig.json", new ApiWebAppResourceBuilder(), false);
                }
                return _configParser;
            }
            set
            {
                _configParser = value;
            }
        }

        public void UpdateWebAppDataResourcesAndList()
        {
            this.WebAppData.ApplicationResources = ConfigParser.RecursiveGetResources(WebAppData.PathToWebAppDirectory, this.WebAppData);
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach(var res in this.WebAppData.ApplicationResources)
            {
                list.Add(res.Name);
            }
            this.AppResourceNamesList = list;
        }

        public void LoadWebAppValuesToGui()
        {
            DefaultPage = this.WebAppData.Default_page;
            NotFoundPage = this.WebAppData.Not_found_page;
            NotAuthorizedPage = this.WebAppData.Not_authorized_page;
            this.WebAppData.ProtectedResources.ForEach(el => ProtectedResourcesGui += el + ",");
            this.WebAppData.ResourcesToIgnoreForUpload.ForEach(el => ResourcesToIgnoreForUploadGui += el + ",");
            this.WebAppData.FileExtensionsToIgnoreForUpload.ForEach(el => FileExtensionsToIgnoreForUploadGui += el + ",");
            this.WebAppData.DirectoriesToIgnoreForUpload.ForEach(el => DirectoriesToIgnoreForUploadGui += el + ",");
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (var resource in WebAppData.ApplicationResources)
            {
                list.Add(resource.Name);
            }
            this.appResNameList = list;
            if(WebAppData.State == ApiWebAppState.Enabled)
            {
                StateRadioBtn = WebAppState.Enabled;
            }
            else if(WebAppData.State == ApiWebAppState.Disabled)
            {
                StateRadioBtn = WebAppState.Disabled;
            }
            if(WebAppData.Type == ApiWebAppType.User)
            {
                TypeRadioBtn = WebAppType.User;
            }
            else if(WebAppData.Type == ApiWebAppType.VoT)
            {
                TypeRadioBtn = WebAppType.VoT;
            }
            if(WebAppData.Redirect_mode == ApiWebAppRedirectMode.Redirect)
            {
                RedirectRadioBtn = WebAppRedirectMode.Redirect;
            }
            else if(WebAppData.Redirect_mode == ApiWebAppRedirectMode.Forward)
            {
                RedirectRadioBtn = WebAppRedirectMode.Forward;
            }
        }
    }
}
