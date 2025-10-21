namespace ApplicationCore.Models.API.Dtos.Webhook
{
    public class VendorDtos
    {
        public record RegisterVendorRequest(string Name, string CallbackUrl, string Secret);
    }
}
