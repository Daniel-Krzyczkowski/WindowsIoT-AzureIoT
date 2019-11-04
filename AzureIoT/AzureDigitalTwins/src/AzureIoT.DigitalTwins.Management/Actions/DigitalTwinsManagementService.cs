using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AzureIoT.DigitalTwins.Management.API;
using AzureIoT.DigitalTwins.Management.Descriptions;
using AzureIoT.DigitalTwins.Management.Models;
using AzureIoT.DigitalTwins.Management.Providers;
using Newtonsoft.Json;
using Serilog;
using YamlDotNet.Serialization;

namespace AzureIoT.DigitalTwins.Management.Actions
{
    public class DigitalTwinsManagementService : IDigitalTwinsManagementService
    {
        private IDigitalTwinsApiConnector _digitalTwinsApiConnector;
        private ISpaceDefinitionProvider _spaceDefinitionProvider;
        private IUserDefinedActionProvider _userDefinedActionProvider;
        private ILogger _logger;

        public DigitalTwinsManagementService(IDigitalTwinsApiConnector digitalTwinsApiConnectorConnector,
            ILogger logger,
             ISpaceDefinitionProvider spaceDefinitionProvider,
            IUserDefinedActionProvider userDefinedActionProvider)
        {
            _digitalTwinsApiConnector = digitalTwinsApiConnectorConnector;
            _logger = logger;
            _spaceDefinitionProvider = spaceDefinitionProvider;
            _userDefinedActionProvider = userDefinedActionProvider;
        }


        public async Task<IEnumerable<Results.Space>> ProvisionSpace()
        {
            var spaceDefinitionAsJson = await _spaceDefinitionProvider.UploadSpaceDefinition();
            if (!string.IsNullOrEmpty(spaceDefinitionAsJson))
            {
                IEnumerable<SpaceDescription> spaceCreateDescriptions;
                spaceCreateDescriptions = GetProvisionSpaceTopology(spaceDefinitionAsJson);

                var results = await CreateSpaces(spaceCreateDescriptions, Guid.Empty);

                System.Diagnostics.Debug.WriteLine($"Completed Provisioning: {JsonConvert.SerializeObject(results, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}");

                return results;
            }
            return null;
        }

        public async Task<IEnumerable<Space>> GetAvailableAndFreshSpaces()
        {
            string spaceDetails = string.Empty;

            System.Diagnostics.Debug.WriteLine("Polling spaces with 'AvailableAndFresh' value type");

            var (spaces, response) = await _digitalTwinsApiConnector.GetManagementItemsAsync<Space>("spaces", "includes=values");
            if (spaces == null)
            {
                var content = await response.Content?.ReadAsStringAsync();
                spaceDetails = $"ERROR: GET spaces?includes=values failed with: {(int)response.StatusCode}, {response.StatusCode} {content}";
                System.Diagnostics.Debug.WriteLine(spaceDetails);
            }

            var availableAndFreshSpaces = spaces.Where(s => s.Values != null && s.Values.Any(v => v.Type == "AvailableAndFresh")).ToList();
            if (availableAndFreshSpaces.Any())
            {
                var availableAndFreshDisplay = availableAndFreshSpaces
                    .Select(s => GetDisplayValues(s))
                    .Aggregate((acc, cur) => acc + "\n" + cur);
                spaceDetails = $"{availableAndFreshDisplay}";
                System.Diagnostics.Debug.WriteLine(spaceDetails);
            }
            else
            {
                spaceDetails = "Unable to find a space with value type 'AvailableAndFresh'";
                System.Diagnostics.Debug.WriteLine(spaceDetails);
            }
            return availableAndFreshSpaces;
        }

        public IEnumerable<SpaceDescription> GetProvisionSpaceTopology(string spaceDefinitionAsJson)
            => new Deserializer().Deserialize<IEnumerable<SpaceDescription>>(spaceDefinitionAsJson);

        public async Task<IEnumerable<Results.Space>> CreateSpaces(
            IEnumerable<SpaceDescription> descriptions,
            Guid parentId)
        {
            var spaceResults = new List<Results.Space>();
            foreach (var description in descriptions)
            {
                var spaceId = await GetExistingSpaceOrCreate(parentId, description);

                if (spaceId != Guid.Empty)
                {
                    // This must happen before devices (or anyhting that could have devices like other spaces)
                    // or the device create will fail because a resource is required on an ancestor space
                    if (description.resources != null)
                        await CreateResources(description.resources, spaceId);

                    var devices = description.devices != null
                        ? await CreateDevices(description.devices, spaceId)
                        : Array.Empty<Device>();

                    if (description.matchers != null)
                        await CreateMatchers(description.matchers, spaceId);

                    if (description.userdefinedfunctions != null)
                        await CreateUserDefinedFunctions(description.userdefinedfunctions, spaceId);

                    if (description.roleassignments != null)
                        await CreateRoleAssignments(description.roleassignments, spaceId);

                    var childSpacesResults = description.spaces != null
                        ? await CreateSpaces(description.spaces, spaceId)
                        : Array.Empty<Results.Space>();

                    var sensors = await _digitalTwinsApiConnector.GetSensorsOfSpace(spaceId);

                    spaceResults.Add(new Results.Space()
                    {
                        Id = spaceId,
                        Devices = devices.Select(device => new Results.Device()
                        {
                            ConnectionString = device.ConnectionString,
                            HardwareId = device.HardwareId,
                        }),
                        Sensors = sensors.Select(sensor => new Results.Sensor()
                        {
                            DataType = sensor.DataType,
                            HardwareId = sensor.HardwareId,
                        }),
                        Spaces = childSpacesResults,
                    });
                }
            }

            return spaceResults;
        }

        public async Task<IEnumerable<Device>> CreateDevices(
            IEnumerable<DeviceDescription> descriptions,
            Guid spaceId)
        {
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Devices must have a spaceId");

            var devices = new List<Device>();

            foreach (var description in descriptions)
            {
                var device = await GetExistingDeviceOrCreate(spaceId, description);

                if (device != null)
                {
                    devices.Add(device);

                    if (description.sensors != null)
                        await CreateSensors(description.sensors, Guid.Parse(device.Id));
                }
            }

            return devices;
        }

        public async Task CreateMatchers(
            IEnumerable<MatcherDescription> descriptions,
            Guid spaceId)
        {
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Matchers must have a spaceId");

            foreach (var description in descriptions)
            {
                await _digitalTwinsApiConnector.CreateMatcher(description.ToMatcherCreate(spaceId));
            }
        }

        public async Task CreateResources(
            IEnumerable<ResourceDescription> descriptions,
            Guid spaceId)
        {
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Resources must have a spaceId");

            foreach (var description in descriptions)
            {
                var createdId = await _digitalTwinsApiConnector.CreateResource(description.ToResourceCreate(spaceId));
                if (createdId != Guid.Empty)
                {
                    // After creation resources might take time to be ready to use so we need
                    // to poll until it is done since downstream operations (like device creation)
                    // may depend on it
                    _logger.Information("Polling until resource is no longer in 'Provisioning' state...");
                    while (await _digitalTwinsApiConnector.IsResourceProvisioning(createdId))
                    {
                        await Task.Delay(5000);
                    }
                }
            }
        }

        public async Task CreateRoleAssignments(
            IEnumerable<RoleAssignmentDescription> descriptions,
            Guid spaceId)
        {
            if (spaceId == Guid.Empty)
                throw new ArgumentException("RoleAssignments must have a spaceId");

            var space = await _digitalTwinsApiConnector.GetSpace(spaceId, includes: "fullpath");

            // A SpacePath is the list of spaces formatted like so: "space1/space2" - where space2 has space1 as a parent
            // When getting SpacePaths of a space itself there is always exactly one path - the path from the root to itself
            // This is not true when getting space paths of other topology items (ie non spaces)
            var path = space.SpacePaths.Single();

            foreach (var description in descriptions)
            {
                string objectId;
                switch (description.objectIdType)
                {
                    case "UserDefinedFunctionId":
                        objectId = (await _digitalTwinsApiConnector.FindUserDefinedFunction(description.objectName, spaceId))?.Id;
                        break;
                    default:
                        objectId = null;
                        _logger.Error($"roleAssignment with objectName must have known objectIdType but instead has '{description.objectIdType}'");
                        break;
                }

                if (objectId != null)
                {
                    await _digitalTwinsApiConnector.CreateRoleAssignment(description.ToRoleAssignmentCreate(objectId, path));
                }
            }
        }

        private async Task CreateSensors(IEnumerable<SensorDescription> descriptions, Guid deviceId)
        {
            if (deviceId == Guid.Empty)
                throw new ArgumentException("Sensors must have a deviceId");

            foreach (var description in descriptions)
            {
                await _digitalTwinsApiConnector.CreateSensor(description.ToSensorCreate(deviceId));
            }
        }

        private async Task CreateUserDefinedFunctions(
            IEnumerable<UserDefinedFunctionDescription> descriptions,
            Guid spaceId)
        {
            if (spaceId == Guid.Empty)
                throw new ArgumentException("UserDefinedFunctions must have a spaceId");

            foreach (var description in descriptions)
            {
                var matchers = await _digitalTwinsApiConnector.FindMatchers(description.matcherNames, spaceId);

                var userDefinedFunctionScript = await _userDefinedActionProvider.UploadUserDefinedAction();

                if (String.IsNullOrWhiteSpace(userDefinedFunctionScript))
                {
                    _logger.Error($"Error creating user defined function: Couldn't read from {description.script}");
                }
                else
                {
                    await CreateOrPatchUserDefinedFunction(
                        description,
                        userDefinedFunctionScript,
                        spaceId,
                        matchers);
                }
            }
        }

        private async Task<Device> GetExistingDeviceOrCreate(Guid spaceId, DeviceDescription description)
        {
            // NOTE: The _digitalTwinsApiConnector doesn't support getting connection strings on bulk get devices calls so we
            // even in the case where we are reusing a preexisting device we need to make the GetDevice
            // call below to get the connection string
            var existingDeviceId = (await _digitalTwinsApiConnector.FindDevice(description.hardwareId, spaceId))?.Id;
            var deviceId = Guid.Empty;
            if (existingDeviceId != null)
            {
                deviceId = Guid.Parse(existingDeviceId);
            }
            else
            {
                deviceId = await _digitalTwinsApiConnector.CreateDevice(description.ToDeviceCreate(spaceId));
            }

            return await _digitalTwinsApiConnector.GetDevice(deviceId, includes: "ConnectionString");
        }

        private async Task<Guid> GetExistingSpaceOrCreate(Guid parentId, SpaceDescription description)
        {
            var existingSpace = await _digitalTwinsApiConnector.FindSpace(description.name, parentId);
            return existingSpace?.Id != null
                ? Guid.Parse(existingSpace.Id)
                : await _digitalTwinsApiConnector.CreateSpace(description.ToSpaceCreate(parentId));
        }

        private async Task CreateOrPatchUserDefinedFunction(
            UserDefinedFunctionDescription description,
            string js,
            Guid spaceId,
            IEnumerable<Matcher> matchers)
        {
            var userDefinedFunction = await _digitalTwinsApiConnector.FindUserDefinedFunction(description.name, spaceId);

            if (userDefinedFunction != null)
            {
                await _digitalTwinsApiConnector.UpdateUserDefinedFunction(
                    description.ToUserDefinedFunctionUpdate(userDefinedFunction.Id, spaceId, matchers.Select(m => m.Id)),
                    js);
            }
            else
            {
                await _digitalTwinsApiConnector.CreateUserDefinedFunction(
                    description.ToUserDefinedFunctionCreate(spaceId, matchers.Select(m => m.Id)),
                    js);
            }
        }

        private string GetDisplayValues(Space space)
        {
            if (space.Values != null)
            {
                var spaceValue = space.Values.First(v => v.Type == "AvailableAndFresh");
                return $"Name: {space.Name}\nId: {space.Id}\nTimestamp: {spaceValue.Timestamp}\nValue: {spaceValue.Value}\n";
            }
            return string.Empty;
        }
    }
}

