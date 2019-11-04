using AzureIoT.DigitalTwins.Management.Providers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.UWP.Providers
{
    public class SpaceDefinitionProvider : ISpaceDefinitionProvider
    {
        private ILogger _logger;
        public SpaceDefinitionProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> UploadSpaceDefinition()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".yaml");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string spaceDefinitionAsJson = await Windows.Storage.FileIO.ReadTextAsync(file);
                return spaceDefinitionAsJson;
            }
            else
            {
                _logger.Information("Open space definition file operation cancelled.");
                return string.Empty;
            }
        }
    }
}
