namespace WCA.Consumer.Api.Models.UMS
{
    public class UmsRolePermissionRequestModel
    {
        public Role_Names[] role_names { get; set; }
    }

    public class Role_Names
    {
        public string roleType { get; set; }
        public string roleName { get; set; }
    }

}
