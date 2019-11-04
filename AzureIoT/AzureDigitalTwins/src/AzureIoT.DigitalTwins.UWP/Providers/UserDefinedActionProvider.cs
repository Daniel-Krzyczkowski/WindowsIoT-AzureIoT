using AzureIoT.DigitalTwins.Management.Providers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.UWP.Providers
{
    public class UserDefinedActionProvider : IUserDefinedActionProvider
    {
        private ILogger _logger;
        public UserDefinedActionProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> UploadUserDefinedAction()
        {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".js");

                Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    string userDefinedActionAsJson = await Windows.Storage.FileIO.ReadTextAsync(file);
                    return userDefinedActionAsJson;
                }
                else
                {
                    _logger.Information("Open space definition file operation cancelled.");
                    return string.Empty;
                }
        }
    }
}
