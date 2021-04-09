using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DonorClient.Models;

namespace DonorClient.Services
{
    public interface IApplePayService
    {
        public bool GetIsSupported();

        public Task<string> GetTokenAsync(int totalToPay, IEnumerable<Donation> donations, bool coverStripeFee);
    }

    public interface IGooglePayService
    {
        public Task<bool> GetIsSupportedAsync();

        public Task<string> GetTokenAsync(int totalToPay);
    }
}
