using AutoMapper;
using Telstra.Core.Data.Entities;
using WCA.Storage.Api.Proto;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class OrgOverviewMappingProfile : Profile
    {
        public OrgOverviewMappingProfile()
        {
            CreateMap<Organisation, OrgSearchTreeNode>()
                .ForMember(dest => dest.Text, opts => opts.MapFrom(s => s.CustomerName))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => "organisation"))
                .ForMember(dest => dest.Href, opts => opts.MapFrom(s => "/organisations?customer=" + s.CustomerId));

        }
    }
}
