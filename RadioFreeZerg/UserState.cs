using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace RadioFreeZerg
{
    public class UserState
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private int volume = 100;

        public static string AppFolderPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RadioFreeZerg");

        public static string StateFilePath { get; } = Path.Combine(AppFolderPath, "state.json");

        public IReadOnlyList<int> AvailableStationsIds { get; set; } = new List<int>();

        public int CurrentPage { get; set; }

        public int CurrentStationId { get; set; }

        public int ToggledStationId { get; set; }

        public int Volume {
            get => volume;
            set {
                if (volume != value) volume = Math.Clamp(value, 0, 100);
            }
        }

        public void Save() {
            Log.Trace("Serializing user state...");
            using var fileWriter = new StreamWriter(StateFilePath, false, Encoding.UTF8);
            using var jsonTextWriter = new JsonTextWriter(fileWriter);
            CuteRadioStationProviderJson.Serializer.Serialize(jsonTextWriter, this);
            Log.Trace("User state serialized.");
        }

        public static UserState Load() {
            if (!File.Exists(StateFilePath)) {
                Log.Debug("No user state exist to deserialize, returning defaults.");
                return new UserState();
            }

            Log.Debug("Deserializing user state...");
            using var fileReader = new StreamReader(StateFilePath);
            using var jsonTextReader = new JsonTextReader(fileReader);
            var deserialized = CuteRadioStationProviderJson.Serializer.Deserialize<UserState>(jsonTextReader) ??
                throw new InvalidDataException($"Cannot deserialize {StateFilePath}");
            Log.Debug("User state deserialized.");
            return deserialized;
        }
    }
}