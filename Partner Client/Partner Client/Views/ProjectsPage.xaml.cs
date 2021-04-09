using PartnerClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectsPage : ContentPage
    {
        public ProjectsViewModel ViewModel { get; } = new ProjectsViewModel();

        public ProjectsPage()
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