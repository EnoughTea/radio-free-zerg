using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RadioFreeZerg
{
    public record RadioStationStreamUri(Uri Uri, string[] ContentType);

    public record RadioStation(int Id, string Title, string Genre, string Source)
    {
        public static RadioStation Default { get; } = new(0, "Default empty radio", "", "");

        public Uri? ToUri() {
            Uri.TryCreate(Source, UriKind.Absolute, out var parsedUri);
            return parsedUri;
        }

        /// <summary>
        ///     Checks if <see cref="RadioStation.Source" /> is an audio stream.
        ///     If so, returns its URI; otherwise downloads content at source URI and finds audio stream link inside.
        /// </summary>
        /// <returns>Found stream URI or failed task.</returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="WebException"></exception>
        public async Task<RadioStationStreamUri> FindStreamUriAsync() {
            var generalUri = ToUri();
            if (generalUri == null)
                throw new InvalidDataException("Cannot find radio station stream; source is not a valid URI");

            var contentType = await FetchContentTypeAsync(generalUri).ConfigureAwait(false);
            var isAudio = contentType.Any(_ => _.Contains("audio") && !_.Contains("url"));
            if (isAudio) return new RadioStationStreamUri(generalUri, contentType);

            var parsedUri = await FindLinkInContentAsync(generalUri).ConfigureAwait(false);
            return new RadioStationStreamUri(parsedUri, contentType);
        }

        private static async Task<string[]> FetchContentTypeAsync(Uri generalUri) {
            var checkHeadersResponse = await SharedHttpClient.Instance.GetAsync(generalUri,
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var contentHeaders = checkHeadersResponse.Content.Headers;
            string[] contentTypeHeaders;
            try {
                contentTypeHeaders = contentHeaders.GetValues("Content-Type").ToArray();
            } catch (InvalidOperationException) {
                contentTypeHeaders = Array.Empty<string>();
            }

            return contentTypeHeaders;
        }

        private static async Task<Uri> FindLinkInContentAsync(Uri generalUri) {
            var contentResponse = await SharedHttpClient.Instance.GetAsync(generalUri).ConfigureAwait(false);
            if (!contentResponse.IsSuccessStatusCode)
                throw new WebException(
                    $"'{generalUri.AbsoluteUri}' returned HTTP status: {contentResponse.StatusCode}");

            string content = await contentResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string foundLink = FindLinkInContent(content);
            if (string.IsNullOrEmpty(foundLink))
                throw new InvalidDataException("Downloaded playlist did not contain any http links");

            return new Uri(foundLink, UriKind.Absolute);
        }

        private static string FindLinkInContent(string content) {
            var lastLinkPos = content.LastIndexOf("http", StringComparison.InvariantCultureIgnoreCase);
            if (lastLinkPos < 0) return "";

            var quotePos = content.IndexOf("\"", lastLinkPos, StringComparison.Ordinal);
            if (quotePos < 0) quotePos = int.MaxValue;
            var eolPos = content.IndexOf("\n", lastLinkPos, StringComparison.Ordinal);
            if (eolPos < 0) eolPos = int.MaxValue;

            var linkEndPos = Math.Min(quotePos, eolPos);
            return content.Substring(lastLinkPos, linkEndPos - lastLinkPos);
        }
    }
}