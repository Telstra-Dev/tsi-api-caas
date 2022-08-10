using System.Collections.Generic;

namespace WCA.Consumer.Api.Models.StorageReponse
{
    public class OrgOverviewResponse : BaseResponse
    {
        public IList<OrgSearchTreeNode> Result { get; set; }
    }
}