using PartnerClient.Models;
using PartnerClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TeamPage : ContentPage
    {
        public TeamViewModel ViewModel { get; } = new TeamViewModel();

        public TeamPage()
        {
            InitializeComponent();
        }
    }

    public class TeamMemberTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? MemberTemplate { get; set; }

        public DataTemplate? MemberRequestTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is TeamMemberViewModel)
                return MemberTemplate;
            else if (item is TeamMemberRequestViewModel)
                return MemberRequestTemplate;
            return null;
        }
    }

}