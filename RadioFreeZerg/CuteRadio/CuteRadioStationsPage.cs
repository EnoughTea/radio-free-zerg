using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace RadioFreeZerg.CuteRadio
{
    public record CuteRadioStationsPage(CuteRadioStationResources Stations) {
        public static async Task<CuteRadioStationsPage> FetchAsync(CuteRadioStationSearchModel requestData) {
            var request = requestData.ToRequest();
            var stationsResponse = await CuteRadioClient.Instance.ExecuteAsync(request).ConfigureAwait(false);
            return FromContent(stationsResponse.Content, request.Resource);
        }

        public Task<CuteRadioStationsPage?> FetchNextAsync() => FetchPageRawAsync(Stations.Next);

        public Task<CuteRadioStationsPage?> FetchPreviousAsync() => FetchPageRawAsync(Stations.Previous);

        private static async Task<CuteRadioStationsPage?> FetchPageRawAsync(string? resourceLink) {
            if (string.IsNullOrEmpty(resourceLink)) return null;
            
            var request = new RestRequest(resourceLink);
            var response = await CuteRadioClient.Instance.ExecuteGetAsync(request).ConfigureAwait(false);
            return FromContent(response.Content, resourceLink);
        }
        
        private static CuteRadioStationsPage FromContent(string content, string resource) {
            if (string.IsNullOrEmpty(content))
                throw new WebException($"{resource} returned empty content.");

            var stationResources = CuteRadioStationResources.Deserialize(content);
            return new CuteRadioStationsPage(stationResources);
        }
    }
}