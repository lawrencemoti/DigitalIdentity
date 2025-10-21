using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using ApplicationCore.Models.Infrastructure.Datanamix;
using AutoMapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ApplicationCore.Enumerators.Enum;

namespace Infrastructure.IDVAPIs
{
    public class IdentityVerificationAgent : IIdentityVerificationAgent
    {
        private readonly ILogger<IdentityVerificationAgent> _log;
        private readonly IConfiguration _cfg;
        private readonly IMapper _mapper;
        private readonly IdentityDBContext _db;

        public IdentityVerificationAgent(IConfiguration configuration,
            IMapper mapper, ILogger<IdentityVerificationAgent> logger, IdentityDBContext db)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _cfg = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        #region IIdentityVerificationAgent Implementation
        public async Task<Identity> VerifyIdentityWithProviderSource(Guid identityId, CancellationToken ct)
        {
            Identity? rec = await _db.Identities.Include(i => i.Verifications)
                .FirstOrDefaultAsync(i => i.Id == identityId, ct);
            
            if (rec is null) return new Identity();

            try
            {
                var request = new
                {
                    IdNumber = rec.IdentityNumber,
                    EnvironmentType = _cfg["ThirdPartyAPIs:DatanamixAPI:EnvironmentType"],
                    OutputFormat = "JSON",
                    ClientReference = "test"
                };
                                
                string resBody = await TryCallProvider(rec.Id, _cfg["ThirdPartyAPIs:DatanamixAPI:BaseUrl"], 
                    _cfg["ThirdPartyAPIs:DatanamixAPI:ProfileIDSearchEndpoint"], 
                    "provider",
                    JsonContent.Create(request),ct);

                if (resBody == "error" || resBody == "timeout")
                {
                    rec.Status = IdentityStatus.Failed;
                    await _db.SaveChangesAsync(ct);
                    return rec;
                }

                var obj = JsonSerializer.Deserialize<ProfileResponse.Root>(resBody);
                rec = _mapper.Map<Identity>(obj?.ProfileIdSearchResult.ConsumerDetails.FirstOrDefault());
                rec.Status = IdentityStatus.Verified;
                await _db.SaveChangesAsync(ct);

                return rec;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Critical error in with Provider Internal Data Source ");
                return new Identity();
            }
        }

        public async Task<Identity> VerifyIdentityWithDHA(Guid identityId, CancellationToken ct)
        {
            Identity? rec = await _db.Identities.Include(i => i.Verifications)
                .FirstOrDefaultAsync(i => i.Id == identityId, ct);

            if (rec is null) return new Identity();
            try
            {
                var request = new
                {
                    IdNumber = rec.IdentityNumber,
                    EnvironmentType = _cfg["ThirdPartyAPIs:DatanamixAPI:EnvironmentType"],
                    OutputFormat = "JSON",
                    PDFEncryptionPassword = rec.IdentityNumber
                };

                
                string resBody = await TryCallProvider(rec.Id, _cfg["ThirdPartyAPIs:DatanamixAPI:BaseUrl"], 
                    _cfg["ThirdPartyAPIs:DatanamixAPI:RealTimeIDVEndpoint"], 
                    "provider",
                    JsonContent.Create(request),ct);

                if (resBody == "error" || resBody == "timeout")
                {
                    rec.Status = IdentityStatus.Failed;
                    await _db.SaveChangesAsync(ct);
                    return rec;
                }

                var obj = JsonSerializer.Deserialize<HomeAffairsIDVCheck.Root>(resBody);
                rec = _mapper.Map<Identity>(obj?.Results.RealTimeIDVerification);
                rec.Status = IdentityStatus.Verified;
                await _db.SaveChangesAsync(ct);

                return rec;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Critical error with Home Affairs IDV Check");
                return new Identity();
            }
        }

        #endregion

        #region Private Methods

        private async Task<string> TryCallProvider(Guid identityId, string baseUrl, string endpoint, 
            string provider, dynamic request, CancellationToken ct)
        {
            var idv = new IdentityVerification
            {
                IdentityId = identityId,
                Provider = provider
            };

            _db.Verifications.Add(idv); 
            await _db.SaveChangesAsync(ct);

            try
            {
                var resp = await new HttpClient().PostAsync($"{baseUrl}{endpoint}", request, ct);
                if (resp.IsSuccessStatusCode)
                {
                    var payload = await resp.Content.ReadAsStringAsync(ct);
                    
                    idv.Payload = payload;
                    idv.HttpStatus = (int)resp.StatusCode;
                    idv.Status = IdentityVerificationStatus.Success;
                    await _db.SaveChangesAsync(ct);

                    return payload;
                }
                else
                {
                    idv.Status = IdentityVerificationStatus.Error;
                    await _db.SaveChangesAsync(ct);
                    return "error";
                }
            }
            catch(Exception ex)
            {
                idv.Status = IdentityVerificationStatus.Error;
                idv.Payload = ex.Message; 
                await _db.SaveChangesAsync(ct);
                return "timeout"; 
            }
        }

        #endregion
    }
}
