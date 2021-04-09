using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerClient.ViewModels;
using SKD.Common.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(ChildUID), "childUid")]
    public partial class QuestionnaireCreationPage : ContentPage
    {
        public QuestionnaireCreationViewModel ViewModel { get; } = new QuestionnaireCreationViewModel();

        public string ChildUID { set => ViewModel.InitForChild(Uri.UnescapeDataString(value)); }

        public QuestionnaireCreationPage()
        {
            InitializeComponent();
        }
    }

    public class QuestionnaireItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? SliderTemplate { get; set; }
        public DataTemplate? OptionsTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if(item is QuestionnaireItemViewModel qItem)
            {
                return qItem.AnswerType switch
                {
                    AnswerType.Slider => SliderTemplate,
                    AnswerType.Options => OptionsTemplate,
                    _ => null
                };
            }
            return null;
        }
    }
}