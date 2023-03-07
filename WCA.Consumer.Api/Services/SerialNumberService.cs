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

namespace WCA.Consumer.Api.Services
{
    public class SerialNumberService : ISerialNumberService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public SerialNumberService(HttpClient httpClient,
                        AppSettings appSettings, 
                        IMapper mapper, 
                        ILogger<OrganisationService> logger)
        {
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<SerialNumberModel> GetSerialNumberByValue(string value)
        {
            SerialNumberModel serialNumberModel = null;
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/serialNumbers?value={value}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var foundSerialNumber = JsonConvert.DeserializeObject<SerialNumber>(reply);
                    if (foundSerialNumber != null)
                    {
                        serialNumberModel = _mapper.Map<SerialNumberModel>(foundSerialNumber);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    _logger.LogError("GetSerialNumberByValue failed with error: " + reply);
                    throw new Exception($"Error getting serial number. {response.StatusCode} Response code from downstream: " + reply);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetSerialNumberByValue: " + e.Message);
                throw new Exception(e.Message); ;
            }
            return serialNumberModel;
        }

        public async Task<IList<SerialNumberModel>> GetSerialNumbersByFilter(string filter, bool inactiveOnly = false, uint? maxResults = null)
        {
            IList<SerialNumberModel> serialNumberModels = null;
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/serialNumbers?filter={filter}&inactiveOnly={inactiveOnly}&maxResults={maxResults}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var foundSerialNumbers = JsonConvert.DeserializeObject<IList<SerialNumber>>(reply);
                    if (foundSerialNumbers != null && foundSerialNumbers.Count > 0)
                    {
                        serialNumberModels = _mapper.Map<IList<SerialNumberModel>>(foundSerialNumbers);
                    }
                }
                else
                {
                    _logger.LogError("GetSerialNumbersByFilter failed with error: " + reply);
                    throw new Exception($"Error getting serial number. {response.StatusCode} Response code from downstream: " + reply);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetSerialNumbersByFilter: " + e.Message);
                throw new Exception(e.Message); ;
            }
            return serialNumberModels;
        }

    }
}