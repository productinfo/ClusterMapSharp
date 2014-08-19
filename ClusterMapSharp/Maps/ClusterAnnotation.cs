//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using System;
using System.Collections.Generic;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// A custom annotation that contains the Cluster and its OriginalAnnotations.
    /// </summary>
    public class ClusterAnnotation : MKAnnotation
    {

        private MapCluster _cluster;
        private CLLocationCoordinate2D _coordinate;

        public override string Title
        {
            get { return Cluster != null ? Cluster.Title : string.Empty; }
        }

        public override string Subtitle
        {
            get { return Cluster != null ? Cluster.Subtitle : string.Empty; }
        }

        public override CLLocationCoordinate2D Coordinate
        {
            get { return _coordinate; }
            set
            {
                WillChangeValue("_coordinate");
                _coordinate = value;
                DidChangeValue("_coordinate");
            }
        }

        public ClusterAnnotationType Type { get; set; }
        public bool ShouldBeRemovedAfterAnimation { get; set; }

        public List<MKAnnotation> OriginalAnnotations
        {
            get
            {
                if (Cluster == null) { throw new Exception("This annotation should have a cluster assigned!"); }
                return Cluster.OriginalAnnotations;
            }
        }

        public MapCluster Cluster
        {
            get
            {
                return _cluster;
            }
            set
            {
                WillChangeValue("_title");
                WillChangeValue("_subtitle");
                _cluster = value;
                DidChangeValue("_subtitle");
                DidChangeValue("_title");
            }
        }

        public bool IsOffscreen(CLLocationCoordinate2D coord)
        {
            return (coord.Latitude == MapConstants.Offscreen.Latitude && coord.Longitude == MapConstants.Offscreen.Longitude);
        }

        public ClusterAnnotation()
        {
            _cluster = null;
            Coordinate = MapConstants.Offscreen;
            Type = ClusterAnnotationType.Unknown;
            ShouldBeRemovedAfterAnimation = false;
        }

        public void Reset()
        {
            Cluster = null;
            Coordinate = MapConstants.Offscreen;
        }
        
    }

}