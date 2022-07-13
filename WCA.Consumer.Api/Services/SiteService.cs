using System.Collections.Generic;
using Telstra.Core.Repo;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Services
{
    public class SiteService : ISiteService
    {
        private MyMultitenantRepository _repo;
        public SiteService(MyMultitenantRepository Repo)
        {
            this._repo = Repo;
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
                    SiteLocationId = "AU/GEO/p0/12161",
                    SiteAddress = "Kellyville, Sydney, New South Wales",
                    GeoLocation = geoLocation
                };
                Site site = new Site {
                    SiteId = "187c1bdd-8efe-493d-b9c3-3a4f027e0940",
                    SiteName = "Kellyville",
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
            GeoLocation geoLocation = new GeoLocation {
                Latitude = -33.71218,
                Longitude = 150.95753
            };
            SiteLocation location = new SiteLocation {
                SiteLocationId = "AU/GEO/p0/12161",
                SiteAddress = "Kellyville, Sydney, New South Wales",
                GeoLocation = geoLocation
            };
            Site site = new Site {
                SiteId = "187c1bdd-8efe-493d-b9c3-3a4f027e0940",
                SiteName = "Kellyville",
                CustomerId = "manual-test-customer-id",
                Location = location,
                CreatedAt = 1651710340528
            };
            sites.Add(site);

            geoLocation = new GeoLocation {
                Latitude = -33.85754,
                Longitude = 151.181
            };
            location = new SiteLocation {
                SiteLocationId = "AU/GEO/p0/11073",
                SiteAddress = "Balmain, Sydney, New South Wales",
                GeoLocation = geoLocation
            };
            site = new Site {
                SiteId = "91126d4d-c12b-40a3-a58b-9fbdda73e9e5",
                SiteName = "telstra-balmain-design-centre",
                CustomerId = "manual-test-customer-id",
                Location = location,
                CreatedAt = 1651710480031
            };
            sites.Add(site);

            geoLocation = new GeoLocation {
                Latitude = -34.41835,
                Longitude = 150.90239
            };
            location = new SiteLocation {
                SiteLocationId = "AU/STR/p0/375083",
                SiteAddress = "Cliff Road, WollongongNew South Wales",
                GeoLocation = geoLocation
            };
            site = new Site {
                SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
                SiteName = "Blue Mile Area Wollongong",
                CustomerId = "manual-test-customer-id",
                Location = location,
                CreatedAt = 1655347904941
            };
            sites.Add(site);
            return sites;
        }
    }
}