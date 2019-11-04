using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoT.DigitalTwins.Management.Providers
{
    public interface IUserDefinedActionProvider
    {
        Task<string> UploadUserDefinedAction();
    }
}
