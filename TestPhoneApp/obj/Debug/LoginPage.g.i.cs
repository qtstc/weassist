﻿#pragma checksum "E:\Projects\Visual Studio Projects\Windows Phone\TestPhoneApp\TestPhoneApp\LoginPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "54A9F91FA49DC384F8B5139D8980B313"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18010
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace TestPhoneApp {
    
    
    public partial class LoginPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel LoginPanel;
        
        internal System.Windows.Controls.TextBox login_username_textbox;
        
        internal System.Windows.Controls.PasswordBox login_password_textbox;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/TestPhoneApp;component/LoginPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.LoginPanel = ((System.Windows.Controls.StackPanel)(this.FindName("LoginPanel")));
            this.login_username_textbox = ((System.Windows.Controls.TextBox)(this.FindName("login_username_textbox")));
            this.login_password_textbox = ((System.Windows.Controls.PasswordBox)(this.FindName("login_password_textbox")));
        }
    }
}

