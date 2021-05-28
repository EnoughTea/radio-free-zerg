using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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

        private static readonly JsonSerializer Serializer = new() {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Culture = CultureInfo.InvariantCulture,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.Indented
        };

        private static async Task Main(string[] args) {
            Log.Info("CuteRadio parser started.");
            SharedHttpClient.Instance.Timeout = TimeSpan.FromSeconds(5);
            ConcurrentDictionary<Uri, RadioStation> sourcesToStations = new();
            var stationSearch = CuteRadioStationSearch.FromSearch("", 0, 50);
            var shouldContinue = false;

            do {
                try {
                    (shouldContinue, stationSearch) = await
                        GatherStationsFromCuteRadioAsync(stationSearch, sourcesToStations).ConfigureAwait(false);
                } catch (Exception e) {
                    Log.Error(e, "Unexpected error when processing stations page");
                    Thread.Sleep(5000);
                }
            } while (shouldContinue);

            var stations = sourcesToStations.Values.OrderBy(_ => _.Id);
            Serialize(Serializer, stations);
            Log.Info($"CuteRadio parser finished with {stationSearch.Offset + stationSearch.Limit} entries processed, " +
                $"among them {sourcesToStations.Count} were valid.");
        }

        private static async Task<(bool, CuteRadioStationSearch)> GatherStationsFromCuteRadioAsync(
            CuteRadioStationSearch search,
            ConcurrentDictionary<Uri, RadioStation> radioStations) {
            var shouldContinue = await ProcessStationsPageAsync(search, radioStations).ConfigureAwait(false);
            search = search with {Offset = search.Offset + search.Limit};
            return (shouldContinue, search);
        }

        private static void Serialize(JsonSerializer serializer, IEnumerable<RadioStation> radioStations) {
            Log.Trace("Serializing stations...");
            using var file = File.CreateText(@"stations.json");
            serializer.Serialize(file, radioStations);
            Log.Trace("Stations serialized.");
        }

        private static async Task<bool> ProcessStationsPageAsync(CuteRadioStationSearch search,
                                                                 ConcurrentDictionary<Uri, RadioStation>
                                                                     radioStations) {
            Log.Info($"Fetching {search.Offset}-{search.Offset + search.Limit} stations...");
            var stationsPage = await CuteRadioStationResources.FetchAsync(search)
                                                              .ConfigureAwait(false);
            if (string.IsNullOrEmpty(stationsPage.Next)) return false;

            Log.Trace("Checking fetched stations' sources...");
            var potentialRadios = stationsPage.ToRadioStations();
            var radioCheckTasks = potentialRadios.Select(potentialRadio => Task.Run(async () => {
                    var validRadioOrNull = await CheckRadioStationSourceAsync(potentialRadio).ConfigureAwait(false);
                    if (validRadioOrNull != null) radioStations.TryAdd(validRadioOrNull.Source, validRadioOrNull);
                }
            ));
            await Task.WhenAll(radioCheckTasks).ConfigureAwait(false);
            return true;
        }

        private static async Task<RadioStation?> CheckRadioStationSourceAsync(RadioStation potentialRadio) {
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