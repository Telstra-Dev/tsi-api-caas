using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Identity.Web;
using Telstra.Common;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Api.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public const string NameKey = "Name";
        public const string EmailIdKey = "Email";
        public const string ObjectIdKey = "OID";
        public const string IdpShortKey = "idp";
        public const string IdpLongKey = "http://schemas.microsoft.com/identity/claims/identityprovider";

        public static PrincipalKey GetUniqueIdentifierId(this ClaimsPrincipal claimsPrincipal)
        {
            Guard.NotNull(claimsPrincipal, nameof(claimsPrincipal));

            var uniqueValue = claimsPrincipal.GetEmail();
            var uniqueValueKey = EmailIdKey;

            if (string.IsNullOrEmpty(uniqueValue))
            {
                uniqueValue = claimsPrincipal.FindFirstValue(ClaimConstants.Name);
                uniqueValueKey = NameKey;
            }

            if (string.IsNullOrEmpty(uniqueValue))
            {
                uniqueValue = claimsPrincipal.GetObjectId();
                uniqueValueKey = ObjectIdKey;
            }

            if (string.IsNullOrEmpty(uniqueValue))
            {
                uniqueValueKey = null;
            }

            return new PrincipalKey(uniqueValue, uniqueValueKey);
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            var emailClaims = principal.Claims.Where(claim => claim.Type == "emails").ToList();
            return emailClaims.Any() ? emailClaims.First().Value.ToLower() : string.Empty;
        }

        /// <summary>
        /// Gets the identity provider associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> from which to retrieve the identity provider.</param>
        /// <returns>Identity provider of the identity, or <c>null</c> if it cannot be found.</returns>
        public static string GetIdpValue(this ClaimsPrincipal principal)
        {
            Guard.NotNull(principal, nameof(principal));

            var idpClaims = principal.Claims.Where(claim => claim.Type.ToLower() == IdpShortKey);
            if (idpClaims.Any())
            {
                return idpClaims.First().Value;
            }

            idpClaims = principal.Claims.Where(claim => claim.Type.ToLower() == IdpLongKey);
            if (idpClaims.Any())
            {
                return idpClaims.First().Value;
            }

            return null;
        }
    }
}
