using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GiftAidDetailsView : DialogView
    {
        public GiftAidDetailsView()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("CloseButton") is Button b)
                b.Clicked += (_, __) => DialogNotifier.Cancel();
        }

        private void SaveButton_Clicked(object sender, EventArgs e) => DialogNotifier.Complete();
    }
}