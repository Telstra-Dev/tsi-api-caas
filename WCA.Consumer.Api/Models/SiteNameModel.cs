﻿using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class SiteNameModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<int> Tags { get; set; }
        public SiteAddress Address { get; set; }
    }

    public class SiteAddress
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string SvNote { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
