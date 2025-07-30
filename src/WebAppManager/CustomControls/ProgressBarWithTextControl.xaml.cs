// Copyright (c) 2022, Siemens AG
//
// SPDX-License-Identifier: MIT
using Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Settings;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls
{
    /// <summary>
    /// Interaction logic for ProgressBarWithText.xaml
    /// </summary>
    public partial class ProgressBarWithTextControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue",
                typeof(ProgressBarValue),
                typeof(ProgressBarWithTextControl));

        public event PropertyChangedEventHandler PropertyChanged;

        private ProgressBarValue _progressBarValue
        {
            get;set;
        }

        public ProgressBarValue ProgressBarValue
        {
            get
            {
                return _progressBarValue;
            }
            set
            {
                _progressBarValue = value;
                pbStatus.Value = value?.Value ?? 0;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressBarValue)));
            }
        }
        public ProgressBarWithTextControl()
        {
            InitializeComponent();
        }
    }
}
