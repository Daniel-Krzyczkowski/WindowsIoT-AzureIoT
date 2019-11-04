using AzureIoT.DigitalTwins.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AzureIoT.DigitalTwins.UWP.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowDialogAsync(string title, string content)
        {
            var informationDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "OK",
            };
            await informationDialog.ShowAsync();
        }
    }
}
