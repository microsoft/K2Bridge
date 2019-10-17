namespace K2Bridge
{
    using System;
    using System.Collections;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class ResponseComparer
    {
        private readonly ILogger logger;
        private readonly string requestId;
        private readonly string requestDescription;

        public ResponseComparer(ILogger logger, string requestName, string requestId)
        {
            this.logger = logger;
            this.requestId = requestId;
            this.requestDescription = requestName;
        }

        private SortedList CreateSortedChildrenList(JToken token)
        {
            SortedList SL = new SortedList();

            foreach (JToken child in token.Children())
            {
                string key = child.Path;
                int lastdot = key.LastIndexOf(".");
                if (lastdot > 0)
                {
                    key = key.Substring(lastdot);
                }

                int openBracket = key.LastIndexOf("[");
                int closeBracket = key.LastIndexOf("]");

                if (openBracket > 0)
                {
                    string itemName = string.Empty;

                    if (key.EndsWith("]"))
                    {
                        try
                        {
                            JObject objectNestedId = JsonConvert.DeserializeObject<JObject>(child.ToString());
                            JToken jID;

                            if (objectNestedId.TryGetValue("_id", out jID))
                            {
                                string value = jID.Value<string>();
                                if (value != string.Empty)
                                {
                                    itemName = value;
                                }
                            }
                            else if (objectNestedId.TryGetValue("_index", out jID))
                            {
                                string value = jID.Value<string>();
                                if (value != string.Empty)
                                {
                                    itemName = value;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // this.Logger.LogInformation($"Can't deserialize object: {child.ToString()}.;
                            itemName = child.ToString();
                        }

                        if (itemName != string.Empty)
                        {
                            key = key.Substring(0, openBracket + 1) + itemName + key.Substring(closeBracket);
                        }
                    }
                }

                if (!SL.ContainsKey(key))
                {
                    SL.Add(key, child);
                }
                else
                {
                    this.ReportToken(child, $"duplicate key {key} while creating compare lists");
                }
            }

            return SL;
        }

        public void CompareStreams(Stream passthroughResposeStream, Stream k2ResultsStream)
        {
            try
            {
                string elasticResponse;
                string k2Response;

                if (k2ResultsStream == null || passthroughResposeStream == null)
                {
                    return;
                }

                using (var reader = new StreamReader(passthroughResposeStream))
                {
                    passthroughResposeStream.Position = 0;
                    elasticResponse = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(k2ResultsStream))
                {
                    k2ResultsStream.Position = 0;
                    k2Response = reader.ReadToEnd();
                }

                // Do  nothing
                JObject elasticJsonObject = JsonConvert.DeserializeObject<JObject>(elasticResponse);
                JObject k2JsonObject = JsonConvert.DeserializeObject<JObject>(k2Response);

                if (elasticJsonObject.Count != k2JsonObject.Count)
                {
                    this.logger.LogError($"inconsistent number of children: e:{elasticJsonObject.Count} k2:{k2JsonObject.Count}");
                }

                this.CompareObjects(elasticJsonObject, k2JsonObject);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }

        private void ReportRedundant(JToken token)
        {
            this.ReportToken(token, "Redundant child path:k");
        }

        private void ReportMissing(JToken token)
        {
            this.ReportToken(token, "missing child path:e");
        }

        private void ReportToken(JToken token, string message)
        {
            string tokenDisplay = token.ToString();
            if (tokenDisplay.Length > 31)
            {
                tokenDisplay = tokenDisplay.Substring(0, 50) + "...}";
            }

            this.logger.LogError($"{message}:{token.Path}, type:{token.Type}, object:{tokenDisplay}");
        }

        private void CompareValues(JToken elasticJsonObject, JToken k2JsonObject)
        {
            if (elasticJsonObject.Type != k2JsonObject.Type)
            {
                this.logger.LogError($"Inconsistent type: {elasticJsonObject.Path}, e.Type ({elasticJsonObject.Type.ToString()}) != k2.Type({k2JsonObject.Type.ToString()})");
            }

            var eValue = elasticJsonObject.TokenToString();
            var k2Value = k2JsonObject.TokenToString();

            if (eValue != k2Value)
            {
                if (eValue?.Length > 50)
                {
                    this.logger.LogTrace($"eValue '{eValue}', was truncated");
                    eValue = eValue.Substring(0, 50);
                }

                if (k2Value?.Length > 50)
                {
                    this.logger.LogTrace($"k2Value '{k2Value}', was truncated");
                    k2Value = k2Value.Substring(0, 50);
                }

                this.logger.LogError($"Inconsistent value:{elasticJsonObject.Path}, e.Value({eValue}) != k2.Value({k2Value})");
            }

            return;
        }

        private void CompareObjects(JToken elasticJsonObject, JToken k2JsonObject)
        {
            SortedList eSL = this.CreateSortedChildrenList(elasticJsonObject);
            SortedList kSL = this.CreateSortedChildrenList(k2JsonObject);

            int eIndex = 0;
            int kIndex = 0;

            while (eIndex < eSL.Count || kIndex < kSL.Count)
            {
                JToken eChild = eIndex < eSL.Count ? (JToken)eSL.GetByIndex(eIndex) : null;
                JToken kChild = kIndex < kSL.Count ? (JToken)kSL.GetByIndex(kIndex) : null;
                string eKey = eIndex < eSL.Count ? (string)eSL.GetKey(eIndex) : null;
                string kKey = kIndex < kSL.Count ? (string)kSL.GetKey(kIndex) : null;

                if (kChild == null)
                {
                    this.ReportMissing(eChild);
                    eIndex++;
                    continue;
                }

                if (eChild == null)
                {
                    this.ReportRedundant(kChild);
                    kIndex++;
                    continue;
                }

                int incompareResult = string.Compare(eKey, kKey);

                if (incompareResult == 0)
                {
                    this.CompareValues(eChild, kChild);
                    this.CompareObjects(eChild, kChild);
                    eIndex++;
                    kIndex++;
                }
                else if (incompareResult < 0)
                {
                    this.ReportMissing(eChild);
                    eIndex++;
                }
                else if (incompareResult > 0)
                {
                    this.ReportRedundant(kChild);
                    kIndex++;
                }
            }
        }
    }
}
