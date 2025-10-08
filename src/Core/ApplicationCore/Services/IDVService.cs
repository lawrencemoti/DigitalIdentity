using ApplicationCore.Contracts.Core;
using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class IDVService : IIDVService
    {
        private readonly IIdentityVerificationAgent _identityVerificationAgent;
        //private readonly IdentityDBContext _dbContext;

        public IDVService(IIdentityVerificationAgent identityVerificationAgent) 
        {
            _identityVerificationAgent = identityVerificationAgent 
                ?? throw new ArgumentNullException(nameof(identityVerificationAgent));
        }

        public async Task<Identity> GetIdentity(long identityNumber, int validityPeriod)
        {
            //TODO: Retrieve record from database
            //var identity = await db.Identities.FindAsync(id);

            return new Identity();
        }
    }
}
