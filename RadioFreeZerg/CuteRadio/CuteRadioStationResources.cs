using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RadioFreeZerg.CuteRadio
{
    /// <summary> CuteRadio response for GET /api/cuteradio/stations — retrieves a list of stations. </summary>
    public class CuteRadioStationResources
    {
        public CuteRadioStationResource[]? Items { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public static CuteRadioStationResources Deserialize(string responseContent) {
            var result = JsonConvert.DeserializeObject<CuteRadioStationResources>(responseContent);
            return result ?? throw new InvalidDataException(
                "CuteRadio responded with empty JSON instead of station array.");
        }

        public IEnumerable<RadioStation> ToRadioStations() =>
            Items != null && Items.Length > 0
                ? Items.Select(_ => _.ToRadio())
                : Enumerable.Empty<RadioStation>();
    }
}