//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// Represents a single cluster and its children.
    /// </summary>
    public class MapCluster
    {

        #region members

        private readonly MapCluster _leftChild;
        private readonly MapCluster _rightChild;
        private readonly MKMapRect _mapRect;
        private readonly string _clusterTitle;

        #endregion

        #region properties

        public CLLocationCoordinate2D ClusterCoordinate { get; set; } // The coordinate for the cluster.

        public string Title
        {
            get
            {
                if (Annotation == null)
                {
                    if (_clusterTitle != null)
                    {
                        return string.Format(_clusterTitle, NumberOfChildren());
                    }
                }

                return null;
            }
        }

        public string Subtitle
        {
            get
            {
                if (Annotation == null && ShowSubtitle)
                {
                    return NamesOfChildren().Aggregate((i, j) => i + ", " + j);
                }

                return null;
            }
        }

        public MapPointAnnotation Annotation { get; set; }

        public List<MKAnnotation> OriginalAnnotations
        {
            get
            {
                List<MKAnnotation> originalAnnotations;

                if (Annotation != null)
                {
                    originalAnnotations = new List<MKAnnotation> {Annotation.Annotation};
                }
                else
                {
                    originalAnnotations = _leftChild.OriginalAnnotations;
                    originalAnnotations.AddRange(_rightChild.OriginalAnnotations);
                }

                return originalAnnotations;
            }
        }

        public int Depth { get; private set; }

        public bool ShowSubtitle { get; set; }

        public MKMapRect MapRect { get { return _mapRect;  } }

        #endregion

        #region constructors

        public MapCluster(List<MapPointAnnotation> annotations, int depth, MKMapRect mapRect, double gamma, string clusterTitle, bool showSubtitle)
        {
            Depth = depth;
            _mapRect = mapRect;
            _clusterTitle = clusterTitle;
            ShowSubtitle = showSubtitle;

            switch (annotations.Count)
            {
                case 0:
                    _leftChild = null;
                    _rightChild = null;
                    Annotation = null;
                    ClusterCoordinate = new CLLocationCoordinate2D(91,181); // Todo: Invalid, instead of constant.
                    break;

                case 1:
                    _leftChild = null;
                    _rightChild = null;
                    Annotation = annotations.Last();
                    ClusterCoordinate = Annotation.Annotation.Coordinate;
                    break;

                default:
                {
                    Annotation = null;

                    // Principal Component Analysis
                    // If cov(x,y) = ∑(x-x_mean) * (y-y_mean) != 0 (covariance different from zero), we are looking for the following principal vector:
                    // a (aX)
                    //   (aY)
                    //
                    // x_ = x - x_mean ; y_ = y - y_mean
                    //
                    // aX = cov(x_,y_)
                    //
                    //
                    // aY = 0.5/n * ( ∑(x_^2) + ∑(y_^2) + sqrt( (∑(x_^2) + ∑(y_^2))^2 + 4 * cov(x_,y_)^2 ) )

                    // compute the means of the coordinate
                    var xSum = 0.0;
                    var ySum = 0.0;

                    foreach (var annotation in annotations)
                    {
                        xSum += annotation.MapPoint.X;
                        ySum += annotation.MapPoint.Y;
                    }

                    var xMean = xSum / annotations.Count;
                    var yMean = ySum / annotations.Count;

                    if (gamma != 1.0)
                    {
                        // take gamma weight into account
                        var gammaSumX = 0.0;
                        var gammaSumY = 0.0;
                        var maxDistance = 0.0;
                        var meanCenter = new MKMapPoint(xMean, yMean);

                        foreach (var annotation in annotations)
                        {
                            var distance = MKGeometry.MetersBetweenMapPoints(annotation.MapPoint, meanCenter);
                            if (distance > maxDistance) { maxDistance = distance; }
                        }

                        var totalWeight = 0.0;
                        foreach (var annotation in annotations)
                        {
                            var point = annotation.MapPoint;
                            var distance = MKGeometry.MetersBetweenMapPoints(point, meanCenter);
                            var normalizedDistance = maxDistance != 0.0 ? distance / maxDistance : 1.0;
                            var weight = Math.Pow(normalizedDistance, gamma - 1.0);
                            gammaSumX += point.X * weight;
                            gammaSumY += point.Y * weight;
                            totalWeight += weight;
                        }

                        xMean = gammaSumX / totalWeight;
                        yMean = gammaSumY / totalWeight;
                    }
                    // compute coefficients

                    var sumXsquared = 0.0;
                    var sumYsquared = 0.0;
                    var sumXy = 0.0;

                    foreach (var annotation in annotations)
                    {
                        var x = annotation.MapPoint.X - xMean;
                        var y = annotation.MapPoint.Y - yMean;
                        sumXsquared += x * x;
                        sumYsquared += y * y;
                        sumXy += x * y;
                    }

                    double aX;
                    double aY;

                    if (Math.Abs(sumXy) / annotations.Count > MapConstants.ClusterDiscriminationPrecision)
                    {
                        aX = sumXy;
                        var lambda = 0.5 * ((sumXsquared + sumYsquared) + Math.Sqrt((sumXsquared + sumYsquared) * (sumXsquared + sumYsquared) + 4 * sumXy * sumXy));
                        aY = lambda - sumXsquared;
                    }
                    else
                    {
                        aX = sumXsquared > sumYsquared ? 1.0 : 0.0;
                        aY = sumXsquared > sumYsquared ? 0.0 : 1.0;
                    }

                    List<MapPointAnnotation> leftAnnotations;
                    List<MapPointAnnotation> rightAnnotations;

                    if (Math.Abs(sumXsquared) / annotations.Count < MapConstants.ClusterDiscriminationPrecision || Math.Abs(sumYsquared) / annotations.Count < MapConstants.ClusterDiscriminationPrecision)
                    {
                        // then every x equals XMean and we have to arbitrarily choose where to put the pivotIndex
                        var pivotIndex = annotations.Count / 2;
                        leftAnnotations = annotations.GetRange(0, pivotIndex);
                        rightAnnotations = annotations.GetRange(pivotIndex, annotations.Count - pivotIndex);
                    }
                    else
                    {
                        // compute scalar product between the vector of this regression line and the vector
                        // (x - x(mean))
                        // (y - y(mean))
                        // the sign of this scalar product determines which cluster the point belongs to
                        leftAnnotations = new List<MapPointAnnotation>();
                        rightAnnotations = new List<MapPointAnnotation>();

                        foreach (var annotation in annotations)
                        {
                            var point = annotation.MapPoint;
                            var positivityConditionOfScalarProduct = true;

                            // TODO: Don't know why this is here... its there in the original code. Or is it a wrong Objective-C interpretation?
                            if (true)
                            {
                                positivityConditionOfScalarProduct = (point.X - xMean) * aX + (point.Y - yMean) * aY > 0.0;
                            }
                            else
                            {
                                positivityConditionOfScalarProduct = (point.Y - yMean) > 0.0;
                            }

                            if (positivityConditionOfScalarProduct)
                                leftAnnotations.Add(annotation);
                            else
                                rightAnnotations.Add(annotation);
                        }
                    }

                    // compute map rects
                    double xMin = float.MaxValue, xMax = 0.0f, yMin = float.MaxValue, yMax = 0.0f;

                    foreach (var point in leftAnnotations.Select(annotation => annotation.MapPoint))
                    {
                        if (point.X > xMax){xMax = point.X;}
                        if (point.Y > yMax){yMax = point.Y;}
                        if (point.X < xMin){xMin = point.X;}
                        if (point.Y < yMin){yMin = point.Y;}
                    }

                    var leftMapRect = new MKMapRect(xMin, yMin, xMax - xMin, yMax - yMin);

                    xMin = float.MaxValue;
                    xMax = 0.0;
                    yMin = float.MaxValue;
                    yMax = 0.0;

                    foreach (var point in rightAnnotations.Select(annotation => annotation.MapPoint))
                    {
                        if (point.X > xMax){xMax = point.X;}
                        if (point.Y > yMax){yMax = point.Y;}
                        if (point.X < xMin){xMin = point.X;}
                        if (point.Y < yMin){yMin = point.Y;}
                    }

                    var rightMapRect = new MKMapRect(xMin, yMin, xMax - xMin, yMax - yMin);
                    ClusterCoordinate = MKMapPoint.ToCoordinate(new MKMapPoint(xMean, yMean));
                    _leftChild = new MapCluster(leftAnnotations, depth + 1, leftMapRect, gamma, clusterTitle, showSubtitle);
                    _rightChild = new MapCluster(rightAnnotations, depth + 1, rightMapRect, gamma, clusterTitle, showSubtitle);
                }
                    
                break;
            }
        }

        #endregion

        #region methods

        public static MapCluster RootClusterForAnnotations(List<MapPointAnnotation> initialAnnotations, double gamma, string clusterTitle, bool showSubtitle)
        {
            // KDTree
            var boundaries = new MKMapRect(Single.PositiveInfinity, Single.PositiveInfinity, 0.0, 0.0);

            foreach (var point in initialAnnotations.Select(annotation => annotation.MapPoint))
            {
                if (point.X < boundaries.Origin.X){boundaries.Origin.X = point.X;}
                if (point.Y < boundaries.Origin.Y){boundaries.Origin.Y = point.Y;}
                if (point.X > boundaries.Origin.X + boundaries.Size.Width){boundaries.Size.Width = point.X - boundaries.Origin.X;}
                if (point.Y > boundaries.Origin.Y + boundaries.Size.Height){boundaries.Size.Height = point.Y - boundaries.Origin.Y;}
            }

            Console.WriteLine("Computing KD-tree...");
            var cluster = new MapCluster(initialAnnotations, 0, boundaries, gamma, clusterTitle, showSubtitle);
            Console.WriteLine("Computation done !");
            return cluster;
        }

        public List<MapCluster> FindChildrenInMapRect(int n, MKMapRect mapRect)
        {
            // Start from the root (self)
            // Adopt a breadth-first search strategy
            // If MapRect intersects the bounds, then keep this element for next iteration
            // Stop if there are N elements or more
            // Or if the bottom of the tree was reached (d'oh!)

            var clusters = new List<MapCluster>() { this };
            var annotations = new List<MapCluster>();
            List<MapCluster> previousLevelClusters = null;
            List<MapCluster> previousLevelAnnotations = null;
            var clustersDidChange = true; // prevents infinite loop at the bottom of the tree

            while (clusters.Count + annotations.Count < n && clusters.Count > 0 && clustersDidChange)
            {
                previousLevelAnnotations = annotations;
                previousLevelClusters = clusters;
                clustersDidChange = false;
                var nextLevelClusters = new List<MapCluster>();

                foreach (var cluster in clusters)
                {
                    foreach (var child in cluster.Children())
                    {
                        if (child.Annotation != null)
                        {
                            annotations.Add(child);
                        }
                        else
                        {
                            if (MKMapRect.Intersects(mapRect, child.MapRect))
                            {
                                nextLevelClusters.Add(child);
                            }
                        }
                    }
                }

                if (nextLevelClusters.Count > 0)
                {
                    clusters = nextLevelClusters;
                    clustersDidChange = true;
                }
            }

            CleanClustersFromAncestorsOfClusters(clusters, annotations);

            if (clusters.Count + annotations.Count > n)
            {
                clusters = previousLevelClusters;
                annotations = previousLevelAnnotations;
                CleanClustersFromAncestorsOfClusters(clusters, annotations);
            }

            CleanClustersOutsideMapRect(clusters, mapRect);
            
            if (annotations != null)
            {
                annotations.AddRange(clusters);
                return annotations;
            }

            return new List<MapCluster>();
        }

        public List<MapCluster> Children()
        {
            var children = new List<MapCluster>();

            if (_leftChild != null)
            {
                children.Add(_leftChild);
            }

            if (_rightChild != null)
            {
                children.Add(_rightChild);
            }

            return children;
        }

        public bool IsAncestorOf(MapCluster mapCluster)
        {
            return Depth < mapCluster.Depth && (_leftChild == mapCluster || _rightChild == mapCluster || _leftChild.IsAncestorOf(mapCluster) || _rightChild.IsAncestorOf(mapCluster));
        }

        public bool IsRootClusterForAnnotation(MKAnnotation annotation)
        {
            return Annotation.Annotation == annotation || _leftChild.IsRootClusterForAnnotation(annotation) || _rightChild.IsRootClusterForAnnotation(annotation);
        }

        public int NumberOfChildren()
        {
            if (_leftChild == null && _rightChild == null)
            {
                return 1;
            }

            return _leftChild.NumberOfChildren() + _rightChild.NumberOfChildren();
        }

        public List<string> NamesOfChildren()
        {
            if (Annotation != null)
            {
                return new List<string> { Annotation.Annotation.Title };
            }

            var names = new List<string>();
            names.AddRange(_leftChild.NamesOfChildren());
            names.AddRange(_rightChild.NamesOfChildren());
            return names;
        }

        private void CleanClustersFromAncestorsOfClusters(List<MapCluster> clusters, List<MapCluster> referenceClusters)
        {
            var clustersToRemove = new List<MapCluster>();

            foreach (var cluster in clusters)
            {
                foreach (var referenceCluster in referenceClusters)
                {
                    if (cluster.IsAncestorOf(referenceCluster))
                    {
                        clustersToRemove.Add(cluster);
                        break;
                    }
                }
            }

            clusters.RemoveAll(clustersToRemove.Contains);
        }

        private void CleanClustersOutsideMapRect(List<MapCluster> clusters, MKMapRect mapRect)
        {
            var clustersToRemove = new List<MapCluster>();
         
            foreach (var cluster in clusters)
            {
                if (!mapRect.Contains(MKMapPoint.FromCoordinate(cluster.ClusterCoordinate)))
                {
                    clustersToRemove.Add(cluster);
                }
            }

            clusters.RemoveAll(clustersToRemove.Contains);
        }

        #endregion

    }

}