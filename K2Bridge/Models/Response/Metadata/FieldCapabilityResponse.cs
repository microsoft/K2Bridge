namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class FieldCapabilityResponse
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
                throw new ArgumentNullException("fieldCapabilityElement is null.");
            }

            if (string.IsNullOrEmpty(fieldCapabilityElement.Name))
            {
                throw new ArgumentNullException("Name of field is null or empty.");
            }

            this.fields.Add(fieldCapabilityElement.Name, fieldCapabilityElement);
        }
    }
}
