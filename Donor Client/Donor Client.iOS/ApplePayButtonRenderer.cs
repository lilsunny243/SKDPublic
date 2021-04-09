using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DonorClient.iOS;
using DonorClient.Views;
using Foundation;
using PassKit;
using SKD.Common.Themes;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NativePayButton), typeof(ApplePayButtonRenderer))]
namespace DonorClient.iOS
{
    public class ApplePayButtonRenderer : ViewRenderer<NativePayButton, PKPaymentButton>
    {
        private PKPaymentButton? _button;

        public ApplePayButtonRenderer()
        {                
            // Subscribe Theme Change
            ThemeEngine.OnThemeChanged += e => OnThemeChanged(e);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<NativePayButton> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                // Unsubscribe
                _button.TouchUpInside -= OnNativeButtonPressed;
            }
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    _button = new PKPaymentButton(PKPaymentButtonType.Donate, ThemeEngine.IsEffectivelyLight ? PKPaymentButtonStyle.Black : PKPaymentButtonStyle.White) { Enabled = e.NewElement.IsEnabled };
                    SetNativeControl(_button); 
                }
                // Subscribe
                _button.TouchUpInside += OnNativeButtonPressed;
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
            Element?.SendAppleClicked();
        }

        private void OnThemeChanged(ThemeChangedEventArgs e)
        {
            if (_button != null)
            {
                _button = new PKPaymentButton(PKPaymentButtonType.Donate, e.IsEffectivelyLight ? PKPaymentButtonStyle.Black : PKPaymentButtonStyle.White);
                SetNativeControl(_button);
                // Subscribe
                _button.TouchUpInside += OnNativeButtonPressed;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // Unsubscribe Theme Change
            ThemeEngine.OnThemeChanged -= OnThemeChanged;
            _button = null;
        }
    }
}