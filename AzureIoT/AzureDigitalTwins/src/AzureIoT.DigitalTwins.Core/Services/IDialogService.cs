using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Core.Services
{
    public interface IDialogService
    {
        Task ShowDialogAsync(string title, string content);
    }
}
