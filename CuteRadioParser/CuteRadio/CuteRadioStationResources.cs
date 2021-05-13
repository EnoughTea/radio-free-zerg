using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace CuteRadioParser.CuteRadio
{
    /// <summary> CuteRadio response for GET /api/cuteradio/stations — retrieves a list of stations. </summary>
    public class CuteRadioStationResources
    {
        public ImmutableList<CuteRadioStationResource>? Items { get; set; }

        [DefaultValue("")] public string Next { get; set; } = "";

        [DefaultValue("")] public string Previous { get; set; } = "";

        public static CuteRadioStationResources Deserialize(string responseContent) {
            var result = JsonConvert.DeserializeObject<CuteRadioStationResources>(responseContent);
            return result ?? throw new InvalidDataException(
                "CuteRadio responded with empty JSON instead of station array.");
        }

        public static async Task<CuteRadioStationResources> FetchAsync(CuteRadioStationSearchModel requestData) {
            var request = requestData.ToRequest();
            var stationsResponse = await CuteRadioClient.Instance.ExecuteAsync(request).ConfigureAwait(false);
            return FromContent(stationsResponse.Content, request.Resource);
        }

        public ImmutableList<RadioStation> ToRadioStations() {
            if (Items is not null && Items.Count > 0)
                return (from stationResouce in Items
                        let radio = stationResouce.ToRadioOrNull()
                        where radio is not null
                        select radio).ToImmutableList()!;

            return ImmutableList<RadioStation>.Empty;
        }

        public Task<CuteRadioStationResources?> FetchNextOrNullAsync() => FetchPageOrNullRawAsync(Next);

        public Task<CuteRadioStationResources?> FetchPreviousOrNullAsync() => FetchPageOrNullRawAsync(Previous);

        private static async Task<CuteRadioStationResources?> FetchPageOrNullRawAsync(string? resourceLink) {
            if (string.IsNullOrEmpty(resourceLink)) return null;

            var request = new RestRequest(resourceLink);
            var response = await CuteRadioClient.Instance.ExecuteGetAsync(request).ConfigureAwait(false);
            return FromContent(response.Content, resourceLink);
        }

        private static CuteRadioStationResources FromContent(string content, string resource) {
            if (string.IsNullOrEmpty(content))
                throw new WebException($"{resource} returned empty content.");

            return Deserialize(content);
        }
    }
}