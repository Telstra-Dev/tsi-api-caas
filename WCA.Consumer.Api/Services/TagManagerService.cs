using AutoMapper;
using Azure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telstra.Common;
using WCA.Consumer.Api.Helpers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Services
{
    public class TagManagerService : ITagManagerService
    {
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime;

        private List<string> _validCategories = new List<string> 
        {
            "region", "type", "large area", "crossing", "dwell area"
        };

        private List<string> _validTypes = new List<string>
        {
            "site", "trip line", "polygon"
        };

        public TagManagerService(IRestClient httpClient,
                                    AppSettings appSettings,
                                    IMapper mapper,
                                    ILogger<TagManagerService> logger,
                                    IMemoryCache cache)
        {
            _appSettings = appSettings;
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;

            _shortCacheTime = DateTimeOffset.Now.AddSeconds(_appSettings.ShortCacheTime);
        }

        public async Task<List<TagModel>> GetTagsAsync(string authorisationEmail)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/tagmanager/{authorisationEmail}");
                var response = await _httpClient.SendWithResponseAsync(request, CancellationToken.None);
                var reply = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var tags = JsonConvert.DeserializeObject<List<TagModel>>(reply);
                    return tags;
                }
                else
                {
                    _logger.LogError($"Fetch tags failed with error: {reply}");
                    throw new Exception($"Error fetching tags. {response.StatusCode} Response code from downstream {reply}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetTagsAsync: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<int> CreateTagsAsync(string authorisationEmail, List<CreateTagModel> tags)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var invalidTag = tags.Where(t => !_validCategories.Contains(t.Category) 
                                            || !_validTypes.Contains(t.Type))
                                        .FirstOrDefault();

                if (invalidTag != null) 
                {
                    var invalidTagError = "Create tag failed with error: Invalid values.";
                    _logger.LogError(invalidTagError);
                    throw new Exception(invalidTagError);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_appSettings.StorageAppHttp.BaseUri}/tagmanager/{authorisationEmail}");
                request.Content = new StringContent(JsonConvert.SerializeObject(tags), Encoding.UTF8, "application/json");
                var response = await _httpClient.SendWithResponseAsync(request, CancellationToken.None);
                var reply = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var createdTagsCount = JsonConvert.DeserializeObject<int>(reply);
                    return createdTagsCount;
                }
                else
                {
                    _logger.LogError($"Create tag failed with error: {reply}");
                    throw new Exception($"Error creating tags. {response.StatusCode} Response code from downstream {reply}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CreateTagsAsync: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<TagModel> RenameTagAsync(string authorisationEmail, TagModel tag)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_appSettings.StorageAppHttp.BaseUri}/tagmanager/{authorisationEmail}");
                request.Content = new StringContent(JsonConvert.SerializeObject(tag), Encoding.UTF8, "application/json");
                var response = await _httpClient.SendWithResponseAsync(request, CancellationToken.None);
                var reply = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var renamedTag = JsonConvert.DeserializeObject<TagModel>(reply);
                    return renamedTag;
                }
                else
                {
                    _logger.LogError($"Rename tag failed with error: {reply}");
                    throw new Exception($"Error renaming tag. {response.StatusCode} Response code from downstream {reply}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("RenameTagAsync: " + e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}
