using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SKD.Common.Views
{
    public class FadeTriggerAction : TriggerAction<VisualElement>
    {
        public FadeDirection Direction { get; set; }

        protected override async void Invoke(VisualElement sender)
        {
            if (Direction is FadeDirection.In)
            {
                sender.IsVisible = true;
                await sender.FadeTo(1d, easing: Easing.CubicInOut);
            }
            else if (Direction is FadeDirection.Out)
            {
                await sender.FadeTo(0d, easing: Easing.CubicInOut);
                sender.IsVisible = false;
            }
        }
    }

    public enum FadeDirection
    {
        In,
        Out
    }
}
