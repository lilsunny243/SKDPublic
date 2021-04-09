using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using DonorClient.Droid;
using DonorClient.Views;
using SKD.Common.Themes;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(NativePayButton), typeof(GooglePayButtonRenderer))]
namespace DonorClient.Droid
{
    public class GooglePayButtonRenderer : ViewRenderer<NativePayButton, AView>
    {
        private AView _button;

        protected override void OnElementChanged(ElementChangedEventArgs<NativePayButton> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                // Unsubscribe
                _button.Click -= OnNativeButtonPressed;
            }
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    _button = LayoutInflater.From(Context).Inflate(Resource.Layout.donate_with_googlepay_button, null);
                    _button.Enabled = e.NewElement.IsEnabled;
                    SetNativeControl(_button);
                }
                // Subscribe
                _button.Click += OnNativeButtonPressed;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName && _button != null)
                _button.Enabled = Element.IsEnabled;
        }

        private void OnNativeButtonPressed(object sender, EventArgs e)
        {
            Element?.SendGoogleClicked();
        }
    }
}