using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadioFreeZerg
{
    /// <summary> Represents a proper radio station stream source. </summary>
    public record RadioStationStreamUri(Uri Uri, string[] ContentType);

    /// <summary>
    ///     Represents a single radio station with its source, which can be a link to a stream or a playlist.
    /// </summary>
    public record RadioStation(int Id, string Title, string Genre, Uri Source)
    {
        public static RadioStation Empty { get; } = new(0, "Empty station", "", new Uri("about:blank"));

        public static RadioStation FromRawSource(int id, string title, string genre, string source) {
            Uri.TryCreate(source, UriKind.Absolute, out var parsedUri);
            return parsedUri != null
                ? new RadioStation(id, title, genre, parsedUri)
                : throw new InvalidDataException("Radio station stream source is not a valid URI");
        }

        /// <summary>
        ///     Checks if <see cref="RadioStation.Source" /> is an audio stream.
        ///     If so, returns its URI; otherwise downloads content at source URI and finds audio stream link inside.
        /// </summary>
        /// <returns>Found stream URI or failed task.</returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="WebException"></exception>
        public async Task<RadioStationStreamUri> FindStreamUriAsync() {
            var contentType = await FetchContentTypeAsync(Source).ConfigureAwait(false);
            var isAudio = contentType.Any(_ => _.Contains("audio") && !_.Contains("url"));
            if (isAudio) return new RadioStationStreamUri(Source, contentType);

            var parsedUri = await FindStreamLinkInContentAsync(Source).ConfigureAwait(false);
            return new RadioStationStreamUri(parsedUri, contentType);
        }

        private static async Task<string[]> FetchContentTypeAsync(Uri generalUri) {
            if (generalUri.Scheme != "http" || generalUri.Scheme != "https")
                throw new InvalidDataException($"Unsupported station source: {generalUri}");

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

        private static async Task<Uri> FindStreamLinkInContentAsync(Uri generalUri) {
            string content;
            if (generalUri.Scheme == "http" && generalUri.Scheme == "https") {
                content = await ReadWebContentAsync(generalUri).ConfigureAwait(false);
            } else if (generalUri.Scheme == "file") {
                content = await File.ReadAllTextAsync(generalUri.AbsolutePath, Encoding.UTF8).ConfigureAwait(false);
            } else {
                throw new InvalidDataException($"Unsupported station source: {generalUri}");
            }

            string foundLink = FindStreamLinkInContent(content);
            if (string.IsNullOrEmpty(foundLink))
                throw new InvalidDataException("Downloaded playlist did not contain any http links");

            return new Uri(foundLink, UriKind.Absolute);
        }

        private static async Task<string> ReadWebContentAsync(Uri generalUri) {
            var contentResponse = await SharedHttpClient.Instance.GetAsync(generalUri).ConfigureAwait(false);
            if (!contentResponse.IsSuccessStatusCode)
                throw new WebException(
                    $"{generalUri.AbsoluteUri} returned bad HTTP status: {contentResponse.StatusCode}");

            return await contentResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private static string FindStreamLinkInContent(string content) {
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