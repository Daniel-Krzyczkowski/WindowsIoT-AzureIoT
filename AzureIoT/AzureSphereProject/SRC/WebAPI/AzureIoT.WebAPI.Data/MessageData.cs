using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureIoT.WebAPI.Data
{
    public class MessageData
    {
        [Required]
        public string Content { get; set; }
    }
}
