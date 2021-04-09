using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using System.Drawing;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Editor), typeof(SKD.Common.iOS.CustomEditorRenderer))]

namespace SKD.Common.iOS
{
    public class CustomEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if(Control != null)
            {
                Control.Layer.CornerRadius = 8;

                //Control.LeftView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
                //Control.RightView = new UIView(new CGRect(0, 0, 8, this.Control.Frame.Height));
                //Control.LeftViewMode = UITextFieldViewMode.Always;
                //Control.RightViewMode = UITextFieldViewMode.Always;

                //Control.BorderStyle = UITextBorderStyle.None;
                //Element.HeightRequest = 30;
            }
        }
    }
}