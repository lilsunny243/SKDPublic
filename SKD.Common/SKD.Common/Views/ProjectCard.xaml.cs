using System.Threading.Tasks;
using AiForms.Dialogs;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectCard : ContentView
    {
        public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(Command<Project>), typeof(ProjectCard));
        public Command<Project> TappedCommand { get => (Command<Project>)GetValue(TappedCommandProperty); set => SetValue(TappedCommandProperty, value); }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(ProjectCardViewModel), typeof(ProjectCard));
        public ProjectCardViewModel ViewModel { get => (ProjectCardViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }


        public ProjectCard()
        {
            InitializeComponent();
        }

        private double fixedHeight = -1;

        public Command ImageButtonCommand => new Command(ShowImageView);
        private async void ShowImageView()
        {

            if (fixedHeight < 0d)
            {
                RemoveBinding(HeightRequestProperty);
                fixedHeight = HeightRequest = InfoView.Height;
            }

            var screenLocation = DependencyService.Get<ILocationFetcher>().GetScreenLocation(this);
            var overlayView = new CardImageOverlayView(screenLocation.Y, Width, fixedHeight, TappedCommand);
            overlayView.Disappeared += () => Opacity = 1;
            _ = Dialog.Instance.ShowAsync(overlayView, ViewModel);
            await Task.Delay(100);
            overlayView.RunAnimation(false);
            await Task.Delay(20);
            Opacity = 0;
        }

    }

}