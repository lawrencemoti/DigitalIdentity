using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Contracts.Core
{
    public interface ISignatureService
    {
        byte[] HashSecret(string secret);
        string ComputeSignature(string secret, string body, string timestamp);
    }
}
