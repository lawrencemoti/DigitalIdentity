namespace ApplicationCore.Models.API.Dtos.Webhook
{
    public class SubscriptionDtos
    {
        public record SubscribeRequest(long VendorId, string EventType);
        public record UnsubscribeRequest(long VendorId, string EventType);
        public record SubscriptionResponse(long SubscriptionId, long VendorId, string EventType);
    }
}
