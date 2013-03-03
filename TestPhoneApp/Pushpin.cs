using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
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

        public Pushpin(GeoPosition<GeoCoordinate> position, Color c)
        {
            _position = position;
            _pushpinOverlay = GetPushpinOverlay(position.Location, c);
        }

        private static MapOverlay GetPushpinOverlay(GeoCoordinate coord, Color c)
        {
            var aPushpin = CreatePushpinObject(c);

            //Creating a MapOverlay and adding the Pushpin to it.
            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = aPushpin;
            MyOverlay.GeoCoordinate = coord;
            MyOverlay.PositionOrigin = new Point(0, 0.5);
            return MyOverlay;
        }

        private static Grid CreatePushpinObject(Color c)
        {
            //Creating a Grid element.
            Grid MyGrid = new Grid();
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.Background = new SolidColorBrush(Colors.Transparent);

            //Creating a Rectangle
            Rectangle MyRectangle = new Rectangle();
            MyRectangle.Fill = new SolidColorBrush(c);
            MyRectangle.Height = 20;
            MyRectangle.Width = 20;
            MyRectangle.SetValue(Grid.RowProperty, 0);
            MyRectangle.SetValue(Grid.ColumnProperty, 0);

            //Adding the Rectangle to the Grid
            MyGrid.Children.Add(MyRectangle);

            //Creating a Polygon
            Polygon MyPolygon = new Polygon();
            MyPolygon.Points.Add(new Point(2, 0));
            MyPolygon.Points.Add(new Point(22, 0));
            MyPolygon.Points.Add(new Point(2, 40));
            MyPolygon.Stroke = new SolidColorBrush(c);
            MyPolygon.Fill = new SolidColorBrush(c);
            MyPolygon.SetValue(Grid.RowProperty, 1);
            MyPolygon.SetValue(Grid.ColumnProperty, 0);

            //Adding the Polygon to the Grid
            MyGrid.Children.Add(MyPolygon);
            return MyGrid;
        }

        public override string ToString()
        {
            string result = "";//"(" + position.Location.Longitude + ", " + position.Location.Latitude+" )";
            result += position.Timestamp.DateTime;
            return result;
        }
    }

}
