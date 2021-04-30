using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RadioFreeZerg
{
    public record RadioStation(int Id, string Title, string Genre, string Source)
    {
        public static RadioStation Default { get; } = new(0, "Default empty radio", "", "");

        public Uri? ToUri() {
            Uri.TryCreate(Source, UriKind.Absolute, out var parsedUri);
            return parsedUri;
        }

        public async Task<bool> CanBeAccessed() {
            var uri = ToUri();
            if (uri != null)
                try {
                    var response = await SharedHttpClient.Instance.GetAsync(uri,
                        HttpCompletionOption.ResponseHeadersRead);
                    return response.IsSuccessStatusCode;
                } catch {
                    return false;
                }

            return false;
        }
    }
}