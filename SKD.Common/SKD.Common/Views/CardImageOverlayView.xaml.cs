using System;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using SKD.Common.Models;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardImageOverlayView : DialogView
    {
        private static int prevIndex;
        private static string prevUID = string.Empty;

        public static CardImageOverlayView? Instance { get; private set; }
        public Command<Project> TappedCommand { get; }

        private readonly double originalHeight;
        private readonly double originalWidth;
        private readonly double originalTranslation;
        private double targetWidth;
        private double currentWidth;
        private double targetHeight;
        private double currentHeight;
        private double targetTranslation;
        private double currentTranslation;
        private double targetOpacity;
        private double currentOpacity;

        public CardImageOverlayView(double y, double width, double height, Command<Project> tappedCommand)
        {
            InitializeComponent();
            Instance = this;
            TappedCommand = tappedCommand;
            Card.WidthRequest = originalWidth = width;
            Card.HeightRequest = originalHeight = height;
            Card.TranslationY = originalTranslation = Device.RuntimePlatform == Device.Android ? y - 24 : y;
        }

        public event Action? Disappeared;

        public override void RunPresentationAnimation() { return; }

        public void RunAnimation(bool hide)
        {
            var vm = BindingContext as ProjectCardViewModel;
            if (vm?.UID == prevUID && !hide)
                CarouselView.ScrollTo(prevIndex);

            currentWidth = Card.Width;
            currentHeight = Card.Height;
            currentTranslation = Card.TranslationY;
            currentOpacity = MaterialFrame.Opacity;
            targetWidth = hide ? originalWidth : Width - 16;
            targetHeight = hide ? originalHeight : Height / 2d;
            targetTranslation = hide ? originalTranslation : Height / 4d;
            targetOpacity = hide ? 0d : 1d;

            this.Animate("CardImageOverlay", CardFlipAnimationCallback, length: 500, easing: Easing.CubicInOut);
        }

        private void CardFlipAnimationCallback(double t)
        {
            Card.WidthRequest = Lerp(currentWidth, targetWidth, t);
            Card.HeightRequest = Lerp(currentHeight, targetHeight, t);
            Card.TranslationY = Lerp(currentTranslation, targetTranslation, t);
            MaterialFrame.Opacity = Lerp(currentOpacity, targetOpacity, t);
            InfoAl.Opacity = 1 - MaterialFrame.Opacity;
        }

        private void Button_Clicked(object sender, EventArgs e) => GetRid();

        public async void GetRid()
        {
            Instance = null;
            prevIndex = CarouselView.Position;
            prevUID = (BindingContext as ProjectCardViewModel)?.UID ?? string.Empty;
            RunAnimation(true);
            await Task.Delay(700);
            DialogNotifier.Complete();
        }

        public override void RunDismissalAnimation() => Disappeared?.Invoke();

        private double Lerp(double start, double end, double t) => ((1 - t) * start) + (t * end);
    }
}