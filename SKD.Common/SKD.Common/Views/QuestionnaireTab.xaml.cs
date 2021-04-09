using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionnaireTab : ContentView
    {
        public bool IsNested { get; set; }

        public QuestionnaireTab()
        {
            InitializeComponent();
        }
    }
}