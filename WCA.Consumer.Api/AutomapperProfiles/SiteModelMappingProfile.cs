using AutoMapper;
using WCA.Consumer.Api.Models;
using Telstra.Core.Data.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WCA.Consumer.Api.AutomapperProfiles
{
    public class SiteModelMappingProfile : Profile
    {
        public SiteModelMappingProfile()
        {
            CreateMap<SiteModel, Site>()
                .ForMember(dest => dest.StoreCode, opts => opts.MapFrom(s => s.Metadata.StoreCode))
                .ForMember(dest => dest.State, opts => opts.MapFrom(s => s.Metadata.State))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(s => s.Metadata.Type))
                .ForMember(dest => dest.StoreFormat, opts => opts.MapFrom(s => s.Metadata.StoreFormat))
                .ForMember(dest => dest.GeoClassification, opts => opts.MapFrom(s => s.Metadata.GeoClassification))
                .ForMember(dest => dest.Region, opts => opts.MapFrom(s => s.Metadata.Region))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(s => remapToEntityTags(s.Metadata.Tags, s.SiteId)))
                .ForMember(dest => dest.OrganisationId, opts => opts.MapFrom(s => s.CustomerId))
                .ForPath(dest => dest.Location.Id, opts => opts.MapFrom(s => s.Location.Id))
                .ForPath(dest => dest.Location.Address, opts => opts.MapFrom(s => s.Location.Address))
                .ForPath(dest => dest.Location.Latitude, opts => opts.MapFrom(s => s.Location.GeoLocation.Latitude))
                .ForPath(dest => dest.Location.Longitude, opts => opts.MapFrom(s => s.Location.GeoLocation.Longitude))
                .ForMember(dest => dest.SiteLocationId, opts => opts.Ignore())
                .ForMember(dest => dest.Organisation, opts => opts.Ignore());

            CreateMap<Site, SiteModel>()
                .ForPath(dest => dest.Metadata.StoreCode, opts => opts.MapFrom(s => s.StoreCode))
                .ForPath(dest => dest.Metadata.State, opts => opts.MapFrom(s => s.State))
                .ForPath(dest => dest.Metadata.Type, opts => opts.MapFrom(s => s.Type))
                .ForPath(dest => dest.Metadata.StoreFormat, opts => opts.MapFrom(s => s.StoreFormat))
                .ForPath(dest => dest.Metadata.GeoClassification, opts => opts.MapFrom(s => s.GeoClassification))
                .ForPath(dest => dest.Metadata.Region, opts => opts.MapFrom(s => s.Region))
                .ForPath(dest => dest.Metadata.Tags, opts => opts.MapFrom(s => remapToTagModel(s.Tags)))
                .ForPath(dest => dest.Location.Id, opts => opts.MapFrom(s => s.Location.Id))
                .ForPath(dest => dest.Location.Address, opts => opts.MapFrom(s => s.Location.Address))
                .ForPath(dest => dest.Location.GeoLocation.Latitude, opts => opts.MapFrom(s => s.Location.Latitude))
                .ForPath(dest => dest.Location.GeoLocation.Longitude, opts => opts.MapFrom(s => s.Location.Longitude));
        }

       private Dictionary<string, string[]> remapToTagModel(ICollection<SiteTag> origTags) {
            Dictionary<string, string[]> remappedTags = null;
            if (origTags != null && origTags.Count > 0)
            {         
                Dictionary<string, string[]> tags = new Dictionary<string, string[]>();
                foreach(var tagItem in origTags)
                {
                    if (!tags.ContainsKey(tagItem.TagName))
                    {
                        tags.Add(tagItem.TagName, new[] { tagItem.TagValue });
                    }
                    else
                    {
                        var newValue = tags[tagItem.TagName].ToList();
                        newValue.Add(tagItem.TagValue);
                        tags[tagItem.TagName] = newValue.ToArray();
                    }
                }
                remappedTags = tags;
            }
            return remappedTags;
        }

        private IList<SiteTag> remapToEntityTags(Dictionary<string, string[]> origTags, string siteId)
        {
            var mappedTags = new List<SiteTag>();

            foreach(var tagItem in origTags)
            {
                foreach (var tagItemValue in tagItem.Value)
                {
                    mappedTags.Add(
                        new SiteTag
                        {
                            SiteId = siteId,
                            TagName = tagItem.Key,
                            TagValue = tagItemValue
                        }
                    );
                }
            }
            return mappedTags;
        }
    }
}
