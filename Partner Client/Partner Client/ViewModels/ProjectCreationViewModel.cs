using FFImageLoading;
using PartnerClient.Models;
using PartnerClient.Resources;
using PartnerClient.Services;
using PartnerClient.Views;
using Plugin.CloudFirestore;
using Plugin.Media;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace PartnerClient.ViewModels
{
    public class ProjectCreationViewModel : PageViewModel
    {
        private IDocumentReference doc;
        private List<string>? alreadyUploadedImageUIDS;
        private bool isBusy;
        private bool isSavedDraft;

        private string name = string.Empty;
        private string description = string.Empty;
        private bool isUrgent;
        private bool isTeamLeader;

        public bool IsTeamLeader { get => isTeamLeader; private set => SetProperty(ref isTeamLeader, value, SubmitCommand.ChangeCanExecute); }
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, SubmitCommand.ChangeCanExecute); }
        public bool IsSavedDraft { get => isSavedDraft; set => SetProperty(ref isSavedDraft, value); }

        public string Name { get => name; set => SetProperty(ref name, value, SubmitCommand.ChangeCanExecute); }
        public string Description { get => description; set => SetProperty(ref description, value, SubmitCommand.ChangeCanExecute); }
        public bool IsUrgent { get => isUrgent; set => SetProperty(ref isUrgent, value); }
        private ImpactMeasurementArea? PrimaryIMA { get => IMAViewModels.SingleOrDefault(x => x.IsStarred)?.IMA; }
        public int Target
        {
            get => CostBreakdownBuilderComponents.Sum(x =>
            {
                var cost = x.IsShallow ? x.UnitCost : x.CalculatedCost;
                return (cost > 0 && x.Quantity > 0) ? cost * x.Quantity : 0;
            });
        }

        public ObservableCollection<ProjectCreationImageViewModel> ImageViewModels { get; } = new ObservableCollection<ProjectCreationImageViewModel>();

        public Command AddCostBreakdownComponentCommand => new Command(AddNewCostBreakdownComponent);

        private void AddCostBreakdownComponent(CostBreakdownBuilderComponent component)
        {
            component.PropertyChanged += CostBreakdownUpdated;
            component.CollectionChanged += CostBreakdownUpdated;
            CostBreakdownBuilderComponents.Add(component);
        }

        private void AddNewCostBreakdownComponent() => AddCostBreakdownComponent(new CostBreakdownBuilderComponent());

        public Command<CostBreakdownBuilderComponent> AddCostBreakdownSubcomponentCommand => new Command<CostBreakdownBuilderComponent>(x => x.Add(new CostBreakdownBuilderSubcomponent(x)));
        public ObservableCollection<CostBreakdownBuilderComponent> CostBreakdownBuilderComponents { get; } = new ObservableCollection<CostBreakdownBuilderComponent>();

        public Command<ICostBreakdownBuilderItem> RemoveCostBreakdownItemCommand => new Command<ICostBreakdownBuilderItem>(ExecuteRemoveCostBreakdownItemCommand);
        private void ExecuteRemoveCostBreakdownItemCommand(ICostBreakdownBuilderItem item)
        {
            if (item is CostBreakdownBuilderComponent component)
                CostBreakdownBuilderComponents.Remove(component);
            else if (item is CostBreakdownBuilderSubcomponent subcomponent)
                subcomponent.Parent.Remove(subcomponent);
        }

        public ProjectCreationIMAViewModel[] IMAViewModels { get; }

        public ProjectCreationViewModel()
        {
            doc = Firestore.Collection("Projects").Document();
            IsTeamLeader = PartnerUser.Current?.IsTeamLeader ?? false;
            ImageViewModels.CollectionChanged += (s, e) => SubmitCommand.ChangeCanExecute();
            CostBreakdownBuilderComponents.CollectionChanged += CostBreakdownUpdated;
            IMAViewModels = Enum.GetValues(typeof(ImpactMeasurementArea))
                .OfType<ImpactMeasurementArea>()
                .Select(x => new ProjectCreationIMAViewModel(x, OnIMASelectedChanged, OnIMAStarred))
                .ToArray();
            foreach (var vm in IMAViewModels)
                vm.PropertyChanged += (s, e) => SubmitCommand.ChangeCanExecute();
            PartnerUser.CurrentUpdated += u => IsTeamLeader = u?.IsTeamLeader ?? false;
        }

        private void OnIMASelectedChanged(ProjectCreationIMAViewModel vm)
        {
            if (vm.IsStarred && !vm.IsSelected && IMAViewModels.Any(x => x.IsSelected))
                IMAViewModels.First(x => x.IsSelected).IsStarred = true;
            vm.IsStarred = vm.IsSelected && PrimaryIMA is null;
        }

        private void OnIMAStarred(ProjectCreationIMAViewModel vm)
            => IMAViewModels.Where(x => x.IMA != vm.IMA).ForEach(x => x.IsStarred = false);

        public void Reset()
        {
            doc = Firestore.Collection("Projects").Document();
            IsSavedDraft = false;
            DeleteDraftCommand.ChangeCanExecute();
            ImageViewModels.Clear();
            IMAViewModels.ForEach(x => (x.Explanation, x.IsSelected, x.IsStarred) = (string.Empty, false, false));
            CostBreakdownBuilderComponents.Clear();
            (Name, Description, IsUrgent) = (string.Empty, string.Empty, false);
            alreadyUploadedImageUIDS = null;
        }

        private void CostBreakdownUpdated(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Target));
            SubmitCommand.ChangeCanExecute();
        }

        public Command AddImageCommand => new Command(ExecuteAddImageCommand);

        public Command<ProjectCreationImageViewModel> RemoveImageCommand
            => new Command<ProjectCreationImageViewModel>(vm => ImageViewModels.Remove(vm));


        private Command<bool>? _submitCommand;
        public Command<bool> SubmitCommand => _submitCommand ??= new Command<bool>(ExecuteSubmitCommand, CanSubmit);

        private bool CanSubmit(bool isDraft) => !isBusy
            && (isDraft || IsTeamLeader)
            && ((!string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Description)
            && ImageViewModels.Any()
            && IMAViewModels.Any(x => x.IsSelected)
            && IMAViewModels.Where(x => x.IsSelected).All(x => !string.IsNullOrWhiteSpace(x.Explanation))
            && CostBreakdownBuilderComponents.All(x => !string.IsNullOrWhiteSpace(x.Name) && x.All(s => !string.IsNullOrWhiteSpace(s.Name)))
            && Target > 0) || isDraft);

        private async void ExecuteAddImageCommand()
        {
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (!(file is null))
                    ImageViewModels.Add(new ProjectCreationImageViewModel(file.GetStreamWithImageRotatedForExternalStorage));
            }
            catch { }
        }

        private async void ExecuteSubmitCommand(bool saveAsDraft)
        {
            IsBusy = true;
            bool isSpanish = Culture.IsSpanish;
            List<string> thingsToTranslate = new List<string>();
            var name = string.IsNullOrWhiteSpace(Name) ? Resources.AppResources.Untitled : Name;
            var desc = Description ?? string.Empty;

            // Impact Measurement Areas
            var imas = IMAViewModels.Where(x => x.IsSelected).ToArray();
            var imaExplanations = imas.Select(x => x.Explanation ?? string.Empty).ToArray();
            thingsToTranslate.AddRange(imaExplanations);

            // Cost Breakdown
            int[] subcomponentCounts = CostBreakdownBuilderComponents.Select(x => x.Count).ToArray();
            var componentNames = CostBreakdownBuilderComponents.Select(x => x.Name ?? string.Empty).ToArray();
            var subcomponentNames = CostBreakdownBuilderComponents.SelectMany(x => x.Select(s => s.Name ?? string.Empty)).ToArray();
            thingsToTranslate.AddRange(componentNames);
            thingsToTranslate.AddRange(subcomponentNames);

            // Translate EVERYTHING in one API Call
            var (arrayTranslations, paramTranslations) = await TranslationService.TranslateAsync(thingsToTranslate, name, desc);

            // Impact Measurement Areas
            var imaExplanationTranslations = arrayTranslations.Take(imas.Length).ToArray();
            Dictionary<ImpactMeasurementArea, ImpactMeasurementAreaExplanation> imaData = new Dictionary<ImpactMeasurementArea, ImpactMeasurementAreaExplanation>();
            for (int i = 0; i < imas.Length; i++)
            {
                var (es, en) = isSpanish ? (imaExplanations[i], imaExplanationTranslations[i]) : (imaExplanationTranslations[i], imaExplanations[i]);
                imaData.Add(imas[i].IMA, new ImpactMeasurementAreaExplanation() { Es = es, En = en });
            }

            // Cost Breakdown
            var componentNameTranslations = arrayTranslations.Skip(imas.Length).Take(componentNames.Length).ToArray();
            var subcomponentNameTranslations = arrayTranslations.Skip(imas.Length + componentNames.Length).ToArray();
            List<CostBreakdownComponent> costBreakdownData = new List<CostBreakdownComponent>();
            for (int i = 0; i < componentNames.Length; i++)
            {
                var builderComponent = CostBreakdownBuilderComponents[i];
                var (es, en) = isSpanish
                    ? (componentNames[i], componentNameTranslations[i])
                    : (componentNameTranslations[i], componentNames[i]);
                var component = new CostBreakdownComponent()
                {
                    NameEs = es,
                    NameEn = en,
                    Quantity = builderComponent.Quantity,
                    UnitCost = builderComponent.IsShallow ? builderComponent.UnitCost : builderComponent.CalculatedCost
                };
                if (!builderComponent.IsShallow)
                {
                    List<CostBreakdownSubcomponent> children = new List<CostBreakdownSubcomponent>();
                    for (int j = 0; j < subcomponentCounts[i]; j++)
                    {
                        var builderSubcomponent = CostBreakdownBuilderComponents[i][j];
                        var k = subcomponentCounts.Take(i).Sum() + j;
                        var (subEs, subEn) = isSpanish
                            ? (subcomponentNames[k], subcomponentNameTranslations[k])
                            : (subcomponentNameTranslations[k], subcomponentNames[k]);
                        var subcomponent = new CostBreakdownSubcomponent()
                        {
                            NameEs = subEs,
                            NameEn = subEn,
                            Quantity = builderSubcomponent.Quantity,
                            UnitCost = builderSubcomponent.UnitCost
                        };
                        children.Add(subcomponent);
                    }
                    component.Children = children;
                }
                costBreakdownData.Add(component);
            }

            // Create Project Model
            var (translatedName, translatedDescription) = (paramTranslations[0], paramTranslations[1]);
            var project = new Project()
            {
                UID = doc.Id,
                NameEn = isSpanish ? translatedName : name,
                DescriptionEn = isSpanish ? translatedDescription : desc,
                NameEs = isSpanish ? name : translatedName,
                DescriptionEs = isSpanish ? desc : translatedDescription,
                IsUrgent = IsUrgent,
                Status = saveAsDraft ? ProjectStatus.Draft : ProjectStatus.Pending,
                Target = Target,
                PartnerUID = Team.Current!.UID,
                PartnerName = Team.Current.OrganisationName,
                Raised = 0,
                ImpactMeasurementAreas = imaData,
                PrimaryImpactMeasurementArea = PrimaryIMA.GetValueOrDefault(),
                CostBreakdown = costBreakdownData,
                NotifyAllStatus = ProjectNotifyAllStatus.Disabled,
                DatePublished = null
            };
            var imagesRef = Storage.RootReference
                 .GetChild("Teams").GetChild(Team.Current.UID)
                 .GetChild("Projects").GetChild(doc.Id)
                 .GetChild("Public");
            if (alreadyUploadedImageUIDS?.Any() ?? false)
            {
                await Task.WhenAll(alreadyUploadedImageUIDS.Where(x => !ImageViewModels.Any(y => y.UID == x)).Select(x => imagesRef.GetChild(x).DeleteAsync()));
                await Task.WhenAll(ImageViewModels.Where(x => !alreadyUploadedImageUIDS.Contains(x.UID)).Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            }
            else
                await Task.WhenAll(ImageViewModels.Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            var imageUIDs = ImageViewModels.Select(x => x.UID);
            var imageURIs = await Task.WhenAll(imageUIDs.Select(x => imagesRef.GetChild(x).GetDownloadUrlAsync()));
            project.Images = imageUIDs.Zip(imageURIs, (k, v) => new ProjectImage { Uid = k, Uri = v.OriginalString }).ToList();
            await doc.SetAsync(project);
            if (saveAsDraft)
            {
                alreadyUploadedImageUIDS = project.Images.Select(x => x.Uid).ToList();
                IsSavedDraft = true;
                DeleteDraftCommand.ChangeCanExecute();
            }
            else
            {
                try
                {
                    await Navigation.PopAsync();
                }
                catch
                {
                    // Attempt to absorb sharpnado bug
                    // Should remove once move to Xamarin Forms drag-drop reorder in collection view (assuming that becomes a thing at some point)
                    // TODO ^^
                }
            }
            IsBusy = false;
        }

        public async void LoadProjectDraft(string uid)
        {
            bool isSpanish = Culture.IsSpanish;
            doc = Firestore.Collection("Projects").Document(uid);
            IsSavedDraft = true;
            DeleteDraftCommand.ChangeCanExecute();
            var project = (await doc.GetAsync()).ToObject<Project>()!;
            Name = isSpanish ? project.NameEs : project.NameEn;
            Description = isSpanish ? project.DescriptionEs : project.DescriptionEn;
            IsUrgent = project.IsUrgent;

            // Images
            alreadyUploadedImageUIDS = project.Images.Select(x => x.Uid).ToList();
            foreach (var image in project.Images)
                ImageViewModels.Add(new ProjectCreationImageViewModel(new Uri(image.Uri), image.Uid));

            // IMAs
            foreach (var ima in project.ImpactMeasurementAreas)
            {
                var vm = IMAViewModels.Single(x => x.IMA == ima.Key);
                vm.Explanation = isSpanish ? ima.Value.Es : ima.Value.En;
                vm.IsSelected = true;
                vm.IsStarred = ima.Key == project.PrimaryImpactMeasurementArea;
            }

            // Cost Breakdown
            foreach (var component in project.CostBreakdown)
            {
                var builderComponent = new CostBreakdownBuilderComponent()
                {
                    Name = isSpanish ? component.NameEs : component.NameEn,
                    Quantity = component.Quantity,
                    UnitCost = component.UnitCost
                };
                if (component.Children?.Any() ?? false)
                {
                    foreach (var subcomponent in component.Children)
                    {
                        var builderSubcomponent = new CostBreakdownBuilderSubcomponent(builderComponent)
                        {
                            Name = isSpanish ? subcomponent.NameEs : subcomponent.NameEn,
                            Quantity = subcomponent.Quantity,
                            UnitCost = subcomponent.UnitCost
                        };
                        builderComponent.Add(builderSubcomponent);
                    }
                }
                AddCostBreakdownComponent(builderComponent);
            }

        }

        private Command? _deleteDraftCommand;
        public Command DeleteDraftCommand => _deleteDraftCommand ??= new Command(ExecuteDeleteDraftCommand, () => IsSavedDraft);
        private async void ExecuteDeleteDraftCommand()
        {
            if (await Shell.Current.DisplayAlert(AppResources.DeleteDraftTitle, AppResources.DeleteDraftMessage, AppResources.Yes, AppResources.Cancel))
            {
                var imagesRef = Storage.RootReference
                     .GetChild("Teams").GetChild(Team.Current!.UID)
                     .GetChild("Projects").GetChild(doc.Id)
                     .GetChild("Public");
                await Task.WhenAll(alreadyUploadedImageUIDS.Select(x => imagesRef.GetChild(x).DeleteAsync()));
                await doc.DeleteAsync();
                try
                {
                    await Navigation.PopAsync();
                }
                catch
                {
                    // Attempt to absorb sharpnado bug
                    // Should remove once move to Xamarin Forms drag-drop reorder in collection view (assuming that becomes a thing at some point)
                    // TODO ^^
                }
            }
        }

    }

    public class ProjectCreationIMAViewModel : CardTagViewModel
    {
        private readonly Action<ProjectCreationIMAViewModel> _onSelectedChanged;
        private readonly Action<ProjectCreationIMAViewModel> _onStarred;

        private bool isSelected;
        private bool isStarred;
        private string explanation = string.Empty;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value, () => _onSelectedChanged?.Invoke(this)); }
        public bool IsStarred { get => isStarred; set => SetProperty(ref isStarred, value, () => (value ? _onStarred : null)?.Invoke(this)); }
        public string Explanation { get => explanation; set => SetProperty(ref explanation, value); }

        public ProjectCreationIMAViewModel(ImpactMeasurementArea ima,
            Action<ProjectCreationIMAViewModel> onSelectedChanged,
            Action<ProjectCreationIMAViewModel> onStarred) : base(ima)
            => (_onSelectedChanged, _onStarred) = (onSelectedChanged, onStarred);
    }

    public class ProjectCreationImageViewModel : BaseViewModel
    {
        private static readonly Random rand = new Random();

        private readonly byte[]? bytes;
        private readonly Uri? _uri;

        public ProjectCreationImageViewModel(Func<Stream> getStream)
        {
            using var memStream = new MemoryStream();
            getStream().CopyTo(memStream);
            bytes = memStream.ToArray();
            UID = rand.Next(0x10000).ToString("X4");
        }

        // Only for images that are already uploaded since the stream function will be null 
        public ProjectCreationImageViewModel(Uri uri, string uid) => (_uri, UID) = (uri, uid);

        public Stream? GetStream() => bytes is null ? null : new MemoryStream(bytes);

        public ImageSource Source => !(_uri is null) ? ImageSource.FromUri(_uri) : ImageSource.FromStream(GetStream);

        public string UID { get; }
    }

    public interface ICostBreakdownBuilderItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int UnitCost { get; set; }
        public int Quantity { get; set; }
    }

    public class CostBreakdownBuilderComponent : ObservableCollection<CostBreakdownBuilderSubcomponent>, ICostBreakdownBuilderItem
    {
        public CostBreakdownBuilderComponent() : base()
        {
            CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(IsShallow));
                RaisePropertyChanged(nameof(CalculatedCost));
            };
        }

        private string name = string.Empty;
        private int unitCost;
        private int quantity;

        public string Name { get => name; set { name = value; RaisePropertyChanged(); } }

        public int UnitCost
        {
            get => unitCost; set
            {
                if (value > 0 || value == -2)
                    unitCost = value;
                RaisePropertyChanged();
            }
        }

        public int Quantity
        {
            get => quantity; set
            {
                if (value > 0 || value == -2)
                    quantity = value;
                RaisePropertyChanged();
            }
        }

        public bool IsShallow => !this.Any();
        public int CalculatedCost => this.Sum(x => (x.UnitCost > 0 && x.Quantity > 0) ? x.UnitCost * x.Quantity : 0);

        public new void Add(CostBreakdownBuilderSubcomponent subcomponent)
        {
            subcomponent.PropertyChanged += (s, e) => RaisePropertyChanged(nameof(CalculatedCost));
            base.Add(subcomponent);
        }

        new public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    public class CostBreakdownBuilderSubcomponent : ICostBreakdownBuilderItem
    {
        private string name = string.Empty;
        private int unitCost;
        private int quantity;

        public CostBreakdownBuilderComponent Parent { get; }

        public CostBreakdownBuilderSubcomponent(CostBreakdownBuilderComponent parent) => Parent = parent;

        public string Name { get => name; set { name = value; RaisePropertyChanged(); } }

        public int UnitCost
        {
            get => unitCost; set
            {
                if (value > 0 || value == -2)
                    unitCost = value;
                RaisePropertyChanged();
            }
        }

        public int Quantity
        {
            get => quantity; set
            {
                if (value > 0 || value == -2)
                    quantity = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
