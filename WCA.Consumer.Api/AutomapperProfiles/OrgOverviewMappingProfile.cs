using AutoMapper;
using WCA.Consumer.Api.Models;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class OrgOverviewMappingProfile : Profile
    {
        public OrgOverviewMappingProfile()
        {
            CreateMap<Organisation, OrgSearchTreeNode>()
                .ForMember(dest => dest.Text, opts => opts.MapFrom(s => s.CustomerName))
                .ForMember(dest => dest.ParentId, opts => opts.MapFrom(s => s.Parent))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => "organisation"))
                .ForMember(dest => dest.Href, opts => opts.MapFrom(s => "/organisations?customerId=" + s.CustomerId))
                .ForMember(dest => dest.Status, opts => opts.Ignore());


            CreateMap<Site, OrgSearchTreeNode>()
                .ForMember(dest => dest.Text, opts => opts.MapFrom(s => s.Name))
                .ForMember(dest => dest.ParentId, opts => opts.MapFrom(s => s.OrganisationId))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(s => s.SiteId))
                //.ForMember(dest => dest.Status, opts => opts.MapFrom(s => s.Active))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => "site"))
                .ForMember(dest => dest.Href, opts => opts.MapFrom(s => "/sites?customerId=" + s.CustomerId))
                .ForMember(dest => dest.Status, opts => opts.Ignore());

            CreateMap<Device, OrgSearchTreeNode>()
                .ForMember(dest => dest.Text, opts => opts.MapFrom(s => s.Name))
                .ForMember(dest => dest.ParentId, opts => opts.MapFrom(s => getParentFromDevice(s)))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(s => s.DeviceId))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => s.Type.ToString()))
                .ForMember(dest => dest.Href, opts => opts.MapFrom(s => "/devices/" + s.DeviceId))
                .ForMember(dest => dest.Status, opts => opts.Ignore());
        }

        private int getParentFromDevice(Device device) {
            var parentId = device.SiteId;;
            if (device != null)
            {
                if (device.Type == DeviceType.camera.ToString())
                    parentId = device.EdgeDeviceId;
            }
            return parentId;
        }
    }
}
