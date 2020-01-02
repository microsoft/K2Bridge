// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Data;

    internal static class BucketFactory
    {
        // TODO: implement more aggregation buckets
        public static IBucket MakeBucket(DataRow record)
        {
            return DateHistogramBucket.Create(record);
        }
    }
}