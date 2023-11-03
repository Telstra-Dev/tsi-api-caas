using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ISerialNumberService
    {
        Task<SerialNumberModel> GetSerialNumberByValue(string authorisationEmail, string value);
        Task<IList<SerialNumberModel>> GetSerialNumbersByFilter(string authorisationEmail, string filter, bool inactiveOnly = false, uint? maxResults = null);
    }
}
