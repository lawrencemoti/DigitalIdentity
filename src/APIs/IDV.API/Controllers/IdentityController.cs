using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using IDV.API.APIModels;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IDV.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class IdentityController : ControllerBase
    {
        //private readonly IdentityDBContext _dbContext;
        private readonly IIdentityVerificationAgent _identityVerificationAgent;

        public IdentityController(IIdentityVerificationAgent identityVerificationAgent)
        {
            //_dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _identityVerificationAgent = identityVerificationAgent ?? throw new ArgumentNullException(nameof(identityVerificationAgent));

        }

        [HttpPost("Retrieve")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIdentity([FromBody] long identityNumber)
        {
            try
            {
                if (identityNumber.ToString().Length != 13)
                {
                    return BadRequest("Invalid identity number, must be 13 characters long.");
                }
                
                var identity = await _identityVerificationAgent.GetCachedProfileFromVendor(identityNumber);
                if (identity == null || identity.IdentityNumber == 0)
                {
                    return NotFound("Identity not found.");
                }
                return Ok(identity);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateIdentity([FromBody] IdentityRequest request)
        {
            try
            {                 
                if (request == null)
                {
                    return BadRequest("Invalid identity data.");
                }

                if (request.IdentityNumber.ToString().Length != 13)
                {
                    return BadRequest("Invalid identity number, must be 13 characters long.");
                }

                var res = request.isRealTimeHomeAffairsVerification
                    ? await _identityVerificationAgent.RealTimeIDVCheck(request.IdentityNumber)
                    : await _identityVerificationAgent.GetCachedProfileFromVendor(request.IdentityNumber);

                return Ok("Identity created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
            
        }

        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateBatch([FromBody] IList<Identity> identities)
        {
            //await _dbContext.Identities.AddRangeAsync(identities);
            //await _dbContext.SaveChangesAsync();

            return Ok("Identities created successfully");
        }
    }
}
