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
    public record RadioStation(int Id,
                               string Title,
                               string Description,
                               string Genre,
                               string Country,
                               string Language,
                               Uri Source,
                               string[] ContentType)
    {
        public static RadioStation Empty { get; } =
            new(0, "Empty station", "", "", "", "", new Uri("about:blank"), Array.Empty<string>());

        public static RadioStation FromRawSource(int id,
                                                 string title,
                                                 string description,
                                                 string genre,
                                                 string country,
                                                 string language,
                                                 string source,
                                                 string[] contentType) {
            Uri.TryCreate(source.Trim(), UriKind.Absolute, out var parsedUri);
            return parsedUri is not null
                ? new RadioStation(id, title, description, genre, country, language, parsedUri, contentType)
                : throw new InvalidDataException("Radio station stream source is not a valid URI");
        }

        /// <summary>
        ///     Checks if <see cref="RadioStation.Source" /> is an audio stream.
        ///     If so, returns its URI; otherwise downloads content at source URI and finds audio stream link inside.
        /// </summary>
        /// <returns>Found stream URI or failed task.</returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="WebException"></exception>
        public static async Task<RadioStationStreamUri> FindStreamUriAsync(Uri source) {
            var contentType = await FetchContentTypeAsync(source).ConfigureAwait(false);
            if (IsAudioContent(contentType)) return new RadioStationStreamUri(source, contentType);

            var (parsedUri, parsedContentType) = await FindStreamLinkInContentAsync(source).ConfigureAwait(false);
            return new RadioStationStreamUri(parsedUri, parsedContentType);
        }

        private static bool IsAudioContent(string[] contentType) =>
            contentType.Any(_ => 
                _.Contains("audio") &&
                !_.Contains("url") &&
                !_.Contains("charset") &&
                !_.Contains("pls") &&
                !_.Contains("xml") &&
                !_.Contains("wax") );

        private static async Task<string[]> FetchContentTypeAsync(Uri generalUri) {
            if (generalUri.Scheme != "http" && generalUri.Scheme != "https")
                throw new InvalidDataException($"Unsupported station source: {generalUri}");

            string[] contentTypeHeaders = Array.Empty<string>();
            try {
                var checkHeadersResponse = await SharedHttpClient.Instance.GetAsync(generalUri,
                    HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                var contentHeaders = checkHeadersResponse.Content.Headers;
                try {
                    contentTypeHeaders = contentHeaders.GetValues("Content-Type").ToArray();
                } catch (InvalidOperationException) {
                    // Content-Type was not present
                }
            } catch (HttpRequestException e) {
                if (e.Message.Contains("ICY 200 OK"))
                    contentTypeHeaders = new[] {"audio/mpeg", "audio/ICY"}; // This was a Shoutcast link
            }

            return contentTypeHeaders;
        }

        private static async Task<RadioStationStreamUri> FindStreamLinkInContentAsync(Uri generalUri) {
            string content;
            if (generalUri.Scheme == "http" || generalUri.Scheme == "https")
                content = await ReadWebContentAsync(generalUri).ConfigureAwait(false);
            else if (generalUri.Scheme == "file")
                content = await File.ReadAllTextAsync(generalUri.AbsolutePath, Encoding.UTF8).ConfigureAwait(false);
            else
                throw new InvalidDataException($"Unsupported station source: {generalUri}");

            string foundLink = FindStreamLinkInContent(content);
            if (string.IsNullOrEmpty(foundLink))
                throw new InvalidDataException("Downloaded playlist did not contain any http links");

            var foundUri = new Uri(foundLink, UriKind.Absolute);
            var foundContentType = await FetchContentTypeAsync(foundUri).ConfigureAwait(false);
            if (!IsAudioContent(foundContentType))
                throw new InvalidDataException(
                    $"Found URI {foundUri.AbsoluteUri} did not point to audio: {string.Join("|", foundContentType)}");

            return new RadioStationStreamUri(foundUri, foundContentType);
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
            // Account for various line endings:
            var eolPos = content.IndexOf("\n", lastLinkPos, StringComparison.Ordinal);
            if (eolPos < 0) eolPos = content.IndexOf("\r", lastLinkPos, StringComparison.Ordinal);
            if (eolPos < 0) eolPos = int.MaxValue;

            var linkEndPos = Math.Min(quotePos, eolPos);
            return content.Substring(lastLinkPos, Math.Min(content.Length - lastLinkPos, linkEndPos - lastLinkPos))
                          .Trim();
        }
    }
}