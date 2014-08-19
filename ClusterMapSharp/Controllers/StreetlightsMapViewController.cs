//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

namespace ClusterMapSharp.Controllers
{

    /// <summary>
    /// ViewController that shows all the streetlights in Paris.
    /// </summary>
    public class StreetlightsMapViewController : MapViewController
    {
        public override string SeedFileName
        {
            get { return "CDStreetlights"; }
        }

        public override string PictoName
        {
            get { return "CDStreetlight.png"; }
        }

        public override string ClusterPictoName
        {
            get { return "CDStreetlightCluster.png"; }
        }
    }

}