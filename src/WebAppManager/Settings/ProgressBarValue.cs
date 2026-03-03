// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls;

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
