using System.Collections.Generic;
using Telstra.Core.Repo;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Services
{
    public class SiteService : ISiteService
    {
        public SiteService()
        {
        }

        public IList<Site> GetSitesForCustomer(string? customerId)
        {
            IList<Site> sites = new List<Site>();
            return GetAllSites();
        }

        public Site GetSite(string? siteId)
        {
            if (siteId != null)
            {
                GeoLocation geoLocation = new GeoLocation {
                    Latitude = -33.71218,
                    Longitude = 150.95753
                };
                SiteLocation location = new SiteLocation {
                    Id = "AU/GEO/p0/12161",
                    Address = "Kellyville, Sydney, New South Wales",
                    GeoLocation = geoLocation
                };
                Site site = new Site {
                    SiteId = "187c1bdd-8efe-493d-b9c3-3a4f027e0940",
                    Name = "Kellyville",
                    CustomerId = "manual-test-customer-id",
                    Location = location,
                    CreatedAt = 1651710340528
                };
                return site;
            }
            else
            {
                return null;
            }
        }

        private IList<Site> GetAllSites()
        {
            IList<Site> sites = new List<Site>();

            Site site = CreateSite(-33.71218, 150.95753, "AU/GEO/p0/12161", "John Scott Park, Samford Village QLD",
                                "187c1bdd-8efe-493d-b9c3-3a4f027e0940", "John Scott Park", "manual-test-customer-id", 1651710340528);
            
            sites.Add(site);

            site = CreateSite(-33.85754, 151.181, "AU/GEO/p0/11073", "Balmain, Sydney, New South Wales",
                                "91126d4d-c12b-40a3-a58b-9fbdda73e9e5", "telstra-balmain-design-centre", "manual-test-customer-id", 1651710480031);
            
            sites.Add(site);

            site = CreateSite(-34.41835, 150.90239, "AU/STR/p0/375083", "Cliff Road, Wollongong, New South Wales",
                                "bceead95-5b9d-47bc-9d93-4740db6c1292", "Blue Mile Area Wollongong", "manual-test-customer-id", 1655347904941);
            
            sites.Add(site);
            return sites;
        }

        public Site CreateSite(Site site)
        {
            return site;
        }

        public Site UpdateSite(string siteId, Site site)
        {
            return site;
        }

        public Site DeleteSite(string siteId)
        {
            Site site = CreateSite(-33.71218, 150.95753, "AU/GEO/p0/12161", "Kellyville, Sydney, New South Wales",
                                "187c1bdd-8efe-493d-b9c3-3a4f027e0940", "Kellyville", "manual-test-customer-id", 1651710340528);
            return site;
        }

        private Site CreateSite(double latitude, double longitude, string siteLocationId, string siteAddress,
                                string siteId, string siteName, string customerId, long createdAt)
        {
            GeoLocation geoLocation = new GeoLocation {
                Latitude = latitude,
                Longitude = longitude
            };
            SiteLocation location = new SiteLocation {
                Id = siteLocationId,
                Address = siteAddress,
                GeoLocation = geoLocation
            };
            Site site = new Site {
                SiteId = siteId,
                Name = siteName,
                CustomerId = customerId,
                Location = location,
                CreatedAt = createdAt
            };
            return site;
        }
    }
}