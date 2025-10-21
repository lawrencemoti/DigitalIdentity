using ApplicationCore.Contracts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class SignatureService: ISignatureService
    {
        public byte[] HashSecret(string secret)
        {
            // In prod, prefer Argon2id/BCrypt/SCrypt library; kept simple here.
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
        }

        public string ComputeSignature(string secret, string body, string ts)
        {
            var payload = $"{ts}.{body}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(sig).ToLowerInvariant();
        }
    }
}
