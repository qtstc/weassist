﻿#pragma checksum "E:\Projects\Visual Studio Projects\Windows Phone\TestPhoneApp\TestPhoneApp\TrackingSettingsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C0788670525BCFD2A01318A2027416A9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
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


namespace CitySafe {
    
    
    public partial class TrackingSettingsPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBlock TrackingTitle;
        
        internal System.Windows.Controls.StackPanel TrackingSettingsPanel;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CitySafe;component/TrackingSettingsPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TrackingTitle = ((System.Windows.Controls.TextBlock)(this.FindName("TrackingTitle")));
            this.TrackingSettingsPanel = ((System.Windows.Controls.StackPanel)(this.FindName("TrackingSettingsPanel")));
        }
    }
}

