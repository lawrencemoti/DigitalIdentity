using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using ApplicationCore.Models.Infrastructure.Datanamix;
using AutoMapper;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.IDVAPIs
{
    public class IdentityVerificationAgent : IIdentityVerificationAgent
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public IdentityVerificationAgent(HttpClient httpClient, IConfiguration configuration,
            IMapper mapper)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Identity> GetCachedProfileFromVendor(long identityNumber)
        {
            try
            {
                var request = new
                {
                    IdNumber = identityNumber.ToString(),
                    EnvironmentType = _configuration["DatanamixAPI:EnvironmentType"],
                    OutputFormat = "JSON",
                    ClientReference = "test"
                };

                HttpResponseMessage res = await _httpClient.PostAsJsonAsync("v1/id-verification/profileID-search", request);
                res.EnsureSuccessStatusCode();

                string resBody = await res.Content.ReadAsStringAsync();

                var obj = JsonSerializer.Deserialize<ProfileResponse.Root>(resBody);
                Identity identity = _mapper.Map<Identity>(obj?.ProfileIdSearchResult.ConsumerDetails.FirstOrDefault());
                return identity;

                //TODO: Upsert identity to local DB
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request exception: {ex.Message}");
                return new Identity();
            }
        }

        public async Task<Identity> RealTimeIDVCheck(long identityNumber)
        {
            try
            {
                var request = new
                {
                    IdNumber = identityNumber.ToString(),
                    EnvironmentType = _configuration["DatanamixAPI:EnvironmentType"],
                    OutputFormat = "JSON",
                    PDFEncryptionPassword = identityNumber.ToString()
                };

                HttpResponseMessage res = await _httpClient.PostAsJsonAsync("v1/id-verification/realtime", request);
                res.EnsureSuccessStatusCode();

                string resBody = await res.Content.ReadAsStringAsync();

                var obj = JsonSerializer.Deserialize<HomeAffairsIDVCheck.Root>(resBody);
                Identity identity = _mapper.Map<Identity>(obj?.Results.RealTimeIDVerification);
                return identity;

                //TODO: Upsert identity to local DB
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request exception: {ex.Message}");
                return new Identity();
            }
        }
    }
}
