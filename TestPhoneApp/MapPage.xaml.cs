using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using Parse;
using ScheduledLocationAgent.Data;
using System.Diagnostics;
using CitySafe.Resources;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using CitySafe.ViewModels;

namespace CitySafe
{
    /// <summary>
    /// The page that is used to display map data.
    /// Currently it uses the application bar to show a 
    /// list of locations. In the future, a panaroma page with
    /// LongListSelector will suit the need better.
    /// </summary>
    public partial class MapPage : PhoneApplicationPage
    {
        //The maximum number of last locations loaded. We set a value because the app bar can have at most 50 items.
        //No point loading more.
        private int LOCATION_SIZE_LIMIT = 40;

        private MapLayer LocationMapLayer;
        private Pushpin lastPushpin;//Used to store the last Pushpin on Map.

        private Pushpin myLocationPushpin;

        public MapPage()
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
            CancellationToken tk = App.ShowProgressOverlay(AppResources.Map_LoadingLocation);
            string message = "";
            try
            {
                LocationHeader.Text = App.trackItemModel.user.Get<string>(ParseContract.UserTable.FIRST_NAME) + AppResources.Map_LocationHeader;

                List<Pushpin> lastLocations = await LoadLastLocations(tk);
                List<Pushpin> sosLocations = await LoadSOSLocations(tk);
                myLocationPushpin = await LoadUserLocation();
                if (myLocationPushpin == null)
                    MessageBox.Show(AppResources.Map_CannotObtainLocation);

                InitAppBar(lastLocations, sosLocations);

                //Add the user's location to the maplayer.
                LocationMapLayer.Add(myLocationPushpin.pushpinLayer);
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
        /// Load the SOS locations of the user being tracked.
        /// </summary>
        /// <returns>The list of all the SOS locations</returns>
        private async Task<List<Pushpin>> LoadSOSLocations(CancellationToken tk)
        {
            var sosLocation = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                              where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == App.trackItemModel.user
                              where request.Get<bool>(ParseContract.SOSRequestTable.RESOLVED) == false
                              select request;
            IEnumerable<ParseObject> results = await sosLocation.FindAsync(tk);
            List<Pushpin> list = new List<Pushpin>();
            for (int i = 0; i < results.Count(); i++)
            {
                ParseObject request = results.ElementAt(i);
                ParseObject location = request.Get<ParseObject>(ParseContract.SOSRequestTable.SENT_LOCATION);
                await location.FetchIfNeededAsync(tk);
                GeoPosition<GeoCoordinate> l = ParseContract.LocationTable.ParseObjectToGeoPosition(location);
                list.Add(new Pushpin(l, Pushpin.TYPE.KNOWN_SOS_LOCATION));
            }
            //Pushpin sos1 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)), SOS_PUSHPIN_COLOR);
            //Pushpin sos2 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)), SOS_PUSHPIN_COLOR);
            //Pushpin sos3 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)),SOS_PUSHPIN_COLOR);
            //list.Add(sos1);
            //list.Add(sos2);
            //list.Add(sos3);
            list.Sort((x, y) => y.position.Timestamp.DateTime.CompareTo(x.position.Timestamp.DateTime));
            return list;
        }

        /// <summary>
        /// Get the location of the current user and create a pushpin with it.
        /// </summary>
        /// <returns></returns>
        private async Task<Pushpin> LoadUserLocation()
        {
            GeoPosition<GeoCoordinate> p = await Utilities.getCurrentGeoPosition();
            if(p == null)
                return null;
            return new Pushpin(p,Pushpin.TYPE.MY_LOCATION);
        }

        /// <summary>
        /// Initialize and load data into lastLocations.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Pushpin>> LoadLastLocations(CancellationToken tk)
        {
            //Initialize the locationList.
            List<Pushpin> Locations = new List<Pushpin>();

            //Get the position of the first location data(oldest one).
            int locationSize = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LOCATION_DATA_SIZE);
            int startPoint = App.trackItemModel.user.Get<int>(ParseContract.UserTable.LAST_LOCATION_INDEX);

            for (int i = 0; i < locationSize; i++)
            {
                if (Locations.Count == LOCATION_SIZE_LIMIT)
                    break;
                int current = startPoint - i;
                if (current < 0)
                    current += locationSize;

                if (App.trackItemModel.user.ContainsKey(ParseContract.UserTable.LOCATION(current)))
                {
                    ParseObject location = App.trackItemModel.user.Get<ParseObject>(ParseContract.UserTable.LOCATION(current));
                    await location.FetchIfNeededAsync(tk);
                    //Debug.WriteLine("line " + i + " : " + location.ObjectId + " " + location.Get<DateTime>(ParseContract.LocationTable.TIME_STAMP));
                    GeoPosition<GeoCoordinate> gp = ParseContract.LocationTable.ParseObjectToGeoPosition(location);
                    Locations.Add(new Pushpin(gp, Pushpin.TYPE.TRACKED_LOCATION));
                }
                Debug.WriteLine(Locations.Count);
            }
            //Sort to make sure the GeoPositions are in sorted order.
            //This is just making sure, the GeoPositions should be sorted already at this point.
            Locations.Sort((x, y) => y.position.Timestamp.DateTime.CompareTo(x.position.Timestamp.DateTime));
            return Locations;
        }

        #endregion

        #region app bar setup
        /// <summary>
        /// Listener for the menu button. Zoom to the location of the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyLocationMenuButton_Click(object sender, EventArgs e)
        {
            if(myLocationPushpin != null)
            LocationMap.SetView(myLocationPushpin.position.Location, LocationMap.ZoomLevel, MapAnimationKind.Parabolic);
        }

        /// <summary>
        /// Listener for the menu items. 
        /// Remove old pushpin and add a new one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void LocationMenuItem_Click(object sender, EventArgs e, Pushpin p)
        {
            LocationMapLayer.Remove(lastPushpin.pushpinLayer);
            AddNewPushpin(p);
            lastPushpin = p;
        }

        /// <summary>
        /// Initialize the application bar with
        /// the list of last locations and sos locations.
        /// </summary>
        /// <param name="lastLocations"></param>
        /// <param name="sosLocations"></param>
        private void InitAppBar(List<Pushpin> lastLocations, List<Pushpin> sosLocations)
        {
            ApplicationBar appBar = new ApplicationBar();

            ApplicationBarIconButton MyLocationMenuButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/MyLocation.png", UriKind.Relative));
            MyLocationMenuButton.Click += new EventHandler(MyLocationMenuButton_Click);
            MyLocationMenuButton.Text = AppResources.Map_MyLocation;
            appBar.Buttons.Add(MyLocationMenuButton);

            //Add the list of sos locations to the application bar.
            for (int i = 0; i < sosLocations.Count; i++)
            {
                ApplicationBarMenuItem item = new ApplicationBarMenuItem();
                Pushpin p = sosLocations.ElementAt<Pushpin>(i);
                item.Text = p.ToString() + " (SOS)";
                appBar.MenuItems.Add(item);
                item.Click += (sender, e) => LocationMenuItem_Click(sender, e, p);
            }

            //Add the list of last locations to the application bar.
            for (int i = 0; i < lastLocations.Count; i++)
            {
                ApplicationBarMenuItem item = new ApplicationBarMenuItem();
                Pushpin p = lastLocations.ElementAt<Pushpin>(i);
                item.Text = p.ToString();
                appBar.MenuItems.Add(item);
                item.Click += (sender, e) => LocationMenuItem_Click(sender, e, p);
            }

            ApplicationBar = appBar;
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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.HideProgressOverlay();
        }
    }
}