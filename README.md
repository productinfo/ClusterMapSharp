#ClusterMapSharp

This project is a (currently incomplete) C# / Xamarin translation/implementation of ADClusterMapView. There are still a few issues with it:

- Most importantly: no annotations are currently being added to the map.
- The original additional delegates have not been translated yet.

## The original: ADClusterMapView - MKMapView with clustering

ADClusterMapView is a drop-in subclass of MKMapView that displays and animates clusters of annotations. This is very useful in cases where you have to display many annotations on the map. Its concept and implementation were described on Applidium's [website][].

[website]: http://applidium.com/en/news/too_many_pins_on_your_map/

## Quick start

1. Add the content of the ClusterMapView folder to your iOS project.
2. Turn your MKMapView instance into a ClusterMapView.
3. Set your annotations by calling `SetAnnotations:`. Do not use `AddAnnotation` or `AddAnnotations` as they are not supported yet.

## Future Work

There are a couple of improvements that could be done. Feel free to send us pull requests if you want to contribute!

- Add support for annotations addition and removal.
- Add support for multiple independant trees
- More?