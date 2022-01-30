// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations;

using K2Bridge.Models.Request;
using K2Bridge.Visitors;

/// <summary>
/// Describes base aggregation class to visit.
/// </summary>
internal abstract class Aggregation : KustoQLBase, IVisitable
{
    /// <summary>
    ///  Gets or sets key of the aggregation.
    /// </summary>
    public virtual string Key { get; set; }

    /// <inheritdoc/>
    public abstract void Accept(IVisitor visitor);
}
