namespace WCA.Consumer.Api.Models
{
    public class Organisation
    {
            public string CustomerId { get; set; }
            public string CustomerName { get; set; } 
            public string Parent {get; set;}
            public string Id {get;set;}
            public string Alias {get; set;}
            public long? CreatedAt {get; set;}
            public Organisation[] Children {get; set;}

    }
}
