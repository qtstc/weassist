using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using CitySafe.Resources;
using Microsoft.Phone.Maps.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using Parse;
using ScheduledLocationAgent.Data;
using System.Device.Location;
using CitySafe.ViewModels;
using System.ComponentModel;

namespace CitySafe
{
    public partial class TrackMapPage : PhoneApplicationPage
    {
        private MapLayer LocationMapLayer;
        private Pushpin lastPushpin;//Used to store the last Pushpin on Map.

        private Pushpin myLocationPushpin;
        private bool loaded;

        public TrackMapPage()
        {
            loaded = false;
            InitializeComponent();
            //Creating a MapLayer and adding the MapOverlay to it
            LocationMapLayer = new MapLayer();
            LocationMap.Layers.Add(LocationMapLayer);
            LocationMap.ZoomLevel = 15;
        }

        #region data loading helper method.
        /// <summary>
        /// Load location data from the server.
        /// Also intialize the application bar with the loaded data.
        /// </summary>
        private async Task LoadUIData()
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
                LocationList<Pushpin> lastLocations = await LoadLastLocations(tk, myLocationPushpin.position);
                LocationList<Pushpin> sosLocations = await LoadSOSLocations(tk, myLocationPushpin.position);

                List<LocationList<Pushpin>> longList = new List<LocationList<Pushpin>>();
                longList.Add(sosLocations);
                longList.AddRange(ViewModels.LocationList<Pushpin>.GroupByTime(lastLocations, 24));

                LocationList.ItemsSource = longList;

                //Add the last location to the maplayer.
                if (lastLocations.Count > 0)
                {
                    AddNewPushpin(lastLocations.First());
                    lastPushpin = lastLocations.First();
                }
                else if (sosLocations.Count > 0)
                {
                    AddNewPushpin(sosLocations.First());
                    lastPushpin = sosLocations.First();
                }
                ApplicationBar.IsVisible = true;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("loading canceled");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                message = AppResources.Map_LoadingFailed;
            }
            App.HideProgressOverlay();
            if (!message.Equals(""))
                MessageBox.Show(message);
        }

        /// <summary>
        /// Load the SOS locations of the user being tracked.
        /// </summary>
        /// <returns>The list of all the SOS locations</returns>
        private async Task<LocationList<Pushpin>> LoadSOSLocations(CancellationToken tk, GeoPosition<GeoCoordinate> reference)
        {
            var sosLocation = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME).Include(ParseContract.SOSRequestTable.SENT_LOCATION)
                              where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == App.trackItemModel.user
                              where request.Get<bool>(ParseContract.SOSRequestTable.RESOLVED) == false
                              select request;

            IEnumerable<ParseObject> results = await sosLocation.FindAsync(tk);
            LocationList<Pushpin> list = new LocationList<Pushpin>(AppResources.Map_SOSLocation);
            for (int i = 0; i < results.Count(); i++)
            {
                ParseObject request = results.ElementAt(i);
                ParseObject location = request.Get<ParseObject>(ParseContract.SOSRequestTable.SENT_LOCATION);
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
            NavigationService.Navigate(SOSInfoPage.GetPrivateInfoUri());
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

        /// <summary>
        /// Initialize and load data into lastLocations.
        /// </summary>
        /// <returns></returns>
        private async Task<LocationList<Pushpin>> LoadLastLocations(CancellationToken tk, GeoPosition<GeoCoordinate> reference)
        {
            //Initialize the locationList.
            LocationList<Pushpin> Locations = new LocationList<Pushpin>(Pushpin.TYPE.TRACKED_LOCATION);

            //Get the position of the first location data(oldest one).
            int locationSize = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
            int startPoint = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);

            for (int i = 0; i < locationSize; i++)
            {
                //if (Locations.Count == LOCATION_SIZE_LIMIT)
                //    break;
                int current = startPoint - i;
                if (current < 0)
                    current += locationSize;

                if (App.trackItemModel.user.ContainsKey(ParseContract.UserTable.LOCATION(current)))
                {
                    ParseObject location = App.trackItemModel.user.Get<ParseObject>(ParseContract.UserTable.LOCATION(current));
                    await location.FetchIfNeededAsync(tk);
                    //Debug.WriteLine("line " + i + " : " + location.ObjectId + " " + location.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP));
                    GeoPosition<GeoCoordinate> gp = ParseContract.LocationTable.ParseObjectToGeoPosition(location);
                    Locations.Add(new Pushpin(gp, Pushpin.TYPE.TRACKED_LOCATION, reference));
                }
            }
            //Sort to make sure the GeoPositions are in sorted order.
            //This is just making sure, the GeoPositions should be sorted already at this point.
            Locations.Sort((x, y) => y.position.Timestamp.DateTime.CompareTo(x.position.Timestamp.DateTime));
            return Locations;
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
            App.HideProgressOverlay();
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!loaded)
            {
                await LoadUIData();
                loaded = true;
            }
        }
    }
}