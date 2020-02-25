// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    [JsonConverter(typeof(FieldCapabilityElementConverter))]
    public class FieldCapabilityElement
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsAggregatable { get; set; } = true;

        public bool IsSearchable { get; set; } = true;
    }
}
