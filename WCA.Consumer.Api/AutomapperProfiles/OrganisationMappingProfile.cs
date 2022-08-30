using AutoMapper;
using WCA.Consumer.Api.Models;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class OrganisationMappingProfile : Profile
    {
        public OrganisationMappingProfile()
        {
            CreateMap<Organisation, OrganisationModel>();
                // .ForMember(dest => dest.Id, opts => opts.MapFrom(s => s.Id))
                // .ForMember(dest => dest.CustomerId, opts => opts.MapFrom(s => s.CustomerId))
                // .ForMember(dest => dest.CustomerName, opts => opts.MapFrom(s => s.CustomerName))
                // .ForMember(dest => dest.Alias, opts => opts.MapFrom(s => s.Alias))
                // .ForMember(dest => dest.CreatedAt, opts => opts.MapFrom(s => s.CreatedAt));
        }
    }
}
