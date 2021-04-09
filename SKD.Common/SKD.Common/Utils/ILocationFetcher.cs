using Xamarin.Forms;

namespace SKD.Common.Utils
{
    public interface ILocationFetcher
    {
        public Point GetScreenLocation(VisualElement element);
    }
}
