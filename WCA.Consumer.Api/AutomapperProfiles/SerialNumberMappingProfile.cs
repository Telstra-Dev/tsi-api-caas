using AutoMapper;
using WCA.Consumer.Api.Models;
// using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class SerialNumberMappingProfile : Profile
    {
        public SerialNumberMappingProfile()
        {
            CreateMap<SerialNumberModel, string>()
                .ConvertUsing(s => s.Value);

            CreateMap<string, SerialNumberModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(s => s));
        }
    }
}
