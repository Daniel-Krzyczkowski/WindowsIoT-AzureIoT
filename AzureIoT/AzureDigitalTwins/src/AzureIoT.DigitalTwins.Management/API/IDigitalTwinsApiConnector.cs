using AzureIoT.DigitalTwins.Management.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Management.API
{
    public interface IDigitalTwinsApiConnector
    {
        Task<Guid> CreateDevice(DeviceCreate deviceCreate);

        Task<Guid> CreateEndpoints(EndpointsCreate endpointCreate);

        Task<Guid> CreateKeyStore(KeyStoreCreate keyStoreCreate);

        Task<Guid> CreateMatcher(MatcherCreate matcherCreate);

        Task<Guid> CreateResource(ResourceCreate resourceCreate);

        Task<Guid> CreateRoleAssignment(RoleAssignmentCreate roleAssignmentCreate);

        Task<Guid> CreateSensor(SensorCreate sensorCreate);

        Task<Guid> CreateSpace(SpaceCreate spaceCreate);

        Task CreateProperty(Guid spaceId, PropertyCreate propertyCreate);

        Task CreatePropertyKey(PropertyKeyCreate propertyKeyCreate);

        Task<Guid> CreateUserDefinedFunction(UserDefinedFunctionCreate userDefinedFunctionCreate, string js);

        Task<Device> FindDevice(string hardwareId, Guid? spaceId, string includes = null);

        Task<IEnumerable<Matcher>> FindMatchers(IEnumerable<string> names, Guid spaceId);

        Task<Space> FindSpace(string name, Guid parentId);
        Task<UserDefinedFunction> FindUserDefinedFunction(string name, Guid spaceId);

        Task<IEnumerable<Ontology>> GetOntologies();

        Task<IEnumerable<PropertyKeys>> GetPropertyKeys();

        Task<Resource> GetResource(Guid id);

        Task<Space> GetSpace(Guid id, string includes = null);

        Task<Device> GetDevice(Guid id, string includes = null);

        Task<IEnumerable<Space>> GetSpaces(
             int maxNumberToGet = 10,
             string includes = null,
             string propertyKey = null);

        Task<IEnumerable<Sensor>> GetSensorsOfSpace(Guid spaceId);

        Task UpdateUserDefinedFunction(UserDefinedFunctionUpdate userDefinedFunction, string js);

        Task<bool> IsResourceProvisioning(Guid id);

        Task<(IEnumerable<T>, HttpResponseMessage)> GetManagementItemsAsync<T>(string queryItem, string queryParams);
    }
}
