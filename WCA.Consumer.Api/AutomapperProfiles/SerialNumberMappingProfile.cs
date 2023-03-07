using AutoMapper;
using WCA.Consumer.Api.Models;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class SerialNumberMappingProfile : Profile
    {
        public SerialNumberMappingProfile()
        {
            CreateMap<SerialNumberModel, SerialNumber>();

            CreateMap<SerialNumber, SerialNumberModel>();
        }
    }
}
