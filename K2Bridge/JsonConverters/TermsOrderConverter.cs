// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the sort element in a <see cref="TermsAggregation"/>.
    /// </summary>
    internal class TermsOrderConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            var obj = new Models.Request.Aggregations.TermsOrder
            {
                Key = first.Name,
                Order = (string)first.Value,
            };

            return obj;
        }
    }
}
