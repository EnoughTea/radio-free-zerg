using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Nito;

namespace RadioFreeZerg.CuteRadio
{
    /// <summary> CuteRadio response for GET /api/cuteradio/stations — retrieves a list of stations. </summary>
    public class CuteRadioStationResources
    {
        public CuteRadioStationResource[]? Items { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public static Try<CuteRadioStationResources> Deserialize(string responseContent) =>
            Try.Create(() => {
                var result = JsonConvert.DeserializeObject<CuteRadioStationResources>(responseContent);
                if (result == null)
                    throw new InvalidDataException(
                        "CuteRadio responded with empty JSON instead of station array.");
                return result;
            });

        public IEnumerable<RadioStation> ToRadioStations() =>
            Items != null && Items.Length > 0
                ? Items.Select(_ => _.ToRadio())
                : Enumerable.Empty<RadioStation>();
    }
}