using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CuteRadioParser.CuteRadio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace CuteRadioParser
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
            
            ConcurrentDictionary<Uri, RadioStation> radioStations = new();
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

        private static void Serialize(JsonSerializer serializer, IDictionary<Uri, RadioStation> radioStations) {
            Log.Trace("Serializing stations...");
            using var file = File.CreateText(@"stations.json");
            serializer.Serialize(file, radioStations.Values.OrderBy(_ => _.Id));
            Log.Trace("Stations serialized.");
        }

        private static async Task<bool> ProcessStationsPage(CuteRadioStationSearchModel searchModel,
                                                            ConcurrentDictionary<Uri, RadioStation> radioStations) {
            Log.Info($"Fetching {searchModel.Offset}-{searchModel.Offset + searchModel.Limit} stations...");
            var stationsPage = await CuteRadioStationResources.FetchAsync(searchModel)
                                                              .ConfigureAwait(false);
            if (string.IsNullOrEmpty(stationsPage.Next)) return false;

            Log.Trace("Checking fetched stations' sources...");
            var potentialRadios = stationsPage.ToRadioStations();
            var radioCheckTasks = potentialRadios.Select(potentialRadio => Task.Run(async () => {
                    var validRadioOrNull = await CheckRadioStationSource(potentialRadio).ConfigureAwait(false);
                    if (validRadioOrNull != null) radioStations.TryAdd(validRadioOrNull.Source, validRadioOrNull);
                }
            ));
            await Task.WhenAll(radioCheckTasks).ConfigureAwait(false); 
            return true;
        }

        private static async Task<RadioStation?> CheckRadioStationSource(RadioStation potentialRadio) {
            try {
                Log.Trace($"Checking {potentialRadio.Title} ({potentialRadio.Source})...");
                var (uri, contentType) = await RadioStation.FindStreamUriAsync(potentialRadio.Source)
                    .ConfigureAwait(false);
                return potentialRadio with {
                    Source = uri,
                    ContentType = contentType
                };
            } catch (Exception e) {
                Log.Debug($"{potentialRadio.Title} ({potentialRadio.Source.AbsoluteUri}) had an invalid source: " +
                    e.Message);
                return null;
            }
        } 
        
    }
}