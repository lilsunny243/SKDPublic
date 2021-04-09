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
    public partial class Face : ContentView
    {

        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(Face));
        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }


        public static readonly BindableProperty FaceTypeProperty = BindableProperty.Create(nameof(FaceType), typeof(FaceType), typeof(Face), FaceType.Default);
        public FaceType FaceType { get => (FaceType)GetValue(FaceTypeProperty); set => SetValue(FaceTypeProperty, value); }


        public Face()
        {
            InitializeComponent();
        }
    }

    public enum FaceType
    {
        Default,
        Shock,
        Understanding
    }
}