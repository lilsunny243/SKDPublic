using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SKD.Common.Models;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectUpdateCard : ContentView
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(ProjectUpdateCardViewModel), typeof(ProjectUpdateCard));
        public ProjectUpdateCardViewModel ViewModel { get => (ProjectUpdateCardViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }

        public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(Command<ProjectUpdate>), typeof(ProjectUpdateCard));
        public Command<ProjectUpdate>? TappedCommand { get => (Command<ProjectUpdate>?)GetValue(TappedCommandProperty); set => SetValue(TappedCommandProperty, value); }

        public bool ForSpecificProject { get; set; }
        public bool NotForSpecificProject => !ForSpecificProject;

        public ProjectUpdateCard()
        {
            InitializeComponent();
        }
    }
}