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
using CitySafe.ViewModels;
using System.Threading;
using CitySafe.Resources;
using System.Diagnostics;
using Parse;
using System.Threading.Tasks;
using System.Device.Location;
using ScheduledLocationAgent.Data;
using System.ComponentModel;

namespace CitySafe
{
    public partial class AreaMapPage : PhoneApplicationPage
    {
        public const double areaRadius = 1;

        private const string MODE_KEY = "mode_areamap";
        private const string RESOLVED_REQUESTS = "resolved";
        private const string UNRESOLVED_REQUESTS = "unresolved";

        private MapLayer LocationMapLayer;
        private Pushpin lastPushpin;//Used to store the last Pushpin on Map.

        private Pushpin myLocationPushpin;

        public AreaMapPage()
        {
            InitializeComponent();
            LoadUIData();
            //Creating a MapLayer and adding the MapOverlay to it
            LocationMapLayer = new MapLayer();
            LocationMap.Layers.Add(LocationMapLayer);
        }

        #region data loading helper method.
        /// <summary>
        /// Load location data from the server.
        /// Also intialize the application bar with the loaded data.
        /// </summary>
        private async void LoadUIData()
        {
            ApplicationBar.IsVisible = false;
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Map_LoadingLocation);
            string message = "";
            try
            {
                myLocationPushpin = await LoadUserLocation();
                if (myLocationPushpin == null)
                    MessageBox.Show(AppResources.Map_CannotObtainLocation);
                else
                    LocationMapLayer.Add(myLocationPushpin.pushpinLayer);//Add the user's location to the maplayer.
                LocationList<Pushpin> sosLocations = await LoadSOSLocations(tk, myLocationPushpin.position);

                List<LocationList<Pushpin>> longList = ViewModels.LocationList<Pushpin>.GroupByTime(sosLocations,24);

                LocationList.ItemsSource = longList;

                //Add the last location to the maplayer.

                if (sosLocations.Count > 0)
                {
                    AddNewPushpin(sosLocations.First());
                    lastPushpin = sosLocations.First();
                }
                ApplicationBar.IsVisible = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                message = AppResources.Map_LoadingFailed;
                NavigationService.GoBack();
            }
            App.HideProgressOverlay();
            if (!message.Equals(""))
                MessageBox.Show(message);
        }


        /// <summary>
        /// Load sos locations, either loads the past ones or the unresolved ones depending on the
        /// parameters passed to this page.
        /// </summary>
        /// <param name="tk"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        private async Task<LocationList<Pushpin>> LoadSOSLocations(CancellationToken tk, GeoPosition<GeoCoordinate> reference)
        {
            bool isResolved = NavigationContext.QueryString[MODE_KEY].Equals(RESOLVED_REQUESTS); 

            ParseGeoPoint userL = new ParseGeoPoint(reference.Location.Latitude, reference.Location.Longitude);

            var nearLocations = ParseObject.GetQuery(ParseContract.LocationTable.TABLE_NAME).WhereWithinDistance(ParseContract.LocationTable.LOCATION, userL, ParseGeoDistance.FromKilometers(areaRadius));

            var sosLocation = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                              join location in nearLocations on request[ParseContract.SOSRequestTable.SENT_LOCATION] equals location
                              select request;

            var result = from request in sosLocation 
                         where request.Get<bool>(ParseContract.SOSRequestTable.RESOLVED) == isResolved
                         where request.Get<bool>(ParseContract.SOSRequestTable.SHARE_REQUEST) == true
                         select request;

            IEnumerable<ParseObject> results = await result.FindAsync(tk);

            LocationList<Pushpin> list = new LocationList<Pushpin>(AppResources.Map_SOSLocation);
            for (int i = 0; i < results.Count(); i++)
            {
                ParseObject request = results.ElementAt(i);
                ParseObject location = request.Get<ParseObject>(ParseContract.SOSRequestTable.SENT_LOCATION);
                await location.FetchIfNeededAsync(tk);
                GeoPosition<GeoCoordinate> l = ParseContract.LocationTable.ParseObjectToGeoPosition(location);
                list.Add(new Pushpin(l, Pushpin.TYPE.KNOWN_SOS_LOCATION, reference, (sender, s) => SOSPushpin_Click(sender, s, request)));
            }
            list.Sort((x, y) => y.position.Timestamp.DateTime.CompareTo(x.position.Timestamp.DateTime));
            return list;

        }

        /// <summary>
        /// The event handler for the pushpin. Because we cannot do navigation in the pushpin class,
        /// we pass an event handler to it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="relation"></param>
        private void SOSPushpin_Click(object sender, RoutedEventArgs e, ParseObject relation)
        {
            App.sosRequestInfo = relation;
            NavigationService.Navigate(SOSInfoPage.GetPulicInfoUri());
            //NavigationService.Navigate(new Uri("SOSInfoPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Get the location of the current user and create a pushpin with it.
        /// </summary>
        /// <returns></returns>
        private async Task<Pushpin> LoadUserLocation()
        {
            GeoPosition<GeoCoordinate> p = await Utilities.getCurrentGeoPosition();
            if (p == null)
                return null;
            return new Pushpin(p, Pushpin.TYPE.MY_LOCATION);
        }
        #endregion


        /// <summary>
        /// Add a pushpin to the map and zoom to the position.
        /// </summary>
        /// <param name="p">the pushpin to be added</param>
        private void AddNewPushpin(Pushpin p)
        {
            LocationMap.SetView(p.position.Location, LocationMap.ZoomLevel, MapAnimationKind.Parabolic);
            LocationMapLayer.Add(p.pushpinLayer);
        }

        private void LocationMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "be4cc146-3d36-4480-8558-dc73c118ff75";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "ve8Kh91JtSnobnbc6D_d4w";
        }

        private void LocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            if (LocationList.SelectedItem == null)
                return;

            MapPivot.SelectedIndex = 1;

            Pushpin p = LocationList.SelectedItem as Pushpin;

            LocationMapLayer.Remove(lastPushpin.pushpinLayer);
            AddNewPushpin(p);
            lastPushpin = p;

            // Reset selected item to null (no selection)
            LocationList.SelectedItem = null;

        }

        /// <summary>
        /// Listener for the menu button. Zoom to the location of the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyLocationMenuButton_Click(object sender, EventArgs e)
        {
            MapPivot.SelectedIndex = 1;
            if (myLocationPushpin != null)
                LocationMap.SetView(myLocationPushpin.position.Location, LocationMap.ZoomLevel, MapAnimationKind.Parabolic);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.HideProgressOverlay())
                e.Cancel = true;
        }

        public static Uri GetResolvedUri()
        {
            return new Uri("/AreaMapPage.xaml?" + MODE_KEY + "=" + RESOLVED_REQUESTS, UriKind.Relative);
        }

        public static Uri GetUnresolvedUri()
        {
            return new Uri("/AreaMapPage.xaml?" + MODE_KEY + "=" + UNRESOLVED_REQUESTS, UriKind.Relative);
        }
    }
}