using Newtonsoft.Json;
using PartnerClient.Models;
using Plugin.FirebaseAuth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PartnerClient.Services
{
    public static class CloudFunctions
    {
        private static readonly HttpClient client = new HttpClient();
        private static IAuth Auth => CrossFirebaseAuth.Current.Instance;

        private static async Task CallFunctionAsync(string name, object? data = null)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, 
                @"" + name); # REDACTED
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                await Auth.CurrentUser!.GetIdTokenAsync(true));
            message.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if(!(data is null))
                message.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await client.SendAsync(message);
            System.Diagnostics.Debug.WriteLine($"Cloud Functions - Status: {resp.StatusCode}," +
                $" Response Body: {await resp.Content.ReadAsStringAsync()}");
        }

        private static async Task<T> CallFunctionAsync<T>(string name, object? data = null)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post,
                @"https://europe-west2-street-kids-direct-dms.cloudfunctions.net/" + name);
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                await Auth.CurrentUser!.GetIdTokenAsync(true));
            message.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (!(data is null))
                message.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await client.SendAsync(message);
            System.Diagnostics.Debug.WriteLine($"Cloud Functions - Status: {resp.StatusCode}," +
                $" Response Body: {await resp.Content.ReadAsStringAsync()}");
            return JsonConvert.DeserializeObject<T>(await resp.Content.ReadAsStringAsync());
        }

        //public static Task RequestCreateTeamAsync(string organisationName, ContactInfo contactDetails) 
        //    => CallFunctionAsync(Names.RequestCreateTeam, new TeamCreationRequest(organisationName, contactDetails));
        //public static Task RequestJoinTeamAsync(string teamUid)
        //    => CallFunctionAsync(Names.RequestJoinTeam, new TeamJoinRequest(teamUid));

        public static Task LeaveTeamAsync()
            => CallFunctionAsync(Names.LeaveTeam);

        public static Task HandleJoinTeamRequestAsync(bool accepted, string memberUid)
            => CallFunctionAsync(Names.HandleJoinTeamRequest, new TeamJoinResponse(accepted, memberUid));

        public static Task TransferTeamOwnershipAsync(string newLeaderUID)
            => CallFunctionAsync(Names.TransferTeamOwnership, new TeamOwnershipTransfer(newLeaderUID));

        public static Task<int> GetNextChildNumber()
            => CallFunctionAsync<int>(Names.GetNextChildNumber);


        private static class Names
        {
            //public const string RequestCreateTeam = "requestCreateTeam";
            //public const string RequestJoinTeam = "requestJoinTeam";
            public const string LeaveTeam = "leaveTeam";
            public const string HandleJoinTeamRequest = "handleJoinTeamRequest";
            public const string TransferTeamOwnership = "transferTeamOwnership";
            public const string GetNextChildNumber = "getNextChildNumber";
        }

    }
}
