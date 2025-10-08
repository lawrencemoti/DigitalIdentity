using ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Contracts.Infrastructure
{
    public interface IIdentityVerificationAgent
    {
        public Task<Identity> GetCachedProfileFromVendor(long identityNumber);

        public Task<Identity> RealTimeIDVCheck(long identityNumber);
    }
}
