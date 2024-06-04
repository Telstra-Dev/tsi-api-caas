namespace WCA.Consumer.Api.Models.UMS
{
    public class UmsRolePermissionResponseModel
    {
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public string roleType { get; set; }
        public string roleName { get; set; }
        public string[] permissions { get; set; }
    }

}
