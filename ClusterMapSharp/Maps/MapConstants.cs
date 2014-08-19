//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

using MonoTouch.CoreLocation;

namespace ClusterMapSharp.Maps
{

    /// <summary>
    /// Wraps some constants used throughout the mapping component.
    /// </summary>
    public class MapConstants
    {

        public static CLLocationCoordinate2D Offscreen = new CLLocationCoordinate2D(85.0, 179.0);
        public const float ClusterDiscriminationPrecision = 1E-4f;

    }

}