using SKD.Common.Droid;
using SKD.Common.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace SKD.Common.Droid
{
    public class LocationFetcher : ILocationFetcher
    {
        public Point GetScreenLocation(VisualElement element)
        {
            var renderer = Platform.GetRenderer(element);
            var nativeView = renderer.View;
            var location = new int[2];
            var density = nativeView.Context.Resources.DisplayMetrics.Density;

            nativeView.GetLocationOnScreen(location);
            return new Point(location[0] / density, location[1] / density);
        }
    }
}