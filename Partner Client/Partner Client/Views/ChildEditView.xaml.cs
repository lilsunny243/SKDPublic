using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChildEditView : DialogView
    {
        public ChildEditView()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            (GetTemplateChild("CloseButton") as Button)!.Clicked += (_, __) => DialogNotifier.Cancel();
        }

        private void SaveButton_Clicked(object sender, EventArgs e) => DialogNotifier.Complete();
    }
}