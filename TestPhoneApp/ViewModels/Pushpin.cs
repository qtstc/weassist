using CitySafe.Resources;
using Microsoft.Phone.Maps.Controls;
using Parse;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;

namespace CitySafe.ViewModels
{
    /// <summary>
    /// This class is used to store a MapOverlay instance for pushpin and
    /// a instance of the GeoPosition class.
    /// It is supposed to inherit from the MapOverlay class, but we could not
    /// do that because it is sealed.
    /// </summary>
    public class Pushpin
    {
        private GeoPosition<GeoCoordinate> _position;
        private MapOverlay _pushpinOverlay;
        private string _type;
        private GeoPosition<GeoCoordinate> _referencePosition;
        private RoutedEventHandler _pushpinEvent;

        public GeoPosition<GeoCoordinate> position { get { return _position; } }
        public MapOverlay pushpinLayer { get { return _pushpinOverlay; } }

        public String distance
        {
            get {
                if (_referencePosition == null)
                    return "";
                int meter = (int)position.Location.GetDistanceTo(_referencePosition.Location);
                double miles = meter * 0.000621371;
                if (miles > 0.25)
                {
                    string s = miles.ToString("#.##");
                    if (s.StartsWith("."))
                        s = "0" + s;
                    return s + " miles away";
                }
                else
                    return (int)(meter * 3.28) + " feet away";
            }
        }

        public String time
        {
            get { return position.Timestamp.DateTime.ToShortTimeString(); }
        }

        public string type
        {
            get { return _type;}
        }

        public Pushpin(GeoPosition<GeoCoordinate> position, string type, GeoPosition<GeoCoordinate> referencePosition = null, RoutedEventHandler pushpinEvent = null)
        {
            _referencePosition = referencePosition;
            _pushpinEvent = pushpinEvent;
            _position = position;
            _type = type;
            _pushpinOverlay = GetPushpinOverlay();
        }

        private MapOverlay GetPushpinOverlay()
        {
            var aPushpin = CreatePushpinObject();
            //Creating a MapOverlay and adding the Pushpin to it.
            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = aPushpin;
            MyOverlay.GeoCoordinate = position.Location;
            MyOverlay.PositionOrigin = new Point(0, 0);
            return MyOverlay;
        }

        private Canvas CreatePushpinObject()
        {
            Color c = GetColor();
            Canvas can = new Canvas();

            //Special pushpin for my location.
            if (_type.Equals(TYPE.MY_LOCATION))
            {
                Ellipse circle = new Ellipse();
                circle.Height = 20;
                circle.Width = 20;
                circle.Fill = new SolidColorBrush(c);
                circle.SetValue(Grid.RowProperty, 1);
                circle.SetValue(Grid.ColumnProperty, 0);
                can.Children.Add(circle);
                Canvas.SetLeft(circle, -10);
                Canvas.SetTop(circle, -10);
                return can;
            }

            Polygon MyPolygon = new Polygon();
            MyPolygon.Points.Add(new Point(0, 0));
            MyPolygon.Points.Add(new Point(20, 0));
            MyPolygon.Points.Add(new Point(0, 40));
            MyPolygon.Stroke = new SolidColorBrush(c);
            MyPolygon.Fill = new SolidColorBrush(c);
            can.Children.Add(MyPolygon);
            Canvas.SetLeft(MyPolygon, 0);
            Canvas.SetTop(MyPolygon, -40);

            Button b = new Button();
            b.Content = GetLabel();
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);
            b.Foreground = new SolidColorBrush(Colors.White);
            b.Background = new SolidColorBrush(c);
            can.Children.Add(b);
            Canvas.SetTop(b, -90);
            Canvas.SetLeft(b, -25);

            if (_pushpinEvent != null)
            {
                Debug.WriteLine("added");
                b.Click += _pushpinEvent;
            }
            //b.Click += (sender, s) => SOSPushpin_Click(sender, s);

            return can;
        }


        private void SOSPushpin_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("triggered in pushpin!");
        }


        /// <summary>
        /// Not really used. Mostly for debugging purpose.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";//"(" + position.Location.Longitude + ", " + position.Location.Latitude+" )";
            result += position.Timestamp.DateTime;
            return result;
        }

        /// <summary>
        /// Defines the label of different types of push pins.
        /// </summary>
        /// <returns></returns>
        private string GetLabel()
        {
            switch (_type)
            {
                case TYPE.MY_LOCATION:
                    return AppResources.Map_MyLocation;
                case TYPE.KNOWN_SOS_LOCATION:
                    return "SOS @ " + position.Timestamp.DateTime.ToShortTimeString() +" "+ position.Timestamp.DateTime.ToShortDateString();
                case TYPE.UNKNOWN_SOS_LOCATION:
                    return "SOS @ " + position.Timestamp.DateTime.ToShortTimeString() +" "+ position.Timestamp.DateTime.ToShortDateString();
                case TYPE.TRACKED_LOCATION:
                    return position.Timestamp.DateTime.ToShortTimeString() +" "+ position.Timestamp.DateTime.ToShortDateString();
                default:
                    return "";
            }
        }

        /// <summary>
        /// Defines the color of different types of pushpins.
        /// </summary>
        /// <returns></returns>
        private Color GetColor()
        {
            switch (_type)
            {
                case TYPE.MY_LOCATION:
                    return Colors.Black;
                case TYPE.KNOWN_SOS_LOCATION:
                    return Colors.Red;
                case TYPE.UNKNOWN_SOS_LOCATION:
                    return Colors.Red;
                case TYPE.TRACKED_LOCATION:
                    return Colors.Blue;
                default:
                    return Colors.Green;
            }
        }

        /// <summary>
        /// A class that store the info used to identify different types of Pushpins.
        /// </summary>
        public static class TYPE
        {
            public const string MY_LOCATION = "MY_LOCATION";
            public const string KNOWN_SOS_LOCATION = "KNOWN_SOS_LOCATION"; 
            public const string UNKNOWN_SOS_LOCATION = "UNKNOWN_SOS_LOCATION";
            public const string TRACKED_LOCATION = "TRACKED_LOCATION";
        }
    }

}
