//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using System;
using System.Collections.Generic;
using ClusterMapSharp.Maps;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using Newtonsoft.Json;

namespace ClusterMapSharp.Controllers
{

    /// <summary>
    /// Default controller to support our custom MKMapView.
    /// </summary>
    public abstract class MapViewController : UIViewController
    {

        #region members

        private ClusterMapView _mapView;

        #endregion

        #region properties

        public abstract string SeedFileName { get; }

        public abstract string PictoName { get; }

        public abstract string ClusterPictoName { get; }

        #endregion

        #region methods

        /// <summary>
        /// Handles when the view is done loading.
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initialize the control.
            _mapView = new ClusterMapView
            {
                Frame = View.Bounds, // Fullscreen map.
                VisibleMapRect = new MKMapRect(135888858.533591, 92250098.902419, 190858.927912, 145995.678292), // Zoom to Paris.
                Delegate = new ClusterMapViewDelegate(), // Custom delegate.
            };

            // Add it to the page.
            Add(_mapView);

            // Some navbar styling.
            UINavigationBar.Appearance.TintColor = UIColor.Blue;
            
            // Add a button to start adding our annotations. This is not done on ViewLoad to make debugging easier.
            // If we do this on ViewDidLoad and start debugging we have to return within 15 seconds or else our app is killed.
            NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Add them!", UIBarButtonItemStyle.Plain, AddAnnotations), false);
        }

        /// <summary>
        /// Adds annotations to our mapview control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAnnotations(object sender, EventArgs e)
        {
            // TODO: Performance improvement; make async

            Console.WriteLine("Loading data!");

            var annotations = new List<MKAnnotation>();
            var jsonData = NSData.FromFile(NSBundle.MainBundle.PathForResource(SeedFileName, "json")).ToString();

            foreach (var annotationData in JsonConvert.DeserializeObject<List<JsonData>>(jsonData))
            {
                annotations.Add(new ClusterableAnnotation(annotationData));
            }

            Console.WriteLine("Building KD-Tree!");
            _mapView.SetAnnotations(annotations);
            Console.WriteLine("Done building KD-Tree!");
        }

        #endregion

    }

}