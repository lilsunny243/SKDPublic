using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DonorClient.Services
{
    public interface ISecureCustAuthService
    {
        public void Launch(string uri, Color accentColour);
    }
}
