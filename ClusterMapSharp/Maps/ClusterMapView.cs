//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// Represents the actual MapView.
    /// </summary>
    public class ClusterMapView : MKMapView
    {
        private MapCluster _rootMapCluster;
        private List<ClusterAnnotation> _singleAnnotationsPool;
        private List<ClusterAnnotation> _clusterAnnotationsPool;
        private bool _isSettingAnnotations;
        private List<MKAnnotation> _annotationsToBeSet;
        private List<MKAnnotation> _originalAnnotations;
        private List<ClusterAnnotation> _clusterAnnotations;

        public bool IsAnimatingClusters { get; set; }
        public bool ShouldComputeClusters { get; set; }

        public ClusterMapView()
        {
            // Initialize these, so they're not null.
            _singleAnnotationsPool = new List<ClusterAnnotation>();
            _clusterAnnotationsPool = new List<ClusterAnnotation>();
            _clusterAnnotations = new List<ClusterAnnotation>();
            _originalAnnotations = new List<MKAnnotation>();
            _annotationsToBeSet = new List<MKAnnotation>();
        }

        public void SetAnnotations(List<MKAnnotation> annotations)
        {
            if (!_isSettingAnnotations)
            {
                _originalAnnotations = annotations;
                _isSettingAnnotations = true;

                RemoveAnnotations(_clusterAnnotations.ToArray()); // TODO: Co-variant conversion

                _singleAnnotationsPool = new List<ClusterAnnotation>();
                _clusterAnnotationsPool = new List<ClusterAnnotation>();
                
                int numberOfAnnotationsInPool = 2 * NumberOfClusters();

                for (int i = 0; i < numberOfAnnotationsInPool; i++)
                {
                    var annotation = new ClusterAnnotation {Type = ClusterAnnotationType.Leaf};
                    _singleAnnotationsPool.Add(annotation);

                    annotation = new ClusterAnnotation {Type = ClusterAnnotationType.Cluster};
                    _clusterAnnotationsPool.Add(annotation);
                }

                AddAnnotations(_singleAnnotationsPool.ToArray()); // TODO: Co-variant conversion
                AddAnnotations(_clusterAnnotationsPool.ToArray()); // TODO: Co-variant conversion

                var temp = _singleAnnotationsPool;
                temp.AddRange(_clusterAnnotationsPool);
                _clusterAnnotations = temp;
                
                const double gamma = 1.0;
                const string clusterTitle = "%d elements";

                // TODO: Run async, like the original, performance optimizing
                //await Task.Run(async () =>
                //{
                var mapPointAnnotations = new List<MapPointAnnotation>();

                foreach (var annotation in annotations)
                {
                    var mapPointAnnotation = new MapPointAnnotation(annotation);
                    mapPointAnnotations.Add(mapPointAnnotation);
                }

                const bool shouldShowSubtitle = true;
                _rootMapCluster = MapCluster.RootClusterForAnnotations(mapPointAnnotations, gamma, clusterTitle, shouldShowSubtitle);

                // TODO: When implementing async; run on main thread from here, uses interface objects.
                ClusterInMapRect(VisibleMapRect);

                _isSettingAnnotations = false;

                if (_annotationsToBeSet != null)
                {
                    var newAnnotations = _annotationsToBeSet;
                    _annotationsToBeSet = null;
                    SetAnnotations(newAnnotations);
                }

                //});
            }
            else
            {
                _annotationsToBeSet = annotations;
            }
        }

        public new void AddAnnotationObject(NSObject annotation)
        {
            throw new Exception("Unsupported method call in ADClusterMapView instance! Please call SetAnnotations() instead.");
        }

        public new void AddAnnotationObjects(params NSObject[] annotations)
        {
            throw new Exception("Unsupported method call in ADClusterMapView instance! Please call SetAnnotations() instead.");
        }

        public override void SelectAnnotation(NSObject annotation, bool animated)
        {
            base.SelectAnnotation(ClusterAnnotationForOriginalAnnotation((MKAnnotation)annotation), animated);
        }

        public void SelectClusterAnnotation(ClusterAnnotation annotation, bool animated)
        {
            base.SelectAnnotation(annotation, animated);
        }

        private List<ClusterAnnotation> DisplayedAnnotations()
        {
            var displayedAnnotations = new List<ClusterAnnotation>();

            var temp = _singleAnnotationsPool;
            temp.AddRange(_clusterAnnotationsPool);

            foreach (var annotation in temp)
            {
                if (annotation.GetType() == typeof(ClusterAnnotation))
                    if (annotation.Coordinate.Latitude != MapConstants.Offscreen.Latitude &&
                        annotation.Coordinate.Longitude != MapConstants.Offscreen.Longitude)
                    {
                        displayedAnnotations.Add(annotation);
                    }
            }

            return displayedAnnotations;
        }

        // TODO: Check performance, is not used currently.
        private List<MKAnnotation> Annotations()
        {
            var otherAnnotations = base.Annotations.ToList().Where(p => p.GetType() != typeof (ClusterAnnotation)).Select(p => (MKAnnotation)p);
            var temp = _originalAnnotations;
            temp.AddRange(otherAnnotations);

            return temp;
        }

        public ClusterAnnotation ClusterAnnotationForOriginalAnnotation(MKAnnotation annotation)
        {
            if (annotation.GetType() != typeof (ClusterAnnotation))
            {
                foreach (var clusterAnnotation in DisplayedAnnotations())
                {
                    if (clusterAnnotation.Cluster.IsRootClusterForAnnotation(annotation))
                    {
                        return clusterAnnotation;
                    }
                }
            }

            return null;
        }

        public void AddNonClusteredAnnotation(MKAnnotation annotation)
        {
            base.AddAnnotation(annotation);
        }

        public void AddNonClusteredAnnotations(List<MKAnnotation> annotations)
        {
            base.AddAnnotations(annotations.ToArray());
        }

        public void RemoveNonClusteredAnnotation(MKAnnotation annotation)
        {
            base.RemoveAnnotation(annotation);
        }

        public void RemoveNonClusteredAnnotations(List<MKAnnotation> annotations)
        {
            base.RemoveAnnotations(annotations.ToArray()); // TODO: Co-variant conversion
        }

        public void ClusterInMapRect(MKMapRect rect)
        {
            if (_rootMapCluster != null)
            {
                var clustersToShowOnMap = _rootMapCluster.FindChildrenInMapRect(NumberOfClusters(), rect);
                var availableSingleAnnotations = new List<ClusterAnnotation>();
                var availableClusterAnnotations = new List<ClusterAnnotation>();
                var selfDividingSingleAnnotations = new List<ClusterAnnotation>();
                var selfDividingClusterAnnotations = new List<ClusterAnnotation>();

                // TODO: Surely this can be done prettier
                var temp = _singleAnnotationsPool;
                temp.AddRange(_clusterAnnotationsPool);

                foreach (var annotation in temp)
                {
                    bool isAncestor = false;
                    if (annotation.Cluster != null)
                    {
                        foreach (var cluster in clustersToShowOnMap)
                        {
                            if (annotation.Cluster.IsAncestorOf(cluster))
                            {
                                if (cluster.Annotation != null)
                                {
                                    selfDividingSingleAnnotations.Add(annotation);
                                }
                                else
                                {
                                    selfDividingClusterAnnotations.Add(annotation);
                                }

                                isAncestor = true;
                                break;
                            }
                        }
                    }

                    if (!isAncestor)
                    {
                        if (!AnnotationBelongsToClusters(annotation, clustersToShowOnMap))
                        {
                            if (annotation.Type == ClusterAnnotationType.Leaf)
                            {
                                availableSingleAnnotations.Add(annotation);
                            }
                            else
                            {
                                availableClusterAnnotations.Add(annotation);
                            }
                        }
                    }
                }

                // TODO: Surely this can be done prettier
                var temp2 = selfDividingSingleAnnotations;
                temp2.AddRange(selfDividingClusterAnnotations);

                foreach (ClusterAnnotation annotation in temp2)
                {
                    var willNeedAnAvailableAnnotation = false;
                    var originalAnnotationCoordinate = annotation.Coordinate;
                    var originalAnnotationCluster = annotation.Cluster;

                    foreach (var cluster in clustersToShowOnMap)
                    {
                        if (originalAnnotationCluster.IsAncestorOf(cluster))
                        {
                            if (!willNeedAnAvailableAnnotation)
                            {
                                willNeedAnAvailableAnnotation = true;
                                annotation.Cluster = cluster;

                                if (cluster.Annotation != null)
                                {
                                    if (annotation.Type != ClusterAnnotationType.Leaf)
                                    {
                                        var singleAnnotation = availableSingleAnnotations.Last();
                                        availableSingleAnnotations.RemoveAt(availableSingleAnnotations.Count - 1);
                                            // TODO What if count = 0
                                        singleAnnotation.Cluster = annotation.Cluster;
                                        singleAnnotation.Coordinate = originalAnnotationCoordinate;
                                        availableClusterAnnotations.Add(annotation);
                                    }
                                }
                            }
                            else
                            {
                                ClusterAnnotation availableAnnotation;

                                if (cluster.Annotation != null)
                                {
                                    availableAnnotation = availableSingleAnnotations.Last();
                                    availableSingleAnnotations.RemoveAt(availableSingleAnnotations.Count - 1);
                                        // TODO What if count = 0
                                }
                                else
                                {
                                    availableAnnotation = availableClusterAnnotations.Last();
                                    availableClusterAnnotations.RemoveAt(availableClusterAnnotations.Count - 1);
                                        // TODO What if count = 0
                                }

                                availableAnnotation.Cluster = cluster;
                                availableAnnotation.Coordinate = originalAnnotationCoordinate;
                            }
                        }
                    }
                }

                foreach (var cluster in clustersToShowOnMap)
                {
                    var didAlreadyFindAChild = false;
                    var tempList = _clusterAnnotations; // TODO: The original adjusts the annotation in below for loop, we can't do that

                    foreach (var annotation in _clusterAnnotations)
                    {
                        if (annotation.GetType() != typeof (MKUserLocation))
                        {
                            if (annotation.Cluster != null && annotation.GetType() != typeof (MKUserLocation))
                            {
                                if (cluster.IsAncestorOf(annotation.Cluster))
                                {
                                    var index = _clusterAnnotations.IndexOf(annotation);

                                    if (annotation.Type == ClusterAnnotationType.Leaf)
                                    {
                                        ClusterAnnotation clusterAnnotation = availableClusterAnnotations.Last();
                                        availableClusterAnnotations.RemoveAt(availableClusterAnnotations.Count - 1);
                                            // TODO What if count = 0
                                        clusterAnnotation.Cluster = cluster;
                                        clusterAnnotation.Coordinate = annotation.Coordinate;
                                        availableSingleAnnotations.Add(annotation);

                                        tempList[index] = clusterAnnotation;
                                    }
                                    else
                                    {
                                        tempList[index].Cluster = cluster;
                                    }

                                    if (didAlreadyFindAChild)
                                    {
                                        tempList[index].ShouldBeRemovedAfterAnimation = true;
                                    }

                                    if (tempList[index].IsOffscreen(tempList[index].Coordinate))
                                    {
                                        tempList[index].Coordinate = tempList[index].Cluster.ClusterCoordinate;
                                    }

                                    didAlreadyFindAChild = true;
                                }
                            }
                        }
                    }

                    _clusterAnnotations = tempList;
                }

                foreach (var annotation in availableSingleAnnotations)
                {
                    if (annotation.Type == ClusterAnnotationType.Leaf && annotation.Cluster != null)
                    {
                        annotation.Reset();
                    }
                }

                foreach (var annotation in availableClusterAnnotations)
                {
                    if (annotation.Type == ClusterAnnotationType.Cluster && annotation.Cluster != null)
                    {
                        annotation.Reset();
                    }
                }

                BeginAnimations("ADClusterMapViewAnimation");
                SetAnimationBeginsFromCurrentState(false);
                SetAnimationDelegate(this);
                SetAnimationDuration(0.5f);

                foreach (var annotation in _clusterAnnotations)
                {
                    if (annotation.GetType() != typeof (MKUserLocation) && annotation.Cluster != null)
                    {
                        if (!annotation.IsOffscreen(annotation.Coordinate))
                            annotation.Coordinate = annotation.Cluster.ClusterCoordinate;
                    }

                }

                CommitAnimations();

                foreach (MapCluster cluster in clustersToShowOnMap)
                {
                    bool isAlreadyAnnotated = false;

                    foreach (ClusterAnnotation annotation in _clusterAnnotations)
                    {
                        if (annotation.GetType() != typeof (MKUserLocation))
                        {
                            if (cluster.Equals(annotation.Cluster))
                            {
                                isAlreadyAnnotated = true;
                                break;
                            }
                        }
                    }

                    if (!isAlreadyAnnotated)
                    {
                        if (cluster.Annotation != null)
                        {
                            (availableSingleAnnotations.Last()).Cluster = cluster;
                            (availableSingleAnnotations.Last()).Coordinate = cluster.ClusterCoordinate;
                            availableSingleAnnotations.RemoveAt(availableSingleAnnotations.Count - 1);
                                // TODO What if count = 0
                        }
                        else
                        {
                            (availableClusterAnnotations.Last()).Cluster = cluster;
                            (availableClusterAnnotations.Last()).Coordinate = cluster.ClusterCoordinate;
                            availableClusterAnnotations.RemoveAt(availableClusterAnnotations.Count - 1);
                                // TODO What if count = 0
                        }
                    }
                }

                foreach (ClusterAnnotation annotation in availableSingleAnnotations)
                {
                    if (annotation.Type == ClusterAnnotationType.Leaf)
                        annotation.Reset();
                }

                foreach (ClusterAnnotation annotation in availableClusterAnnotations)
                {
                    if (annotation.Type == ClusterAnnotationType.Cluster)
                        annotation.Reset();
                }
            }
        }

        private int NumberOfClusters()
        {
            return 32;
        }

        private bool AnnotationBelongsToClusters(ClusterAnnotation annotation, List<MapCluster> clusters)
        {
            if (annotation.Cluster == null) return false;

            foreach (var cluster in clusters)
            {
                if (cluster.IsAncestorOf(annotation.Cluster) || cluster.Equals(annotation.Cluster))
                {
                    return true;
                }
            }

            return false;
        }

    }
}