using AzureIoT.DigitalTwins.Management.Models;
using AzureIoT.DigitalTwins.Management.Providers;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Management.API
{
    public class DigitalTwinsApiConnector : IDigitalTwinsApiConnector
    {
        private HttpClient _httpClient;
        private ILogger _logger;
        private IAccessTokenProvider _accessTokenProvider;
        private IConfigurationProvider _configurationProvider;
        public DigitalTwinsApiConnector(ILogger logger,
            IAccessTokenProvider accessTokenProvider,
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _accessTokenProvider = accessTokenProvider;
            _configurationProvider = configurationProvider;
            _httpClient = new HttpClient() { BaseAddress = new Uri(_configurationProvider.AzureDigitalTwinsApiUrl) };
        }

        public async Task<Guid> CreateDevice(DeviceCreate deviceCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Device: {JsonConvert.SerializeObject(deviceCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(deviceCreate);
            var response = await _httpClient.PostAsync("devices", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateEndpoints(EndpointsCreate endpointCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Endpoint: {JsonConvert.SerializeObject(endpointCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(endpointCreate);
            var response = await _httpClient.PostAsync("endpoints", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateKeyStore(KeyStoreCreate keyStoreCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating KeyStore: {JsonConvert.SerializeObject(keyStoreCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(keyStoreCreate);
            var response = await _httpClient.PostAsync("keystores", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateMatcher(MatcherCreate matcherCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Matcher: {JsonConvert.SerializeObject(matcherCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(matcherCreate);
            var response = await _httpClient.PostAsync("matchers", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateResource(ResourceCreate resourceCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Resource: {JsonConvert.SerializeObject(resourceCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(resourceCreate);
            var response = await _httpClient.PostAsync("resources", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateRoleAssignment(RoleAssignmentCreate roleAssignmentCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating RoleAssignment: {JsonConvert.SerializeObject(roleAssignmentCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(roleAssignmentCreate);
            var response = await _httpClient.PostAsync("roleassignments", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateSensor(SensorCreate sensorCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Sensor: {JsonConvert.SerializeObject(sensorCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(sensorCreate);
            var response = await _httpClient.PostAsync("sensors", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Guid> CreateSpace(SpaceCreate spaceCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Space: {JsonConvert.SerializeObject(spaceCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(spaceCreate);
            var response = await _httpClient.PostAsync("spaces", new StringContent(content, Encoding.UTF8, "application/json"));
            return await GetIdFromResponse(response, _logger);
        }

        public async Task CreateProperty(Guid spaceId, PropertyCreate propertyCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating Property: {JsonConvert.SerializeObject(propertyCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(propertyCreate);
            var response = await _httpClient.PostAsync($"spaces/{spaceId.ToString()}/properties", new StringContent(content, Encoding.UTF8, "application/json"));
            _logger.Information($"Creating Property Response: {response}");
        }

        public async Task CreatePropertyKey(PropertyKeyCreate propertyKeyCreate)
        {
            SetupHttpClient();
            _logger.Information($"Creating PropertyKey: {JsonConvert.SerializeObject(propertyKeyCreate, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(propertyKeyCreate);
            var response = await _httpClient.PostAsync($"propertykeys", new StringContent(content, Encoding.UTF8, "application/json"));
            _logger.Information($"Creating PropertyKey Response: {response}");
        }

        public async Task<Guid> CreateUserDefinedFunction(
            UserDefinedFunctionCreate userDefinedFunctionCreate,
            string js)
        {
            SetupHttpClient();
            _logger.Information($"Creating UserDefinedFunction with Metadata: {JsonConvert.SerializeObject(userDefinedFunctionCreate, Formatting.Indented)}");
            var displayContent = js.Length > 100 ? js.Substring(0, 100) + "..." : js;
            _logger.Information($"Creating UserDefinedFunction with Content: {displayContent}");

            var metadataContent = new StringContent(JsonConvert.SerializeObject(userDefinedFunctionCreate), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var multipartContent = new MultipartFormDataContent("userDefinedFunctionBoundary");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(new StringContent(js), "contents");

            var response = await _httpClient.PostAsync("userdefinedfunctions", multipartContent);
            return await GetIdFromResponse(response, _logger);
        }

        public async Task<Device> FindDevice(
          string hardwareId,
          Guid? spaceId,
          string includes = null)
        {
            SetupHttpClient();
            var filterHardwareIds = $"hardwareIds={hardwareId}";
            var filterSpaceId = spaceId != null ? $"&spaceIds={spaceId.ToString()}" : "";
            var includesParam = includes != null ? $"&includes={includes}" : "";
            var filter = $"{filterHardwareIds}{filterSpaceId}{includesParam}";

            var response = await _httpClient.GetAsync($"devices?{filter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var devices = JsonConvert.DeserializeObject<IReadOnlyCollection<Device>>(content);
                var matchingDevice = devices.SingleOrDefault();
                if (matchingDevice != null)
                {
                    _logger.Information($"Retrieved Unique Device using 'hardwareId' and 'spaceId': {JsonConvert.SerializeObject(matchingDevice, Formatting.Indented)}");
                    return matchingDevice;
                }
            }
            return null;
        }

        // Returns a matcher with same name and spaceId if there is exactly one.
        // Otherwise returns null.
        public async Task<IEnumerable<Matcher>> FindMatchers(IEnumerable<string> names, Guid spaceId)
        {
            SetupHttpClient();
            var commaDelimitedNames = names.Aggregate((string acc, string s) => acc + "," + s);
            var filterNames = $"names={commaDelimitedNames}";
            var filterSpaceId = $"&spaceIds={spaceId.ToString()}";
            var filter = $"{filterNames}{filterSpaceId}";

            var response = await _httpClient.GetAsync($"matchers?{filter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var matchers = JsonConvert.DeserializeObject<IReadOnlyCollection<Matcher>>(content);
                if (matchers != null)
                {
                    _logger.Information($"Retrieved Unique Matchers using 'names' and 'spaceId': {JsonConvert.SerializeObject(matchers, Formatting.Indented)}");
                    return matchers;
                }
            }
            return null;
        }

        // Returns a space with same name and parentId if there is exactly one
        // that maches that criteria. Otherwise returns null.
        public async Task<Space> FindSpace(string name, Guid parentId)
        {
            SetupHttpClient();
            var filterName = $"Name eq '{name}'";
            var filterParentSpaceId = parentId != Guid.Empty
                ? $"ParentSpaceId eq guid'{parentId}'"
                : $"ParentSpaceId eq null";
            var odataFilter = $"$filter={filterName} and {filterParentSpaceId}";

            var response = await _httpClient.GetAsync($"spaces?{odataFilter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var spaces = JsonConvert.DeserializeObject<IReadOnlyCollection<Space>>(content);
                var matchingSpace = spaces.SingleOrDefault();
                if (matchingSpace != null)
                {
                    _logger.Information($"Retrieved Unique Space using 'name' and 'parentSpaceId': {JsonConvert.SerializeObject(matchingSpace, Formatting.Indented)}");
                    return matchingSpace;
                }
            }
            return null;
        }

        // Returns a user defined fucntion with same name and spaceId if there is exactly one.
        // Otherwise returns null.
        public async Task<UserDefinedFunction> FindUserDefinedFunction(string name, Guid spaceId)
        {
            SetupHttpClient();
            var filterNames = $"names={name}";
            var filterSpaceId = $"&spaceIds={spaceId.ToString()}";
            var filter = $"{filterNames}{filterSpaceId}";

            var response = await _httpClient.GetAsync($"userdefinedfunctions?{filter}&includes=matchers");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userDefinedFunctions = JsonConvert.DeserializeObject<IReadOnlyCollection<UserDefinedFunction>>(content);
                var userDefinedFunction = userDefinedFunctions.SingleOrDefault();
                if (userDefinedFunction != null)
                {
                    _logger.Information($"Retrieved Unique UserDefinedFunction using 'name' and 'spaceId': {JsonConvert.SerializeObject(userDefinedFunction, Formatting.Indented)}");
                    return userDefinedFunction;
                }
            }
            return null;
        }


        public async Task<IEnumerable<Ontology>> GetOntologies()
        {
            SetupHttpClient();
            var response = await _httpClient.GetAsync($"ontologies");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ontologies = JsonConvert.DeserializeObject<IEnumerable<Ontology>>(content);
                _logger.Information($"Retrieved Ontologies: {JsonConvert.SerializeObject(ontologies, Formatting.Indented)}");
                return ontologies;
            }
            else
            {
                return Array.Empty<Ontology>();
            }
        }

        public async Task<IEnumerable<PropertyKeys>> GetPropertyKeys()
        {
            SetupHttpClient();
            var response = await _httpClient.GetAsync($"propertykeys");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var propertyKeys = JsonConvert.DeserializeObject<IEnumerable<PropertyKeys>>(content);
                _logger.Information($"Retrieved PropertyKeys: {JsonConvert.SerializeObject(propertyKeys, Formatting.Indented)}");
                return propertyKeys;
            }
            else
            {
                return Array.Empty<PropertyKeys>();
            }
        }

        public async Task<Resource> GetResource(Guid id)
        {
            SetupHttpClient();
            if (id == Guid.Empty)
                throw new ArgumentException("GetResource requires a non empty guid as id");

            var response = await _httpClient.GetAsync($"resources/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resource = JsonConvert.DeserializeObject<Resource>(content);
                _logger.Information($"Retrieved Resource: {JsonConvert.SerializeObject(resource, Formatting.Indented)}");
                return resource;
            }

            return null;
        }

        public async Task<Space> GetSpace(Guid id, string includes = null)
        {
            SetupHttpClient();
            if (id == Guid.Empty)
                throw new ArgumentException("GetSpace requires a non empty guid as id");

            var response = await _httpClient.GetAsync($"spaces/{id}/" + (includes != null ? $"?includes={includes}" : ""));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var space = JsonConvert.DeserializeObject<Space>(content);
                _logger.Information($"Retrieved Space: {JsonConvert.SerializeObject(space, Formatting.Indented)}");
                return space;
            }

            return null;
        }

        public async Task<Device> GetDevice(Guid id, string includes = null)
        {
            SetupHttpClient();
            if (id == Guid.Empty)
                throw new ArgumentException("GetDevice requires a non empty guid as id");

            var response = await _httpClient.GetAsync($"devices/{id}/" + (includes != null ? $"?includes={includes}" : ""));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var device = JsonConvert.DeserializeObject<Device>(content);
                _logger.Information($"Retrieved Device: {JsonConvert.SerializeObject(device, Formatting.Indented)}");
                return device;
            }

            return null;
        }

        public async Task<IEnumerable<Space>> GetSpaces(
            int maxNumberToGet = 10,
            string includes = null,
            string propertyKey = null)
        {
            SetupHttpClient();
            var includesFilter = (includes != null ? $"includes={includes}" : "");
            var propertyKeyFilter = (propertyKey != null ? $"propertyKey={propertyKey}" : "");
            var topFilter = $"$top={maxNumberToGet}";
            var response = await _httpClient.GetAsync($"spaces{MakeQueryParams(new[] { includesFilter, propertyKeyFilter, topFilter })}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var spaces = JsonConvert.DeserializeObject<IEnumerable<Space>>(content);
                _logger.Information($"Retrieved {spaces.Count()} Spaces");
                return spaces;
            }
            else
            {
                return Array.Empty<Space>();
            }
        }

        public async Task<IEnumerable<Sensor>> GetSensorsOfSpace(Guid spaceId)
        {
            SetupHttpClient();
            var response = await _httpClient.GetAsync($"sensors?spaceId={spaceId.ToString()}&includes=Types");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sensors = JsonConvert.DeserializeObject<IEnumerable<Sensor>>(content);
                _logger.Information($"Retrieved {sensors.Count()} Sensors");
                return sensors;
            }
            else
            {
                return Array.Empty<Sensor>();
            }
        }

        public async Task UpdateUserDefinedFunction(UserDefinedFunctionUpdate userDefinedFunction, string js)
        {
            SetupHttpClient();
            _logger.Information($"Updating UserDefinedFunction with Metadata: {JsonConvert.SerializeObject(userDefinedFunction, Formatting.Indented)}");
            var displayContent = js.Length > 100 ? js.Substring(0, 100) + "..." : js;
            _logger.Information($"Updating UserDefinedFunction with Content: {displayContent}");

            var metadataContent = new StringContent(JsonConvert.SerializeObject(userDefinedFunction), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var multipartContent = new MultipartFormDataContent("userDefinedFunctionBoundary");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(new StringContent(js), "contents");

            var method = "PATCH";
            var httpVerb = new HttpMethod(method);

            var httpRequestMessage =
                new HttpRequestMessage(httpVerb, $"userdefinedfunctions/{userDefinedFunction.Id}")
                {
                    Content = multipartContent
                };

            await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<bool> IsResourceProvisioning(Guid id)
        {
            SetupHttpClient();
            var resource = await GetResource(id);
            if (resource == null)
            {
                _logger.Error($"Failed to find expected resource, {id.ToString()}");
                return false;
            }

            return resource.Status.ToLower() == "provisioning";
        }

        public async Task<(IEnumerable<T>, HttpResponseMessage)> GetManagementItemsAsync<T>(string queryItem, string queryParams)
        {
            SetupHttpClient();
            var response = await _httpClient.GetAsync($"{queryItem}?{queryParams}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var objects = JsonConvert.DeserializeObject<IEnumerable<T>>(content);
                return (objects, response);
            }
            return (null, response);
        }

        private string MakeQueryParams(IEnumerable<string> queryParams)
        {
            return queryParams
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select((s, i) => (i == 0 ? '?' : '&') + s)
                .Aggregate((result, cur) => result + cur);
        }

        private async Task<Guid> GetIdFromResponse(HttpResponseMessage response, ILogger logger)
        {
            if (!response.IsSuccessStatusCode)
                return Guid.Empty;

            var content = await response.Content.ReadAsStringAsync();

            // strip out the double quotes that come in the response and parse into a guid
            if (!Guid.TryParse(content.Substring(1, content.Length - 2), out var createdId))
            {
                logger.Error($"Returned value from POST did not parse into a guid: {content}");
                return Guid.Empty;
            }

            return createdId;
        }

        private void SetupHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessTokenProvider.AccessToken);
        }
    }
}
