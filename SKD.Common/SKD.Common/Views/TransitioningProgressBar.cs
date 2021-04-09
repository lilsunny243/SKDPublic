
using Xamarin.Forms;

namespace SKD.Common.Views
{
    public class TransitioningProgressBar : ProgressBar
    {
        new public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(TransitioningProgressBar), 0d, propertyChanged: OnProgressChanged);
        new public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }

        private bool initialised = false;

        private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var (oldVal, newVal) = ((double)oldValue, (double)newValue);
            var bar = (bindable as TransitioningProgressBar)!;
            if ((oldVal == newVal && bar.initialised) || double.IsNaN(newVal)) return;
            bar.ProgressTo(newVal, bar.initialised ? 250u : 0u, Easing.CubicInOut);
            bar.initialised = true;
        }
    }
}