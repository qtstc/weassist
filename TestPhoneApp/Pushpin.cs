using Microsoft.Phone.Maps.Controls;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CitySafe
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

        public GeoPosition<GeoCoordinate> position { get { return _position; } }
        public MapOverlay pushpinLayer { get { return _pushpinOverlay; } }

        public Pushpin(GeoPosition<GeoCoordinate> position, Color c, string text)
        {
            _position = position;
            _pushpinOverlay = GetPushpinOverlay(position, c,text);
        }

        private static MapOverlay GetPushpinOverlay(GeoPosition<GeoCoordinate> p, Color c,string text)
        {
            var aPushpin = CreatePushpinObject(c,text);

            //Creating a MapOverlay and adding the Pushpin to it.
            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = aPushpin;
            MyOverlay.GeoCoordinate = p.Location;
            MyOverlay.PositionOrigin = new Point(0, 0);
            return MyOverlay;
        }

        private static Canvas CreatePushpinObject(Color c, string text)
        {

            Canvas can = new Canvas();
            //Ellipse circle = new Ellipse();
            //circle.Height = 20;
            //circle.Width = 20;
            //circle.Fill = new SolidColorBrush(c);
            //circle.SetValue(Grid.RowProperty, 1);
            //circle.SetValue(Grid.ColumnProperty, 0);
            //can.Children.Add(circle);
            //Canvas.SetLeft(circle, -10);
            //Canvas.SetTop(circle, -10);

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
            b.Text = text;
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
    }

}
