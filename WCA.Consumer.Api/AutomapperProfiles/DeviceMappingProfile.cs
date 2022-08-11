using AutoMapper;
using WCA.Consumer.Api.Models;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class DeviceMappingProfile : Profile
    {
        public DeviceMappingProfile()
        {
            CreateMap<Gateway, Telstra.Core.Data.Entities.Device>()
                .ForMember(dest => dest.EdgeDeviceId, opts => opts.MapFrom(s => s.EdgeDevice))
                .ForMember(dest => dest.MetadataUrl, opts => opts.MapFrom(s => s.Metadata.Hub))
                .ForMember(dest => dest.IsEdgeCapable, opts => opts.MapFrom(s => s.EdgeCapable))
                .ForMember(dest => dest.IsActive, opts => opts.MapFrom(s => s.Active))
                .ForMember(dest => dest.MetadataHub, opts => opts.MapFrom(s => s.Metadata.Hub))
                .ForMember(dest => dest.MetadataAuthConnString, opts => opts.MapFrom(s => s.Metadata.Auth.IotHubConnectionString))
                .ForMember(dest => dest.MetadataAuthSymmetricKey, opts => opts.MapFrom(s => s.Metadata.Auth.SymmetricKey.PrimaryKey));
            
            CreateMap<Device, Gateway>()
                .ForMember(dest => dest.EdgeDevice, opts => opts.MapFrom(s => s.EdgeDeviceId))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => s.Type.ToString()))
                .ForMember(dest => dest.EdgeCapable, opts => opts.MapFrom(s => s.IsEdgeCapable))
                .ForMember(dest => dest.Active, opts => opts.MapFrom(s => s.IsActive))
                .ForPath(dest => dest.Metadata.Hub, opts => opts.MapFrom(s => s.MetadataHub))
                .ForPath(dest => dest.Metadata.Auth.IotHubConnectionString, opts => opts.MapFrom(s => s.MetadataAuthConnString))
                .ForPath(dest => dest.Metadata.Auth.SymmetricKey.PrimaryKey, opts => opts.MapFrom(s => s.MetadataAuthSymmetricKey));

            CreateMap<Camera, Device>()
                .ForMember(dest => dest.EdgeDeviceId, opts => opts.MapFrom(s => s.EdgeDevice))
                .ForMember(dest => dest.MetadataUrl, opts => opts.MapFrom(s => s.Metadata.Hub))
                .ForMember(dest => dest.IsEdgeCapable, opts => opts.MapFrom(s => s.EdgeCapable))
                .ForMember(dest => dest.IsActive, opts => opts.MapFrom(s => s.Active))
                .ForMember(dest => dest.MetadataUrl, opts => opts.MapFrom(s => s.Metadata.Url))
                .ForMember(dest => dest.MetadataUsername, opts => opts.MapFrom(s => s.Metadata.Username))
                .ForMember(dest => dest.MetadataPassword, opts => opts.MapFrom(s => s.Metadata.Password))
                .ForMember(dest => dest.MetadataHub, opts => opts.MapFrom(s => s.Metadata.Hub))
                .ForMember(dest => dest.MetadataAuthConnString, opts => opts.MapFrom(s => s.Metadata.Auth.IotHubConnectionString))
                .ForMember(dest => dest.MetadataAuthSymmetricKey, opts => opts.MapFrom(s => s.Metadata.Auth.SymmetricKey.PrimaryKey));
            
            CreateMap<Device, Camera>()
                .ForMember(dest => dest.EdgeDevice, opts => opts.MapFrom(s => s.EdgeDeviceId))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => s.Type.ToString()))
                .ForMember(dest => dest.EdgeCapable, opts => opts.MapFrom(s => s.IsEdgeCapable))
                .ForMember(dest => dest.Active, opts => opts.MapFrom(s => s.IsActive))
                .ForPath(dest => dest.Metadata.Url, opts => opts.MapFrom(s => s.MetadataUrl))
                .ForPath(dest => dest.Metadata.Username, opts => opts.MapFrom(s => s.MetadataUsername))
                .ForPath(dest => dest.Metadata.Password, opts => opts.MapFrom(s => s.MetadataPassword))
                .ForPath(dest => dest.Metadata.Hub, opts => opts.MapFrom(s => s.MetadataHub))
                .ForPath(dest => dest.Metadata.Auth.IotHubConnectionString, opts => opts.MapFrom(s => s.MetadataAuthConnString))
                .ForPath(dest => dest.Metadata.Auth.SymmetricKey.PrimaryKey, opts => opts.MapFrom(s => s.MetadataAuthSymmetricKey));
        }
    }
}
