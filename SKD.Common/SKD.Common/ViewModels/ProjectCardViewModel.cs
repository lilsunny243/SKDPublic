using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SKD.Common.Models;
using SKD.Common.Utils;
using Xamarin.Forms;

namespace SKD.Common.ViewModels
{
    public class ProjectCardViewModel : CardViewModel<Project>
    {
        private string name;
        private int raised;
        private Uri[] imageURIs;
        private Color accentColour;
        private ProjectStatus status;
        private bool isUrgent;
        private List<CardTagViewModel> tags;

        public string Name { get => name; private set => SetProperty(ref name, value); }
        public Uri[] ImageURIs { get => imageURIs; private set => SetProperty(ref imageURIs, value, RaisePropertyChanged(nameof(ImageCount))); }
        public int ImageCount { get => ImageURIs?.Length ?? 0; }
        public Color AccentColour { get => accentColour; private set => SetProperty(ref accentColour, value); }
        public ProjectStatus Status { get => status; set => SetProperty(ref status, value); }
        public int Target { get; private set; }
        public int Raised { get => raised; set => SetProperty(ref raised, value, RaisePropertyChanged(nameof(Progress))); }
        public double Progress => Math.Min(Raised / (double)Target, 1d);
        public bool IsUrgent { get => isUrgent; set => SetProperty(ref isUrgent, value); }
        public List<CardTagViewModel> Tags { get => tags; private set => SetProperty(ref tags, value); }

        public ProjectCardViewModel(Project project) : base(project)
        {
            name = Culture.IsSpanish ? project.NameEs : project.NameEn;
            status = project.Status;
            Target = project.Target;
            raised = project.Raised;
            isUrgent = project.IsUrgent;
            accentColour = project.PrimaryImpactMeasurementArea.GetAccentColour();
            tags = project.ImpactMeasurementAreas.Keys
                .OrderBy(x => x == project.PrimaryImpactMeasurementArea ? int.MinValue : x.GetColourOrder())
                .Select(x => new CardTagViewModel(x)).ToList();
            imageURIs = project.Images.Select(x => new Uri(x.Uri)).ToArray();
        }

        public override void Update(Project project)
        {
            base.Update(project);
            Name = Culture.IsSpanish ? project.NameEs : project.NameEn;
            Status = project.Status;
            Raised = project.Raised;
            IsUrgent = project.IsUrgent;
            AccentColour = project.PrimaryImpactMeasurementArea.GetAccentColour();
            Tags = project.ImpactMeasurementAreas.Keys
                .OrderBy(x => x == project.PrimaryImpactMeasurementArea ? int.MinValue : x.GetColourOrder())
                .Select(x => new CardTagViewModel(x)).ToList();
            ImageURIs = project.Images.Select(x => new Uri(x.Uri)).ToArray();
        }

    }

    public class CardTagViewModel : BaseViewModel
    {
        public CardTagViewModel(ImpactMeasurementArea ima) =>
            (IMA, Text, Colour, Glyph) = (ima, ima.GetLocalisedName(), ima.GetAccentColour(), ima.GetGlyph());

        public ImpactMeasurementArea IMA { get; }
        public string Text { get; }
        public Color Colour { get; }
        public string Glyph { get; }
    }

}
