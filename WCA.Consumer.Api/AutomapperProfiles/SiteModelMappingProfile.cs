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
