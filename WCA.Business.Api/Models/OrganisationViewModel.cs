namespace WCA.Business.Api.Models
{
    public class OrganisationViewModel
    {
            public string CustomerId { get; set; }
            public string CustomerName { get; set; } 
            public string Parent {get; set;}
            public string? Id {get;set;}
            public string? Alias {get; set;}
            public long? CreatedAt {get; set;}
            public OrganisationViewModel[] Children {get; set;}

    }
}
