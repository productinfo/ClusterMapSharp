#ClusterMapSharp

This project is a (currently incomplete) C# / Xamarin translation/implementation of [ADClusterMapView][] as originally developed by Applidium. The credits for the original go to Applidium. Currently there's no map clustering component for Xamarin, which is why I decided to try translating the original to C#. There are still a few issues with this translated version which I'm hoping the community is willing to help solve:

- Most importantly: no annotations are currently being added to the map. All the classes from the original have been translated, but there appears to be a bug somewhere that is causing clusters to not show up on the map.
- The original additional delegates have not been translated yet.

## The original: ADClusterMapView - MKMapView with clustering

ADClusterMapView is a drop-in subclass of MKMapView that displays and animates clusters of annotations. This is very useful in cases where you have to display many annotations on the map. Its concept and implementation were described on Applidium's [website][].

[ADClusterMapView]: https://github.com/applidium/ADClusterMapView
[website]: http://applidium.com/en/news/too_many_pins_on_your_map/

## Quick start

1. Add the content of the `Maps` folder to your iOS project.
2. Turn your MKMapView instance into a ClusterMapView.
3. Set your annotations by calling `SetAnnotations`. Do not use `AddAnnotation` or `AddAnnotations` as they are not supported yet.

## Future Work

There are a couple of improvements that could be done. Feel free to send us pull requests if you want to contribute!

- Add support for annotations addition and removal.
- Add support for multiple independant trees
- More?