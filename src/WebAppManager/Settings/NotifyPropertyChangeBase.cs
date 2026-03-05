// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.ComponentModel;

namespace Webserver.Api.Gui.Settings
{
    public class NotifyPropertyChangeBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
