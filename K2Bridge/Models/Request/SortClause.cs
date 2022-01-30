// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request;

using K2Bridge.JsonConverters;
using K2Bridge.Visitors;
using Newtonsoft.Json;

/// <summary>
/// Clause representing the requested order value in query.
/// </summary>
[JsonConverter(typeof(SortClauseConverter))]
internal class SortClause : KustoQLBase, IVisitable
{
    /// <summary>
    /// Gets or sets the field name to order by.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the order value.
    /// </summary>
    public string Order { get; set; }

    /// <inheritdoc/>
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
