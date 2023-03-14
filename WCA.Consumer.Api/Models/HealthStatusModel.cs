namespace WCA.Consumer.Api.Models
{
    public class HealthStatusModel
    {
        public HealthStatusCode Code { get; set; }
        public string Reason { get; set; }
        public string Action { get; set; }
    }
}