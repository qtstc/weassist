﻿#pragma checksum "E:\Projects\Visual Studio Projects\Windows Phone\TestPhoneApp\TestPhoneApp\MapPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "861FBCAA02FDE8F751C461D301249C9E"
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
using Microsoft.Phone.Maps.Controls;
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
    
    
    public partial class MapPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid MapPanel;
        
        internal Microsoft.Phone.Maps.Controls.Map LocationMap;
        
        internal System.Windows.Controls.Slider LocationSlider;
        
        internal System.Windows.Controls.TextBlock LocationInfoTextBlock;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CitySafe;component/MapPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.MapPanel = ((System.Windows.Controls.Grid)(this.FindName("MapPanel")));
            this.LocationMap = ((Microsoft.Phone.Maps.Controls.Map)(this.FindName("LocationMap")));
            this.LocationSlider = ((System.Windows.Controls.Slider)(this.FindName("LocationSlider")));
            this.LocationInfoTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("LocationInfoTextBlock")));
        }
    }
}

