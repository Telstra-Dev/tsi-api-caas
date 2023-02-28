namespace WCA.Consumer.Api.Helpers
{
    public static class TokenClaimsHelper
    {
        public static string GetEmailFromToken(string token)
        {
            // email format: [project suffix]-[user name]@telstrasmartspacesdemo.onmicrosoft.com
            // or [user name]@team.telstra.com
            var emailClaim = JwtExtractor.ExtractField(token, "email").ToLower();

            return emailClaim;
        }
    }
}
