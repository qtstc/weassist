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
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace CitySafe
{
    public partial class MapPage : PhoneApplicationPage
    {
        private Color LAST_LOCATION_PUSHPIN_COLOR = Colors.Blue;
        private Color SOS_PUSHPIN_COLOR = Colors.Red;
        private Color MY_LOCATION_PUSHPIN_COLOR = Colors.Black;
        private int ZOOMLEVEL = 10;

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

        /// <summary>
        /// Load location data from the server.
        /// Also intialize the application bar with the loaded data.
        /// </summary>
        private async void LoadUIData()
        {
            App.ShowProgressOverlay(AppResources.Map_LoadingLocation);
            try
            {
                LocationHeader.Text = App.trackItemModel.user.Username + AppResources.Map_LocationOf;

                List<Pushpin> lastLocations =  await LoadLastLocations();
                List<Pushpin> sosLocations = await LoadSOSLocations();
                myLocationPushpin = LoadUserLocation();

                InitAppBar(lastLocations,sosLocations);

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
                MessageBox.Show(AppResources.Map_LoadingFailed);
            }
            App.HideProgressOverlay();
        }

        /// <summary>
        /// Load the SOS locations of the user being tracked.
        /// </summary>
        /// <returns>The list of all the SOS locations</returns>
        private async Task<List<Pushpin>> LoadSOSLocations()
        {
            var sosLocation = from request in ParseObject.GetQuery(ParseContract.SOSRequestTable.TABLE_NAME)
                              where request.Get<ParseUser>(ParseContract.SOSRequestTable.SENDER) == App.trackItemModel.user
                              where request.Get<bool>(ParseContract.SOSRequestTable.RESOLVED) == false
                              select request;
            IEnumerable<ParseObject> results = await sosLocation.FindAsync();
            List<Pushpin> list = new List<Pushpin>();
            for (int i = 0; i < results.Count(); i++)
            {
                ParseObject request = results.ElementAt(i);
                ParseObject location = request.Get<ParseObject>(ParseContract.SOSRequestTable.sentLocation);
                await location.FetchIfNeededAsync();
                list.Add(new Pushpin(ParseContract.LocationTable.ParseObjectToGeoPosition(location), SOS_PUSHPIN_COLOR));
            }
            //Pushpin sos1 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)), SOS_PUSHPIN_COLOR);
            //Pushpin sos2 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)), SOS_PUSHPIN_COLOR);
            //Pushpin sos3 = new Pushpin(new GeoPosition<GeoCoordinate>(new DateTimeOffset(DateTime.MinValue), new GeoCoordinate(12, 13)),SOS_PUSHPIN_COLOR);
            //list.Add(sos1);
            //list.Add(sos2);
            //list.Add(sos3);
            list.Sort((x, y) => x.position.Timestamp.DateTime.CompareTo(y.position.Timestamp.DateTime));
            return list;
        }

        /// <summary>
        /// Get the location of the current user and create a pushpin with it.
        /// </summary>
        /// <returns></returns>
        private Pushpin LoadUserLocation()
        {
            return new Pushpin(Utilities.getCurrentGeoPosition(), MY_LOCATION_PUSHPIN_COLOR);
        }

        /// <summary>
        /// Initialize and load data into lastLocations.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Pushpin>> LoadLastLocations()
        {
            //Initialize the locationList.
            List<Pushpin> Locations = new List<Pushpin>();

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
                    Locations.Add(new Pushpin(gp, LAST_LOCATION_PUSHPIN_COLOR));
                }
            }
            //Sort to make sure the GeoPositions are in sorted order.
            //This is just making sure, the GeoPositions should be sorted already at this point.
            Locations.Sort((x, y) => x.position.Timestamp.DateTime.CompareTo(y.position.Timestamp.DateTime));
            return Locations;
        }

        /// <summary>
        /// Add a pushpin to the map and zoom to the position.
        /// </summary>
        /// <param name="p">the pushpin to be added</param>
        private void AddNewPushpin(Pushpin p)
        {
            LocationMap.SetView(p.position.Location, ZOOMLEVEL, MapAnimationKind.Parabolic);
            LocationMapLayer.Add(p.pushpinLayer);
        }

        /// <summary>
        /// Listener for the menu button. Zoom to the location of the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyLocationMenuButton_Click(object sender, EventArgs e)
        {
            LocationMap.SetView(myLocationPushpin.position.Location, ZOOMLEVEL, MapAnimationKind.Parabolic);
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
                item.Text = "SOS "+p.ToString();
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

        private void LocationMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "be4cc146-3d36-4480-8558-dc73c118ff75";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "ve8Kh91JtSnobnbc6D_d4w";
        }

    }
}