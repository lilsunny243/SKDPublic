using SKD.Common.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace SKD.Common.iOS
{
    public class LocationFetcher : ILocationFetcher
    {
        public Point GetScreenLocation(VisualElement element)
        {
            var renderer = Platform.GetRenderer(element);
            var nativeView = renderer.NativeView;
            var rect = nativeView.Superview.ConvertPointToView(nativeView.Frame.Location, null);
            return new Point(rect.X, rect.Y);
        }
    }
}