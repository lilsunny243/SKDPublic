using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerClient.Models;
using PartnerClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChildCard : ContentView
    {
        public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(Command<Kid>), typeof(ChildCard));
        public Command<Kid> TappedCommand { get => (Command<Kid>)GetValue(TappedCommandProperty); set => SetValue(TappedCommandProperty, value); }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(ChildCardViewModel), typeof(ChildCard));
        public ChildCardViewModel ViewModel { get => (ChildCardViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }


        public ChildCard()
        {
            InitializeComponent();
        }
    }
}