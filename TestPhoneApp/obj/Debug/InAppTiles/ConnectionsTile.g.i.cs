﻿#pragma checksum "E:\Projects\Visual Studio Projects\Windows Phone\TestPhoneApp\TestPhoneApp\InAppTiles\ConnectionsTile.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "781B643284EC173DBE1CBF1EEC137012"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
    
    
    public partial class ConnectionsTile : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard liveTileAnim1;
        
        internal System.Windows.Media.Animation.Storyboard liveTileAnim1_Inverse;
        
        internal System.Windows.Controls.Grid grid1;
        
        internal System.Windows.Media.TranslateTransform panel1;
        
        internal System.Windows.Controls.Image img1;
        
        internal System.Windows.Controls.TextBlock txt1;
        
        internal System.Windows.Controls.Grid grid2;
        
        internal System.Windows.Media.TranslateTransform panel2;
        
        internal System.Windows.Controls.TextBlock txt2;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/CitySafe;component/InAppTiles/ConnectionsTile.xaml", System.UriKind.Relative));
            this.liveTileAnim1 = ((System.Windows.Media.Animation.Storyboard)(this.FindName("liveTileAnim1")));
            this.liveTileAnim1_Inverse = ((System.Windows.Media.Animation.Storyboard)(this.FindName("liveTileAnim1_Inverse")));
            this.grid1 = ((System.Windows.Controls.Grid)(this.FindName("grid1")));
            this.panel1 = ((System.Windows.Media.TranslateTransform)(this.FindName("panel1")));
            this.img1 = ((System.Windows.Controls.Image)(this.FindName("img1")));
            this.txt1 = ((System.Windows.Controls.TextBlock)(this.FindName("txt1")));
            this.grid2 = ((System.Windows.Controls.Grid)(this.FindName("grid2")));
            this.panel2 = ((System.Windows.Media.TranslateTransform)(this.FindName("panel2")));
            this.txt2 = ((System.Windows.Controls.TextBlock)(this.FindName("txt2")));
        }
    }
}

