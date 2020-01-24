// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class FieldCapabilityResponse
    {
        private readonly Dictionary<string, FieldCapabilityElement> fields = new Dictionary<string, FieldCapabilityElement>();

        [JsonProperty("fields")]
        public IDictionary<string, FieldCapabilityElement> Fields => fields;

        public void AddField(FieldCapabilityElement fieldCapabilityElement)
        {
            Ensure.IsNotNull(fieldCapabilityElement, nameof(fieldCapabilityElement));
            Ensure.IsNotNull(fieldCapabilityElement.Name, nameof(fieldCapabilityElement.Name));

            fields.Add(fieldCapabilityElement.Name, fieldCapabilityElement);
        }
    }
}
