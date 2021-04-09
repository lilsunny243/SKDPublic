using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [QueryProperty(nameof(ChildId), "childId")]
    [QueryProperty(nameof(QuestionnaireId), "qId")]
    [QueryProperty(nameof(ChildFirstNames), "firstNames")]
    [QueryProperty(nameof(ChildLastNames), "lastNames")]
    [QueryProperty(nameof(DateString), "dateString")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionnaireViewPage : ContentPage
    {
        private string? childId;
        private string? qId;

        public QuestionnaireViewModel ViewModel { get; } = new QuestionnaireViewModel();

        public string ChildId { set { childId = Uri.UnescapeDataString(value); TryInit(); } }
        public string QuestionnaireId { set { qId = Uri.UnescapeDataString(value); TryInit(); } }

        public string ChildFirstNames { set => ViewModel.ChildFirstNames = Uri.UnescapeDataString(value); }
        public string ChildLastNames { set => ViewModel.ChildLastNames = Uri.UnescapeDataString(value); }
        public string DateString { set => ViewModel.DateString = Uri.UnescapeDataString(value); }

        private void TryInit()
        {
            if (childId is null || qId is null) return;
            ViewModel.Init(childId, qId);
            childId = qId = null;
        }

        public QuestionnaireViewPage()
        {
            InitializeComponent();
        }
    }

    public class QuestionnaireAnswerTemplateSelector : DataTemplateSelector
    {

        public DataTemplate? SliderTemplate { get; set; }
        public DataTemplate? OptionsTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is QuestionnaireAnswerViewModel qAnswer)
            {
                return qAnswer.AnswerType switch
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