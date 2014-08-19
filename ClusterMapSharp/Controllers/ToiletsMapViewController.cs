//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# for Xamarin by Steven Thewissen.
//

namespace ClusterMapSharp.Controllers
{

    /// <summary>
    /// ViewController that shows all the public restrooms in Paris.
    /// </summary>
    public class ToiletsMapViewController : MapViewController
    {
        public override string SeedFileName
        {
            get { return "CDToilets"; }
        }

        public override string PictoName
        {
            get { return "CDToilet.png"; }
        }

        public override string ClusterPictoName
        {
            get { return "CDToiletCluster.png"; }
        }
    }

}