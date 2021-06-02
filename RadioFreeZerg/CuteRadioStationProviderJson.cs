using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace RadioFreeZerg
{
    public class CuteRadioStationProviderJson : ICuteRadioStationProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private static readonly JsonSerializer Serializer = new() {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Culture = CultureInfo.InvariantCulture,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.Indented
        };

        private readonly List<CuteRadioStationProperties> stations;

        public CuteRadioStationProviderJson(string stationsFile) {
            if (!File.Exists(stationsFile))
                throw new ArgumentException("Radio stations JSON file does not exist", nameof(stationsFile));

            Log.Debug($"Deserializing radio stations from {stationsFile}...");
            using var fileReader = new StreamReader(stationsFile);
            using var jsonTextReader = new JsonTextReader(fileReader);
            stations = Serializer.Deserialize<List<CuteRadioStationProperties>>(jsonTextReader) ??
                throw new InvalidDataException($"Cannot deserialize {stationsFile}");
            Log.Debug($"Radio stations deserialized.");
        }

        public IEnumerator<CuteRadioStationProperties> GetEnumerator() =>
            ((IEnumerable<CuteRadioStationProperties>) stations).GetEnumerator();

        public IReadOnlyList<CuteRadioStationProperties> All() => stations;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}