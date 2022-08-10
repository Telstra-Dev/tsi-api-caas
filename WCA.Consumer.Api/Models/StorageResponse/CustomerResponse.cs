using System.Collections.Generic;

namespace WCA.Consumer.Api.Models.StorageReponse
{
    public class CustomerResponse : BaseResponse
    {
        public IList<Customer> Result { get; set; }
    }
}