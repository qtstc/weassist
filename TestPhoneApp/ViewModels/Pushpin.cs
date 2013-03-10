using CitySafe.Resources;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public GeoPosition<GeoCoordinate> position { get { return _position; } }
        public MapOverlay pushpinLayer { get { return _pushpinOverlay; } }

        public String distance
        {
            get {
                if (_referencePosition == null)
                    return "";
                return (int)position.Location.GetDistanceTo(_referencePosition.Location) + " m"; }
        }

        public String time
        {
            get { return position.Timestamp.DateTime.ToShortTimeString(); }
        }

        public string type
        {
            get { return _type;}
        }

        public Pushpin(GeoPosition<GeoCoordinate> position, string type, GeoPosition<GeoCoordinate> referencePosition = null)
        {
            _position = position;
            _type = type;
            _pushpinOverlay = GetPushpinOverlay();
            _referencePosition = referencePosition;
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

            TextBox b = new TextBox();
            b.Text = GetLabel();
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);
            b.TextWrapping = TextWrapping.Wrap;
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);
            b.Foreground = new SolidColorBrush(Colors.White);
            b.Background = new SolidColorBrush(c);
            b.IsHitTestVisible = false;
            can.Children.Add(b);
            Canvas.SetTop(b,-90);
            Canvas.SetLeft(b, -25);

            return can;
        }

        public override string ToString()
        {
            string result = "";//"(" + position.Location.Longitude + ", " + position.Location.Latitude+" )";
            result += position.Timestamp.DateTime;
            return result;
        }

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

        public static class TYPE
        {
            public const string MY_LOCATION = "MY_LOCATION";
            public const string KNOWN_SOS_LOCATION = "KNOWN_SOS_LOCATION"; 
            public const string UNKNOWN_SOS_LOCATION = "UNKNOWN_SOS_LOCATION";
            public const string TRACKED_LOCATION = "TRACKED_LOCATION";
        }
    }

}
