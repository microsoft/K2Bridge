// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using System.Linq;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the order element from TermsAggregation to <see cref="TermsOrder"/>.
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
            var property = (JProperty)jo.First;

            var obj = new TermsOrder();

            if (property != null)
            {
                obj.Field = property.Name;
                obj.Sort = property.Value.ToString();
            }

            return obj;
        }
    }
}
