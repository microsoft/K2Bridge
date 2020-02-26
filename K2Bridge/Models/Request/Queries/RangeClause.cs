﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(RangeClauseConverter))]
    internal class RangeClause : KustoQLBase, ILeafClause, IVisitable
    {
        public string FieldName { get; set; }

        public string GTEValue { get; set; }

        // isn't created by kibana but kept here for completeness
        public string GTValue { get; set; }

        public string LTEValue { get; set; }

        public string LTValue { get; set; }

        public string Format { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
