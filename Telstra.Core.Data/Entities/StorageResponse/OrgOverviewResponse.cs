using System.Collections.Generic;

namespace Telstra.Core.Data.Entities.StorageReponse
{
    public class OrgOverviewResponse : BaseResponse
    {
        public IList<OrgSearchTreeNode> Result { get; set; }
    }
}