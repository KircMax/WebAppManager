// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings
{
    public class ProgressBarValue : PropertyChangedBase
    {
        private int _value;
        private string _statusText;

        public int Value 
        { 
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public ProgressBarValue(int value, string statusText = "")
        {
            Value = value;
            StatusText = statusText;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(StatusText) 
                ? $"{Value}%" 
                : $"{Value}% - {StatusText}";
        }
    }
}
