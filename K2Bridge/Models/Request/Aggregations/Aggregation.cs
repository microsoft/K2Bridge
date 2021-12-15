// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes base aggregation class to visit.
    /// </summary>
    public abstract class Aggregation : KustoQLBase, IVisitable
    {
        public string Key { get; internal set; }

        /// <inheritdoc/>
        public abstract void Accept(IVisitor visitor);
    }
}
