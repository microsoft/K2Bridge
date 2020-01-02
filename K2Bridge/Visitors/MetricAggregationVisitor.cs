// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(AvgAggregation avgAggregation)
        {
            ValidateField(avgAggregation.FieldName);

            avgAggregation.KQL = $"{KQLOperators.Avg}({avgAggregation.FieldName})";
        }

        public void Visit(CardinalityAggregation cardinalityAggregation)
        {
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
