// Copyright (c) 2026, Siemens AG
//
// SPDX-License-Identifier: MIT
using System.Windows;

namespace Webserver.Api.Gui.Pages
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
