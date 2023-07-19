using System;

namespace WCA.Consumer.Api.Helpers
{
    public static class TokenClaimsHelper
    {
        public static string GetEmailFromToken(string token)
        {
            try
            {
                var emailClaim = string.Empty;

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
            catch (Exception ex)
            {
                throw new NullReferenceException("Cannot get valid claim from token. " + ex.Message);
            }
        }

        public static bool IsAadB2cToken(string token)
        {
            return JwtExtractor.ExtractField(token, "iss").ToLower().Contains("b2clogin");
        }
    }
}
