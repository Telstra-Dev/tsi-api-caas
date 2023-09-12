using Azure.Core;
using Microsoft.AspNetCore.Http;

namespace WCA.Consumer.Api.Helpers
{
    public static class TokenHelper
    {
        public static string GetToken(HttpContext context)
        {
            return context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        }
    }
}
