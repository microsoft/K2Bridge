// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;
    using Newtonsoft.Json;

    public class TermBucket : IBucket
    {
        private enum DataReaderMapping
        {
            Key = 0,
        }

        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        public static TermBucket Create(System.Data.IDataRecord record)
        {
            var key = record[(int)DataReaderMapping.Key];

            return new TermBucket
            {
                Key = Convert.ToString(key),
            };
        }
    }
}
