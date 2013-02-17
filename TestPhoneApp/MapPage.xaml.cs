using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;

namespace CitySafe
{
    public partial class MapPage : PhoneApplicationPage
    {
        public MapPage()
        {
            InitializeComponent();
            InitializeMap();
        }

        private void InitializeMap()
        {
            LocationMap = new Map();
            LocationMap.Center = new GeoCoordinate(47.6097, -122.3331);
        }
    }
}