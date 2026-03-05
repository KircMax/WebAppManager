// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System;

namespace Webserver.Api.Gui.Settings
{
    public static class StandardValues
    {
        public static string StandardSaveFileName = "WebAppManagerConfig.json";
        public static string StandardWebAppConfigSaveFileName = "WebAppConfig.json";
        public static string SettingsDirectoryName = "WebApplicationManagerSettings";
        public static string RackConfigDirectoryName = "PlcRackConfigurations";

        // Timeout and interval constants
        public static readonly TimeSpan DeploymentTimeout = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan ContinuousDeploymentInterval = TimeSpan.FromSeconds(5);
        public static readonly int DefaultWindowMargin = 50;
    }
}
