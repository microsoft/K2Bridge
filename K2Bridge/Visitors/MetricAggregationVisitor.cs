// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Aggregations;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(AvgAggregation avgAggregation)
        {
            if (avgAggregation == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(avgAggregation));
            }

            ValidateField(avgAggregation.FieldName);

            avgAggregation.KQL = $"{KQLOperators.Avg}({avgAggregation.FieldName})";
        }

        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
            if (cardinalityAggregation == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null",
                    nameof(cardinalityAggregation));
            }

            ValidateField(cardinalityAggregation.FieldName);

            cardinalityAggregation.KQL = $"{KQLOperators.DCount}({cardinalityAggregation.FieldName})";
        }

        private static void ValidateField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new IllegalClauseException("Average FieldName must have a valid value");
            }
        }
    }
}
