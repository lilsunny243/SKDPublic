using Xamarin.Forms;

namespace SKD.Common.Views
{
    public class RotateTriggerAction : TriggerAction<VisualElement>
    {
        public double Rotation { get; set; }

        protected override void Invoke(VisualElement sender)
            => sender.RotateTo(Rotation, easing: Easing.CubicInOut);
    }
}
