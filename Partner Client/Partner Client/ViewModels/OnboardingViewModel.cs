using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PartnerClient.Models;
using PartnerClient.Resources;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Functions = PartnerClient.Services.CloudFunctions;

namespace PartnerClient.ViewModels
{
    public class OnboardingViewModel : PageViewModel
    {
        public List<ToggleOption> AreasOfWorkOptions { get; } = new List<ToggleOption>
        {
            new ToggleOption(AppResources.Administration),
            new ToggleOption(AppResources.Reach),
            new ToggleOption(AppResources.Prevention),
            new ToggleOption(AppResources.Mentoring),
            new ToggleOption(AppResources.Center),
            new ToggleOption(AppResources.YoungBoys),
            new ToggleOption(AppResources.AlexisHouse),
        };

        public List<ToggleOption> DocumentOptions { get; } = new List<ToggleOption>
        {
            new ToggleOption(AppResources.WorkPlanOMRE),
            new ToggleOption(AppResources.Budget),
            new ToggleOption(AppResources.WorkManual),
            new ToggleOption(AppResources.JobProfile),
            new ToggleOption(AppResources.EmployeeContract),
            new ToggleOption(AppResources.Induction),
            new ToggleOption(AppResources.TrainingPlan),
            new ToggleOption(AppResources.ProceduresAndResponsabilites),
            new ToggleOption(AppResources.RiskAssessment),
            new ToggleOption(AppResources.Securities),
            new ToggleOption(AppResources.ReportStructure),
            new ToggleOption(AppResources.EmployeeEvaluation),
            new ToggleOption(AppResources.Presentation),
            new ToggleOption(AppResources.ImpactEvaluation),
            new ToggleOption(AppResources.PastoralPlan),
            new ToggleOption(AppResources.VolunteersProfile),
            new ToggleOption(AppResources.VolunteersApplicationProcess),
            new ToggleOption(AppResources.VolunteersSelectionProcess),
            new ToggleOption(AppResources.VolunteersTraningPlan),
            new ToggleOption(AppResources.VolunteersWorkPlan),
            new ToggleOption(AppResources.VolunteersCareAndSupportPlan),
            new ToggleOption(AppResources.VolunteersValuesAndProcedures),
            new ToggleOption(AppResources.VolunteersAssessmentsAndIncentives),
            new ToggleOption(AppResources.VisitsPlanAndPurpose),
            new ToggleOption(AppResources.VisitsWaiverForm),
            new ToggleOption(AppResources.VisitsInformationPage),
            new ToggleOption(AppResources.VisitsPresentationAndTour),
            new ToggleOption(AppResources.VisitsEvaluationAndAcknowledgement),
        };

        private bool join = true;
        private string name = string.Empty;

        private string orgName = string.Empty;
        private string orgEmail = string.Empty;
        private string orgPhone = string.Empty;
        private string orgAddress = string.Empty;

        private string teamCode = string.Empty; // For join input;
        private string authCode = string.Empty; // For create output;

        public bool Join { get => join; set => SetProperty(ref join, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }

        public string OrgName { get => orgName; set => SetProperty(ref orgName, value); }
        public string OrgEmail { get => orgEmail; set => SetProperty(ref orgEmail, value); }
        public string OrgPhone { get => orgPhone; set => SetProperty(ref orgPhone, value); }
        public string OrgAddress { get => orgAddress; set => SetProperty(ref orgAddress, value); }

        public string AuthCode { get => authCode; private set => SetProperty(ref authCode, value); }
        public string TeamCode { get => teamCode; set => SetProperty(ref teamCode, value); }

        private bool isBusy = false;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, OnIsBusyChanged); }
        private void OnIsBusyChanged()
        {
            CancelJoinCommand.ChangeCanExecute();
            SubmitCommand.ChangeCanExecute();
        }

        public event Action<bool, bool>? StateChanged;
        private bool isCreating;
        private bool isJoining;

        public bool IsCreating { get => isCreating; set => SetProperty(ref isCreating, value); }
        public bool IsJoining { get => isJoining; set => SetProperty(ref isJoining, value); }


        private string? joiningTeamName;
        public string? JoiningTeamName { get => joiningTeamName; set => SetProperty(ref joiningTeamName, value); }


        public OnboardingViewModel()
        {
            OnUserChanged(PartnerUser.Current);
            PartnerUser.CurrentUpdated += OnUserChanged;
        }

        private async void OnUserChanged(PartnerUser? user)
        {
            if (user is null)
                return;
            (Name, IsCreating) = (user.Name, user.IsTeamLeader);
            IsCreating = !string.IsNullOrEmpty(user.TeamUID) && user.IsTeamLeader;
            IsJoining = !string.IsNullOrEmpty(user.TeamUID) && !user.IsTeamLeader;
            try
            {
                if (IsCreating)
                {
                    var teamRequest = (await Firestore.Collection("TeamCreationRequests").Document(user.TeamUID!)
                        .GetAsync()).ToObject<TeamCreationRequest>()!;
                    AuthCode = teamRequest.AuthCode;
                }
                else if (IsJoining)
                {
                    var team = (await Firestore.Collection("Teams").Document(user.TeamUID!)
                        .GetAsync()).ToObject<Team>()!;
                    JoiningTeamName = team.OrganisationName;
                }
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            StateChanged?.Invoke(IsCreating, IsJoining);
            CancelJoinCommand.ChangeCanExecute();
        }

        public Command<bool> SwitchButtonCommand => new Command<bool>(b =>
        {
            Join = b;
            SwitchButtonItems[0].IsSelected = b;
            SwitchButtonItems[1].IsSelected = !b;
        });

        public List<SwitchButtonViewModel<bool>> SwitchButtonItems { get; } = new List<SwitchButtonViewModel<bool>>
        {
            new SwitchButtonViewModel<bool>(AppResources.JoinTeam, true, true),
            new SwitchButtonViewModel<bool>(AppResources.CreateTeam, false)
        };

        private Command? _submitCommand;
        public Command SubmitCommand => _submitCommand ??= new Command(ExecuteSubmitCommand, () => !IsBusy);

        private async void ExecuteSubmitCommand()
        {
            IsBusy = true;
            try
            {
                await PartnerUser.Current!.Doc.UpdateAsync(new { Name });
                await Auth.CurrentUser!.GetIdTokenAsync(true); // Force refresh of ID Token in case account was just created!
                if (Join)
                {
                    var teamDoc = (await Firestore.Collection("Teams")
                        .WhereEqualsTo(nameof(Team.AuthCode), TeamCode)
                        .GetAsync()).Documents
                        .SingleOrDefault();
                    var team = teamDoc.ToObject<Team>();
                    if (team is null)
                    {
                        await Shell.Current.DisplayAlert(AppResources.InvalidCode, AppResources.InvalidCodeMessage, AppResources.Okay);
                        return;
                    }
                    var confirm = await Shell.Current.DisplayAlert(AppResources.Confirm,
                        string.Format(AppResources.ConfirmTeamFormat, team.OrganisationName), 
                        AppResources.Yes, AppResources.No);
                    if (confirm)
                    {
                        // Usually we wouldn't have to do the next 3 commands here because Firestore would handle
                        // latency compensation for us, but the condition is on the user document which is modified by a cloud function
                        // which does have some latency that we need to compensate for instead
                        JoiningTeamName = team.OrganisationName;
                        IsJoining = true;
                        StateChanged?.Invoke(IsCreating, IsJoining);
                        await teamDoc.Reference.Collection("MemberRequests").Document(PartnerUser.Current.UID)
                            .SetAsync(new TeamMember()
                            {
                                Name = Name,
                                Email = PartnerUser.Current.Email
                            });
                    }
                }
                else
                {
                    // Same as here for the latency compensation
                    var authCode = AuthCode = GetRandomString(6);
                    IsCreating = true;
                    StateChanged?.Invoke(IsCreating, IsJoining);
                    var user = PartnerUser.Current;
                    var request = new TeamCreationRequest()
                    {
                        OrganisationName = OrgName,
                        ContactDetails = new ContactInfo()
                        {
                            Email = OrgEmail,
                            Address = OrgAddress,
                            PhoneNumber = OrgPhone
                        },
                        AuthCode = authCode,
                        Leader = new TeamLeader()
                        {
                            UID = user.UID,
                            Name = Name,
                            Email = user.Email
                        },
                        AreasOfWork = AreasOfWorkOptions.Where(x => x.IsChecked).Select(x => x.Name).ToList(),
                        AvailableDocuments = DocumentOptions.Where(x => x.IsChecked).Select(x => x.Name).ToList(),
                    };
                    await Firestore.Collection("TeamCreationRequests").AddAsync(request);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(AppResources.Error, ex.Message, AppResources.Okay);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static readonly Random rand = new Random();

        private static char GetRandomChar() => (char)(rand.Next(3) switch
        {
            0 => rand.Next(65, 91),
            1 => rand.Next(97, 123),
            _ => rand.Next(48, 58)
        });

        private static string GetRandomString(int n)
        {
            var sb = new StringBuilder(n);
            for (int i = 0; i < n; i++)
                sb.Append(GetRandomChar());
            return sb.ToString();
        }

        private Command? _cancelJoinCommand;
        public Command CancelJoinCommand => _cancelJoinCommand ??= new Command(ExecuteCancelJoinCommand, () => IsJoining && !IsBusy);

        private async void ExecuteCancelJoinCommand()
        {
            IsBusy = true;
            await Functions.LeaveTeamAsync();
            IsBusy = false;
        }

        public Command LogoutCommand => new Command(() => Auth.SignOut());

    }

    public class ToggleOption
    {

        public ToggleOption(string name) => (Name, IsChecked) = (name, false);

        public string Name { get; }
        public bool IsChecked { get; set; }
    }
}
