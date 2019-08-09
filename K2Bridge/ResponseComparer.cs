namespace K2Bridge
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Collections;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class ResponseComparer
    {

        Serilog.ILogger Logger;

        public ResponseComparer(Serilog.ILogger Logger)
        {
            this.Logger = Logger;
        }

        private SortedList CreateSortedChildrenList(JToken token)
        {
            SortedList SL = new SortedList();

            foreach (JToken child in token.Children())
            {
                string key = child.Path;

                if (key.EndsWith("]"))
                {
                    char[] open = new char[1];
                    open[0] = '[';

                    int openBracket = key.LastIndexOfAny(open);

                    Hit hit = JsonConvert.DeserializeObject<Hit>(child.ToString());

                    string itemName = hit._id;

                    if (itemName != string.Empty)
                    {
                        key = key.Substring(0, openBracket) + "[" + itemName + "]";
                    }
                }

                SL.Add(key, child);
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


                //Do  nothing
                JObject elasticJsonObject = JsonConvert.DeserializeObject<JObject>(elasticResponse);
                JObject k2JsonObject = JsonConvert.DeserializeObject<JObject>(k2Response);

                if (elasticJsonObject.Count != k2JsonObject.Count)
                {
                    this.Logger.Error($"inconsistent number of children: e:{elasticJsonObject.Count} k2:{k2JsonObject.Count}");
                }

                CompareObjects(elasticJsonObject, k2JsonObject);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "An exception...");
            }
        }
        private void CompareObjects(JToken elasticJsonObject, JToken k2JsonObject)
        {
            SortedList eSL = CreateSortedChildrenList(elasticJsonObject);
            SortedList kSL = CreateSortedChildrenList(k2JsonObject);

            int eIndex = 0;
            int kIndex = 0;

            while (eIndex < eSL.Count || kIndex < kSL.Count)
            {
                JToken eChild = (eIndex < eSL.Count ? (JToken)eSL.GetByIndex(eIndex) : null);
                JToken kChild = (kIndex < kSL.Count ? (JToken)kSL.GetByIndex(kIndex) : null);

                if (kChild == null)
                {
                    this.Logger.Error($"missing child path: e:{eChild.Path}, type:{eChild.Type}, object:{eChild}");
                    eIndex++;
                    continue;
                }
                if (eChild == null)
                {
                    this.Logger.Error($"Redundant child path: k:{kChild.Path}, type:{kChild.Type}, object:{kChild}");
                    kIndex++;
                    continue;
                }

                string ePath = eChild.Path;
                string kPath = kChild.Path;


                int incompareResult = String.Compare(ePath, kPath);

                if (0 == incompareResult)
                {
                    if (eChild.HasValues && kChild.HasValues)
                    {
                        CompareObjects(eChild, kChild);
                    }
                    eIndex++;
                    kIndex++;
                }
                else if (incompareResult > 0)
                {
                    this.Logger.Error($"missing child path: e:{eChild.Path}, type:{eChild.Type}, object:{eChild}");
                    eIndex++;
                }
                else if (incompareResult < 0)
                {
                    this.Logger.Error($"Redundant child path: k:{kChild.Path}, type:{kChild.Type}, object:{kChild}");
                    kIndex++;
                }
            }
        }
    }
}
