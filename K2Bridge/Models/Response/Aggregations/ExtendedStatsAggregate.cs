// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations;

/// <summary>
/// Describes extended stats response element for standard deviation.
/// </summary>
public class ExtendedStatsAggregate : IAggregate
{
    /// <summary>
    /// Gets or sets the count value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("count")]
    public long? Count { get; set; }

    /// <summary>
    /// Gets or sets the min value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("min")]
    public double? Min { get; set; }

    /// <summary>
    /// Gets or sets the max value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("max")]
    public double? Max { get; set; }

    /// <summary>
    /// Gets or sets the average value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("avg")]
    public double? Average { get; set; }

    /// <summary>
    /// Gets or sets the sum value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("sum")]
    public double? Sum { get; set; }

    /// <summary>
    /// Gets or sets the sum_of_squares value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("sum_of_squares")]
    public double? SumOfSquares { get; set; }

    /// <summary>
    /// Gets or sets the variance value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("variance")]
    public double? Variance { get; set; }

    /// <summary>
    /// Gets or sets the variance_population value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("variance_population")]
    public double? VariancePopulation { get; set; }

    /// <summary>
    /// Gets or sets the variance_sampling value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("variance_sampling")]
    public double? VarianceSampling { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("std_deviation")]
    public double? StandardDeviation { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation population value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("std_deviation_population")]
    public double? StandardDeviationPopulation { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation sampling value for the extended stats aggregate.
    /// </summary>
    [JsonProperty("std_deviation_sampling")]
    public double? StandardDeviationSampling { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation bounds valuse for the extended stats aggregate.
    /// </summary>
    [JsonProperty("std_deviation_bounds")]
    public ExtendedStatsStandardDeviationBounds StandardDeviationBounds { get; set; } = new ExtendedStatsStandardDeviationBounds();
}
