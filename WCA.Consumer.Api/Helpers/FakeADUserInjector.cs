using System;
using System.Security.Claims;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Telstra.Consumer.Api.Extensions;
using Telstra.Core.Data;

namespace Telstra.Consumer.Api.Helpers
{
    
    public class FakeADUserInjector
    {
        private const int DEFAULT_TENANT_CUSTOMER_ID = 1;
        private const string DEFAULT_TENANT_NAME = "Telstra";
        private const string DEFAULT_TENANT_IDENTIFIER = "TELSTRA";
        private const string DEFAULT_TENANT_ID = "tenant-telstra-d043a056";
        private const string IDENTITY_CLAIM_TYPE_EMAILS = "emails";
        private const string IDENTITY_CLAIM_TYPE_NAME = "name";
        private const string IDENTITY_CLAIM_TYPE_OID = "oid";

        
        public void Inject(HttpContext actionContext)
        {
            // Inject a fake tenant for use
            var tenantInfo = new MyTenentInfo();
            tenantInfo.CustomerId = DEFAULT_TENANT_CUSTOMER_ID;
            tenantInfo.Name = DEFAULT_TENANT_NAME;
            tenantInfo.Identifier = DEFAULT_TENANT_IDENTIFIER;
            tenantInfo.Id = DEFAULT_TENANT_ID;

            actionContext.TrySetTenantInfo(tenantInfo, true);

            // Inject a fake principal/user for use
            var fakeClaimsPrincipal = new ClaimsPrincipal();
            var fakeIdentity = new ClaimsIdentity();

            if (EnvironmentExtensions.fakeADUserEmail() != null)
                fakeIdentity.AddClaim(new Claim(IDENTITY_CLAIM_TYPE_EMAILS, EnvironmentExtensions.fakeADUserEmail()));
            if (EnvironmentExtensions.fakeADUserName() != null)
                fakeIdentity.AddClaim(new Claim(IDENTITY_CLAIM_TYPE_NAME, EnvironmentExtensions.fakeADUserName()));
            if (EnvironmentExtensions.fakeADUserOid() != null)
                fakeIdentity.AddClaim(new Claim(IDENTITY_CLAIM_TYPE_OID, EnvironmentExtensions.fakeADUserOid()));

            fakeClaimsPrincipal.AddIdentity(fakeIdentity);
            actionContext.User = fakeClaimsPrincipal;
        }
    }
}
