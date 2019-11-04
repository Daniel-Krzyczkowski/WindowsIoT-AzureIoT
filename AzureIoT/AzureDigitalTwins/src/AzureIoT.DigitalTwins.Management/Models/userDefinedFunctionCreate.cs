// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AzureIoT.DigitalTwins.Management.Models
{
    public class UserDefinedFunctionCreate
    {
        public IEnumerable<string> Matchers { get; set; }
        public string Name { get; set; }
        public string SpaceId { get; set; }
    }
}