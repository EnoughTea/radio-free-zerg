using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RadioFreeZerg.CuteRadio
{
    /// <summary> CuteRadio response for GET /api/cuteradio/stations — retrieves a list of stations. </summary>
    public class CuteRadioStationResources
    {
        public ImmutableList<CuteRadioStationResource>? Items { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public static CuteRadioStationResources Deserialize(string responseContent) {
            var result = JsonConvert.DeserializeObject<CuteRadioStationResources>(responseContent);
            return result ?? throw new InvalidDataException(
                "CuteRadio responded with empty JSON instead of station array.");
        }

        public ImmutableList<RadioStation> ToRadioStations() {
            if (Items != null && Items.Count > 0)
                return (from stationResouce in Items
                        let radio = stationResouce.ToRadio()
                        where radio != null
                        select radio).ToImmutableList()!;

            return ImmutableList<RadioStation>.Empty;
        }
    }
}