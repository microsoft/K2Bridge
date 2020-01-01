// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class FieldCapabilityResponse
    {
        private readonly Dictionary<string, FieldCapabilityElement> fields = new Dictionary<string, FieldCapabilityElement>();

        [JsonProperty("fields")]
        public IDictionary<string, FieldCapabilityElement> Fields
        {
            get { return this.fields; }
        }

        public void AddField(FieldCapabilityElement fieldCapabilityElement)
        {
            if (fieldCapabilityElement == null)
            {
                throw new ArgumentNullException("fieldCapabilityElement");
            }

            if (string.IsNullOrEmpty(fieldCapabilityElement.Name))
            {
                throw new ArgumentNullException("fieldCapabilityElement.Name");
            }

            this.fields.Add(fieldCapabilityElement.Name, fieldCapabilityElement);
        }
    }
}
