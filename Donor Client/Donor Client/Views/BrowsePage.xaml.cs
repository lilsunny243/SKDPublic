using DonorClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowsePage : ContentPage
    {

        public BrowseViewModel ViewModel { get; } = new BrowseViewModel();

        public BrowsePage()
        {
            InitializeComponent();
        }

        private double cvHeight;
        private void FilterButton_Toggled(object sender, ToggledEventArgs e)
        {
            CollectionFrame.TranslateTo(0, e.Value ? FilterLayout.Height - 18 : -18, 500, Easing.CubicInOut);
            RefreshView.TranslateTo(0, e.Value ? -16 : 0, 500, Easing.CubicInOut);
            if (e.Value)
                cvHeight = CollectionView.Height;
            CollectionView.LayoutTo(new Rectangle(CollectionView.X, CollectionView.Y, CollectionView.Width,
                e.Value ? (cvHeight - FilterLayout.Height) : cvHeight),
                500, Easing.CubicInOut);
        }

    }
}