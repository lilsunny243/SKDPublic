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
    public partial class FaceSlider : ContentView
    {

        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(FaceSlider), defaultBindingMode: BindingMode.TwoWay);
        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }


        public static readonly BindableProperty FaceTypeProperty = BindableProperty.Create(nameof(FaceType), typeof(FaceType), typeof(FaceSlider), FaceType.Default);
        public FaceType FaceType { get => (FaceType)GetValue(FaceTypeProperty); set => SetValue(FaceTypeProperty, value); }


        public FaceSlider()
        {
            InitializeComponent();
        }
    }
}