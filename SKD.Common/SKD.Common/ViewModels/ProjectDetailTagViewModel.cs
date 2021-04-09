using System;
using System.Collections.Generic;
using System.Text;
using SKD.Common.Models;
using SKD.Common.Utils;

namespace SKD.Common.ViewModels
{
    public class ProjectDetailTagViewModel : CardTagViewModel
    {
        private string explanation = string.Empty;
        public string Explanation { get => explanation; set => SetProperty(ref explanation, value); }

        public ProjectDetailTagViewModel(KeyValuePair<ImpactMeasurementArea, ImpactMeasurementAreaExplanation> ima) : base(ima.Key)
            => Explanation = Culture.IsSpanish ? ima.Value.Es : ima.Value.En;
    }
}
