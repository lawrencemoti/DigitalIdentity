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
        public Task<Identity> VerifyIdentityWithProviderSource(Guid identityId, CancellationToken ct);

        public Task<Identity> VerifyIdentityWithDHA(Guid identityId, CancellationToken ct);
    }
}
