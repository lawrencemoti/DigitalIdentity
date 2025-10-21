using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ApplicationCore.Contracts.Infrastructure;
using ApplicationCore.Models;
using ApplicationCore.Models.API.Dtos;
using ApplicationCore.Models.SQS;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using static ApplicationCore.Models.SQS.SqsMessages;

namespace IDV.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class IdentityController : ControllerBase
    {
        private readonly IdentityDBContext _db;
        private readonly IIdentityVerificationAgent _idvAgent;
        private readonly IAmazonSimpleNotificationService _sns;
        // SNS Topic ARN
        private readonly string snsTopicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")!;

        public IdentityController(IIdentityVerificationAgent identityVerificationAgent,
            IdentityDBContext db, IAmazonSimpleNotificationService sns)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _idvAgent = identityVerificationAgent ?? throw new ArgumentNullException(nameof(identityVerificationAgent));
            _sns = sns ?? throw new ArgumentNullException(nameof(sns));
        }

        [HttpPost("Retrieve")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIdentity([FromBody] long identityNumber)
        {
            try
            {
                if (identityNumber.ToString().Length != 13)
                {
                    return BadRequest("Invalid identity number, must be 13 characters long.");
                }

                var rec = await _db.Identities.FirstOrDefaultAsync(i => i.IdentityNumber == identityNumber);
                if (rec == null) return NotFound();                            
                
                return Ok(rec);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> CreateIdentity([FromBody] IdentityDto req)
        {
            try
            {                 
                if (req == null)
                    return BadRequest("Invalid identity data.");

                if (req.IdentityNumber.ToString().Length != 13)
                    return BadRequest("Invalid identity number, must be 13 characters long.");

                if (!Uri.IsWellFormedUriString(req.CallbackUrl, UriKind.Absolute))
                    return BadRequest("Invalid callback URL");

                var rec = new Identity
                {
                    IdentityNumber = req.IdentityNumber,
                    FirstName = req.FirstName ?? string.Empty,
                    Surname = req.Surname ?? string.Empty,
                    CallbackUrl = req.CallbackUrl
                };

                _db.Identities.Add(rec);
                await _db.SaveChangesAsync();

                // Enqueue message to SNS for background processing
                var msg = JsonSerializer.Serialize(
                    new IdentityMessage 
                    {
                        IdentityId = rec.Id,
                        IsVerificationWithDHA = req.isVerificationWithDHA,
                        ValidityPeriod = req.lastRequestValidityPeriod
                    });
                
                await _sns.PublishAsync(new PublishRequest { TopicArn = snsTopicArn, Message = msg });
                return Accepted($"/identity/{rec.Id}", 
                    new { trackId = rec.Id, status = rec.Status.ToString() });
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
