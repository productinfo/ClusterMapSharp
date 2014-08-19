//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// Represents a custom Annotation that defines the mappoint it is at.
    /// </summary>
    public class MapPointAnnotation
    {

        public MKMapPoint MapPoint { get; private set; }
        public MKAnnotation Annotation { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="annotation"></param>
        public MapPointAnnotation(MKAnnotation annotation)
        {
            MapPoint = MKMapPoint.FromCoordinate(annotation.Coordinate);
            Annotation = annotation;
        }

    }

}