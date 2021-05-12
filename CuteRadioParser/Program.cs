using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using RadioFreeZerg.CuteRadio;

namespace RadioFreeZerg
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args) {
            var serializer = new JsonSerializer {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Culture = CultureInfo.InvariantCulture,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Formatting = Formatting.Indented
            };
            
            Dictionary<Uri, RadioStation> radioStations = new();
            var searchModel = CuteRadioStationSearchModel.FromSearch("", 0, 50);
            bool quit = false;
            SharedHttpClient.Instance.Timeout = TimeSpan.FromSeconds(5);
            Log.Info("CuteRadio parser started.");
            while (!quit) {
                var retryCount = 0;
                bool? shouldContinue = null;
                while (!shouldContinue.HasValue && retryCount < 3) {
                    try {
                        shouldContinue =
                            await ProcessStationsPage(searchModel, radioStations).ConfigureAwait(false);
                        searchModel = searchModel with {Offset = searchModel.Offset + searchModel.Limit};
                        if ((bool) !shouldContinue) {
                            Log.Info("Reached the end of CuteRadio pages.");
                            quit = true;
                            break;
                        }
                    } catch (Exception e) {
                        Log.Error(e, "Unexpected error when processing stations page");
                        retryCount++;
                    }
                }

                Serialize(serializer, radioStations);
            }
            
            Log.Info($"CuteRadio parser finished with {searchModel.Offset + searchModel.Limit} entries processed, " +
                $"among them {radioStations.Count} were valid.");
        }

        private static void Serialize(JsonSerializer serializer, Dictionary<Uri, RadioStation> radioStations) {
            Log.Trace("Serializing stations...");
            using var file = File.CreateText(@"stations.json");
            serializer.Serialize(file, radioStations.Values.OrderBy(_ => _.Id));
            Log.Trace("Stations serialized.");
        }

        private static async Task<bool> ProcessStationsPage(CuteRadioStationSearchModel searchModel,
                                                            IDictionary<Uri, RadioStation> radioStations) {
            Log.Info($"Fetching {searchModel.Offset}-{searchModel.Offset + searchModel.Limit} stations...");
            var stationsPage = await CuteRadioStationResources.FetchAsync(searchModel)
                                                              .ConfigureAwait(false);
            if (string.IsNullOrEmpty(stationsPage.Next)) return false;

            Log.Trace("Checking fetched stations' sources...");
            var potentialRadios = stationsPage.ToRadioStations();
            foreach (var potentialRadio in potentialRadios) {
                try {
                    Log.Trace($"Checking {potentialRadio.Title} ({potentialRadio.Source})...");
                    var radioStream = await RadioStation.FindStreamUriAsync(potentialRadio.Source)
                                                        .ConfigureAwait(false);
                    var validRadio = potentialRadio with {
                        Source = radioStream.Uri, ContentType = radioStream.ContentType
                    };
                    radioStations.TryAdd(validRadio.Source, validRadio);
                } catch (Exception e) {
                    Log.Debug($"{potentialRadio.Title} ({potentialRadio.Source.AbsoluteUri}) had an invalid source: " +
                        e.Message);
                }
            }

            return true;
        }
    }
}