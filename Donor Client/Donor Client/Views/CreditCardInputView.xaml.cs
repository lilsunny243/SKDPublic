using AiForms.Dialogs.Abstractions;
using DonorClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreditCardInputView : DialogView
    {
        public CreditCardInputView()
        {
            InitializeComponent();
        }

        private bool completed;

        private void SubmitButton_Clicked(object sender, EventArgs e)
        {
            completed = true;
            DialogNotifier.Complete();
        }

        public static event Action? Appearing;
        public static event Action? Disappearing;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("CloseButton") is Button b)
                b.Clicked += (_, __) => DialogNotifier.Cancel();
        }

        public override void RunPresentationAnimation()
        {
            base.RunPresentationAnimation();
            Appearing?.Invoke();
        }

        public override void RunDismissalAnimation()
        {
            base.RunDismissalAnimation();
            if (!completed)
                SettingsViewModel.ClearPaymentData?.Invoke();
            Disappearing?.Invoke();
        }
    }
}