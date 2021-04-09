using System;
using System.Collections.Generic;
using System.Text;
using SKD.Common.Models;
using Xamarin.Forms;

namespace SKD.Common.ViewModels
{
    public class IMAFilterItem : BaseViewModel
    {
        public IMAFilterItem(ImpactMeasurementArea ima)
            => (IMA, Glyph, AccentColour) = (ima,
            ima.GetGlyph(),
            ima.GetAccentColour());

        public ImpactMeasurementArea IMA { get; }
        public string Glyph { get; }
        public Color AccentColour { get; }

        private bool isActive;
        public bool IsActive { get => isActive; set => SetProperty(ref isActive, value); }
    }
}
