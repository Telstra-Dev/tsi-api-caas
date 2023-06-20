namespace WCA.Consumer.Api.Helpers
{
    public static class TokenClaimsHelper
    {
        public static string GetEmailFromToken(string token)
        {
            var emailClaim = "";

            if (IsAadB2cToken(token))
            {
                emailClaim = JwtExtractor.ExtractField(token, "email").ToLower();
            }
            else
            {
                emailClaim = JwtExtractor.ExtractField(token, "username").ToLower();
            }

            return emailClaim;
        }

        public static bool IsAadB2cToken(string token)
        {
            return JwtExtractor.ExtractField(token, "iss").ToLower().Contains("b2clogin");
        }
    }
}
