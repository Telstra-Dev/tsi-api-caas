using System;
using Finbuckle.MultiTenant;

namespace Telstra.Core.Data
{
    public class MyTenentInfo : TenantInfo
    {
        // Add Aditional Properties Here
        public int CustomerId { get; set; }
    }
}
