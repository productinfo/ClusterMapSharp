//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// A small class, purely used to easily deserialize the example data.
    /// </summary>
    public class JsonData
    {
        public string Name { get; set; }
        public JsonDataCoordinate Coordinates { get; set; }
    }

    /// <summary>
    /// A small class, purely used to easily deserialize the example data.
    /// </summary>
    public class JsonDataCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// A small class, purely used to easily use the example data.
    /// </summary>
    public class ClusterableAnnotation : MKAnnotation
    {

        protected string Name;
        
        public new string Title { get { return Name; } }

        public override CLLocationCoordinate2D Coordinate { get; set; }

        public ClusterableAnnotation(JsonData data)
        {
            Name = data.Name;
            Coordinate = new CLLocationCoordinate2D(data.Coordinates.Latitude, data.Coordinates.Longitude);
        }

    }

}