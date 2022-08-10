namespace WCA.Consumer.Api.Models.AzureMapsResponse
{
    public class AuthToken
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string expires_on { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }
        public string ms_client_id { get; set; }
    }
}