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
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using CitySafe.Resources;

namespace CitySafe
{
    public partial class MapPage : PhoneApplicationPage
    {
        private List<GeoPosition<GeoCoordinate>> locationList;

        public MapPage()
        {
            InitializeComponent();
            LoadUIData();
        }

        private async void LoadUIData()
        {
            App.ShowProgressOverlay(AppResources.Map_LoadingLocation);
            try
            {
                //Initialize the locationList.
                locationList = new List<GeoPosition<GeoCoordinate>>();

                //Get the position of the first location data(oldest one).
                int locationSize = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
                int startPoint = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);
                startPoint++;

                for (int i = 0; i < locationSize; i++)
                {
                    ParseObject location = App.trackItemModel.user.Get<ParseObject>(ParseContract.UserTable.LOCATION((i + startPoint) % locationSize));
                    if (location.ObjectId != ParseContract.LocationTable.DUMMY_LOCATION)
                    {
                        await location.FetchIfNeededAsync();
                        //Debug.WriteLine("line " + i + " : " + location.ObjectId + " " + location.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP));
                        GeoPosition<GeoCoordinate> gp = ParseContract.LocationTable.ParseObjectToGeoPosition(location);
                        locationList.Add(gp);
                    }
                }
                //Sort to make sure the GeoPositions are in sorted order.
                //This is just making sure, the GeoPositions should be sorted already at this point.
                locationList.Sort((x, y) => x.Timestamp.DateTime.CompareTo(y.Timestamp.DateTime));
                LocationSlider.Maximum = locationList.Count - 1;
                LocationHeader.Text = App.trackItemModel.user.Username+AppResources.Map_LocationOf;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                MessageBox.Show(AppResources.Map_LoadingFailed);
            }
            App.HideProgressOverlay();
        }

        private void LocationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GeoPosition<GeoCoordinate> newLocation = locationList.ElementAt((Int32)e.NewValue);
            LocationInfoTextBlock.Text = newLocation.Location.ToString();
            //LocationMap.Center = newLocation.Location;
            LocationMap.SetView(newLocation.Location, 10, MapAnimationKind.Parabolic);
        }

        private void LocationMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "be4cc146-3d36-4480-8558-dc73c118ff75";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "ve8Kh91JtSnobnbc6D_d4w";
        }
    }
}