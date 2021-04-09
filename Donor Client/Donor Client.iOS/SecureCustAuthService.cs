using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonorClient.iOS;
using DonorClient.Services;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly:Dependency(typeof(SecureCustAuthService))]
namespace DonorClient.iOS
{
    class SecureCustAuthService : ISecureCustAuthService
    {
        public void Launch(string uri, Color accentColour)
        {
            Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(new Uri(uri), new Uri("skd://sca.completed"));
        }
    }
}