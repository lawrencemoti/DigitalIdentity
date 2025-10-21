using ApplicationCore.Models.Webhook;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static ApplicationCore.Models.API.Dtos.Webhook.SubscriptionDtos;
using static ApplicationCore.Models.API.Dtos.Webhook.VendorDtos;

namespace IDV.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        [HttpPost("vendor/register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterVendor([FromBody] RegisterVendorRequest req)
        {
            if (!Uri.IsWellFormedUriString(req.CallbackUrl, UriKind.Absolute))
                return BadRequest("Invalid callback URL");

            var v = new Vendor
            {
                Name = req.Name,
                CallbackUrl = req.CallbackUrl,
                //SecretHash = _sig.HashSecret(req.Secret),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            //TODO: Save vendor to database
            //_db.Vendors.Add(v);
            //await _db.SaveChangesAsync();

            // Implementation for registering a vendor for webhooks
            return Ok(new {VendorId = v.Id});
        }

        [HttpDelete("vendor/vendorId")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeregisterVendor(long vendorId)
        {
            //var v = await _db.Vendors.FindAsync(vendorId);
            //if (v == null) return NotFound();
            //v.IsActive = false;
            //v.UpdatedAt = DateTime.UtcNow;
            //await _db.SaveChangesAsync();

            // Implementation for unregistering a vendor
            return Ok("Vendor deregistered successfully");
        }

        [HttpPost("subscription/subscribe")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<SubscriptionResponse>> Subscribe(SubscribeRequest req)
        {
            if (!EventTypes.IsValid(req.EventType)) return BadRequest("Unknown event type");
            //var v = await _db.Vendors.FindAsync(req.VendorId);
            
            //if (v == null || !v.IsActive) return NotFound("Vendor not found or inactive");


            //var sub = new Subscription 
            //{ 
            //  VendorId = req.VendorId, 
            //  EventType = req.EventType.ToUpperInvariant(), 
            // CreatedAt = DateTime.Now 
            //};

            //_db.Subscriptions.Add(sub);
            //await _db.SaveChangesAsync();
            //return Ok(new SubscriptionResponse(sub.Id, sub.VendorId, sub.EventType));
            return Ok();
        }

        [HttpDelete("subscription/unsubscribe")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Unsubscribe(UnsubscribeRequest req)
        {
            //var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.VendorId == req.VendorId && s.EventType == req.EventType);
            
            //if (sub == null) return NotFound();
            //_db.Subscriptions.Remove(sub);
            
            //await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
