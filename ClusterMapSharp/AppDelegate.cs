//
//  Created by Patrick Nollet.
//  Copyright 2011 Applidium. All rights reserved.
//
//  Converted to C# by Steven Thewissen - 2014.
//

using ClusterMapSharp.Controllers;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ClusterMapSharp
{

    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        UIWindow _window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            _window = new UIWindow(UIScreen.MainScreen.Bounds);

            var tabbarController = new UITabBarController();

            var toiletsViewController = new UINavigationController(new ToiletsMapViewController
            {
                TabBarItem = new UITabBarItem("Free Toilets", new UIImage("CDToiletItem.png"), 0)
            });

            var streetlightsViewController = new UINavigationController(new StreetlightsMapViewController
            {
                TabBarItem = new UITabBarItem("Streetlights", new UIImage("CDStreetlightItem.png"), 0)
            });

            tabbarController.ViewControllers = new UIViewController[] { toiletsViewController, streetlightsViewController };
            _window.RootViewController = tabbarController;
            _window.MakeKeyAndVisible();

            return true;
        }
    }

}