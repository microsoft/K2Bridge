// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Find documents by field and a range.
    /// </summary>
    [JsonConverter(typeof(RangeClauseConverter))]
    public class RangeClause : KustoQLBase, ILeafClause, IVisitable
    {
        /// <summary>
        /// Gets or sets the field name to query.
        /// </summary>
        public string FieldName { get; internal set; }

        /// <summary>
        /// Gets or sets GTE (greater than or equal to or equal) value.
        /// </summary>
        public string GTEValue { get; internal set; }

        /// <summary>
        /// Gets or sets GT (greater than) value.
        /// isn't created by kibana but kept here for completeness.
        /// </summary
        public string GTValue { get; set; }

        /// <summary>
        /// Gets or sets LTE (less than or equal) value.
        /// </summary>
        public string LTEValue { get; internal set; }

        /// <summary>
        /// Gets or sets LT (less than) value.
        /// </summary>
        public string LTValue { get; internal set; }

        /// <summary>
        /// Gets or sets Date format used to convert date values in the query.
        /// </summary>
        public string Format { get; internal set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
