using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net.Http;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading;

namespace WCA.Consumer.Api.Services
{
    public class SerialNumberService : ISerialNumberService
    {
        private readonly AppSettings _appSettings;
        private readonly IRestClient _httpClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public SerialNumberService(IRestClient httpClient,
                        AppSettings appSettings,
                        IMapper mapper,
                        ILogger<OrganisationService> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SerialNumberModel> GetSerialNumberByValue(string value)
        {
            SerialNumberModel serialNumberModel = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/serialNumbers?value={value}");
                var foundSerialNumber = await _httpClient.SendAsync<SerialNumber>(request, CancellationToken.None);

                if (foundSerialNumber != null)
                {
                    serialNumberModel = _mapper.Map<SerialNumberModel>(foundSerialNumber);
                }
                return serialNumberModel;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetSerialNumberByValue failed: {e.Message}");
                throw new Exception($"GetSerialNumberByValue failed: {e.Message}"); ;
            }
        }

        public async Task<IList<SerialNumberModel>> GetSerialNumbersByFilter(string filter, bool inactiveOnly = false, uint? maxResults = null)
        {
            IList<SerialNumberModel> serialNumberModels = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/serialNumbers?filter={filter}&inactiveOnly={inactiveOnly}&maxResults={maxResults}");
                var foundSerialNumbers = await _httpClient.SendAsync<IList<SerialNumber>>(request, CancellationToken.None);

                if (foundSerialNumbers != null && foundSerialNumbers.Count > 0)
                {
                    serialNumberModels = _mapper.Map<IList<SerialNumberModel>>(foundSerialNumbers);
                }
                return serialNumberModels;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetSerialNumbers failed: {e.Message}");
                throw new Exception($"GetSerialNumbers failed: {e.Message}"); ;
            }
        }

    }
}