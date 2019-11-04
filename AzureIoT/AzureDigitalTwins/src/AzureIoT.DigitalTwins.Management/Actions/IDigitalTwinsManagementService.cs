using AzureIoT.DigitalTwins.Management.Descriptions;
using AzureIoT.DigitalTwins.Management.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Management.Actions
{
    public interface IDigitalTwinsManagementService
    {
        Task<IEnumerable<Results.Space>> ProvisionSpace();
        Task<IEnumerable<Space>> GetAvailableAndFreshSpaces();

        IEnumerable<SpaceDescription> GetProvisionSpaceTopology(string jsonContent);

        Task<IEnumerable<Results.Space>> CreateSpaces(
            IEnumerable<SpaceDescription> descriptions,
            Guid parentId);

        Task<IEnumerable<Device>> CreateDevices(
            IEnumerable<DeviceDescription> descriptions,
            Guid spaceId);

        Task CreateMatchers(
            IEnumerable<MatcherDescription> descriptions,
            Guid spaceId);

        Task CreateResources(
            IEnumerable<ResourceDescription> descriptions,
            Guid spaceId);

        Task CreateRoleAssignments(
            IEnumerable<RoleAssignmentDescription> descriptions,
            Guid spaceId);
    }
}
