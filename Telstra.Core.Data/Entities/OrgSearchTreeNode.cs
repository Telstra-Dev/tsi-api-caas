namespace Telstra.Core.Data.Entities
{
    public class OrgSearchTreeNode
    {
            public string Id { get; set; }
            public string Type { get; set; } 
            public string Text {get; set;}
            public string ParentId {get;set;}
            public string Href {get; set;}
            public Status Status {get; set;}
    }
}
