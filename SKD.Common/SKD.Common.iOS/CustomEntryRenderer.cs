using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using System.Drawing;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Entry), typeof(SKD.Common.iOS.CustomEntryRenderer))]

namespace SKD.Common.iOS
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            // Check for only Numeric keyboard
            if (Element?.Keyboard == Keyboard.Numeric)
            {
                AddDoneButton();
            }
            if(Control != null)
            {
                Control.Layer.CornerRadius = 8;
                //Control.Layer.BorderWidth = 3;
                //Control.Layer.BorderColor = UIColor.White.CGColor;

                //Control.LeftView = new UIView(new CGRect(0, 0, 4, 0));
                //Control.LeftViewMode = UITextFieldViewMode.Always;
                //Element.HeightRequest =

                Control.LeftView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
                Control.RightView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
                Control.LeftViewMode = UITextFieldViewMode.Always;
                Control.RightViewMode = UITextFieldViewMode.Always;

                Control.BorderStyle = UITextBorderStyle.None;
                Element.HeightRequest = 30;
            }
        }

        /// <summary>
        /// Add toolbar with Done button
        /// </summary>
        protected void AddDoneButton()
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));

            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate {
                this.Control.ResignFirstResponder();
            });

            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton
            };
            this.Control.InputAccessoryView = toolbar;
        }
    }
}