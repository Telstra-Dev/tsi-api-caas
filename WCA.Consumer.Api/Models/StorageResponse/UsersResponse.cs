namespace WCA.Consumer.Api.Models.StorageResponse
{
    public class UsersResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public Role Role { get; set; }
        public SiteUserMapping[] SiteUserMappings { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
    }

    public class SiteUserMapping
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public Site Site { get; set; }
    }

    public class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
