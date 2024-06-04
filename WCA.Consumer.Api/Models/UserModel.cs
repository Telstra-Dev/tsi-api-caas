using System;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class UserModelBase
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public string[] Roles { get; set; }
        public string Email { get; set; }
        public string LastActive { get; set; }
    }

    public class UpdateUserModel
    {
        public string Email { get; set; }
        public string[] Roles { get; set; }
        public List<int> Sites { get; set; }
    }

    public class UserModel : UserModelBase
    {
        public List<int> Sites { get; set; }
    }

    public class GetUserModel : UserModelBase
    {
        public UmsMetadata Meta { get; set; }
        public IList<UserSiteModel> Sites { get; set; }
    }

    public class UmsMetadata
    {
        public string UmsId { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public string Location { get; set; }
    }
}
