using System.Collections.Generic;

namespace Telstra.Core.Data.Entities.StorageReponse
{
    public class CustomerResponse : BaseResponse
    {
        public IList<Customer> Result { get; set; }
    }
}