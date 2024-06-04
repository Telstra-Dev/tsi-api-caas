using System;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class UsersModel
    {
        public DateTime LastActive { get; set; } = DateTime.Now;
        public string Permission { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<UserSites> Sites { get; set; }
    }

    public class UserSites
    {
        public string SiteName { get; set; }
    }
}
