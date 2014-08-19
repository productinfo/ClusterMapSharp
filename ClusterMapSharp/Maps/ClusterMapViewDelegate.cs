//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// Represents a delegate that handles some basic MapView things.
    /// </summary>
    public class ClusterMapViewDelegate : MKMapViewDelegate
    {

        /// <summary>
        /// Handles when our region has changed.
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="animated"></param>
        public override void RegionChanged(MKMapView mapView, bool animated)
        {
            // TODO: Casts really needed or is there a better way?
            if (((ClusterMapView)mapView).IsAnimatingClusters)
            {
                ((ClusterMapView)mapView).ShouldComputeClusters = true;
            }
            else
            {
                ((ClusterMapView)mapView).IsAnimatingClusters = true;
                ((ClusterMapView)mapView).ClusterInMapRect(mapView.VisibleMapRect);
            }

            if (mapView.SelectedAnnotations != null)
            {
                foreach (var annotation in mapView.SelectedAnnotations)
                {
                    mapView.DeselectAnnotation(annotation, true);
                }
            }
        }

        /// <summary>
        /// Retrieves a view for our annotations.
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, NSObject annotation)
        {
            if (annotation.GetType() != typeof (ClusterAnnotation))
            {
                return MapViewViewForAnnotation(mapView, annotation);
            }

            if (((ClusterAnnotation) annotation).Type == ClusterAnnotationType.Leaf)
            {
                return MapViewViewForAnnotation(mapView, annotation);
            }
                
            if (((ClusterAnnotation) annotation).Type == ClusterAnnotationType.Cluster)
            {
                return MapViewViewForClusterAnnotation(mapView, annotation);
            }

            return null;
        }

        /// <summary>
        /// Handles creating a view for a clustered annotation.
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        private MKAnnotationView MapViewViewForClusterAnnotation(MKMapView mapView, NSObject annotation)
        {
            var pinView = (MKAnnotationView)mapView.DequeueReusableAnnotation("ADClusterableAnnotation");
            
            if (pinView == null)
            {
                pinView = new MKAnnotationView(annotation, "ADClusterableAnnotation");
                pinView.CanShowCallout = true;
            }
            else
            {
                pinView.Annotation = annotation;
            }

            return pinView;
        }


        /// <summary>
        /// Handles creating a view for a single annotation.
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        private MKAnnotationView MapViewViewForAnnotation(MKMapView mapView, NSObject annotation)
        {
            var pinView = (MKAnnotationView)mapView.DequeueReusableAnnotation("ADMapCluster");
            if (pinView == null)
            {
                pinView = new MKAnnotationView(annotation, "ADMapCluster");
                pinView.CanShowCallout = true;
            }
            else
            {
                pinView.Annotation = annotation;
            }

            return pinView;
        }

    }

}