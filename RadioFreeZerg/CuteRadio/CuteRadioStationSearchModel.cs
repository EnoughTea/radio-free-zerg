using RestSharp;

namespace RadioFreeZerg.CuteRadio
{
    /// <summary>
    ///     Represents CuteRadio search capabilites.
    /// </summary>
    public record CuteRadioStationSearchModel
    {
        /// <summary> Restricts results to those matching the specified country. </summary>
        public string Country { get; init; } = "";

        /// <summary> Restricts results to those matching the specified genre. </summary>
        public string Genre { get; init; } = "";

        /// <summary> Restricts results to those matching the comma-separated list of station ids. </summary>
        public string Id { get; init; } = "";

        /// <summary> Restricts results to those matching the specified language. </summary>
        public string Language { get; init; } = "";

        /// <summary>
        ///     The maximum number of results that should be returned. Must be between 1 and 50. The default is 20.
        /// </summary>
        public int Limit { get; init; } = 10;

        /// <summary> The index of the first result to be returned. The index is 0-based. </summary>
        public int Offset { get; init; }

        /// <summary> The keyword(s) to be used to filter results. </summary>
        public string Search { get; init; }

        /// <summary> The property used to sort the results. The default is 'title'. </summary>
        public CuteRadioStationSort Sort { get; init; } = CuteRadioStationSort.Title;

        /// <summary> Whether result should be sorted in descending order. The default is false. </summary>
        public bool SortDescending { get; init; }

        /// <summary>
        ///     Restricts results to those created by the authenticated user. Requires a valid access token.
        /// </summary>
        public bool Mine { get; init; }

        public RestRequest ToRequest() =>
            (RestRequest) new RestRequest("stations", Method.GET)
                          .AddQueryParameter("limit", Limit.ToString())
                          .AddQueryParameter("offset", Offset.ToString())
                          .AddQueryParameter("country", Country)
                          .AddQueryParameter("genre", Genre)
                          .AddQueryParameter("id", Id)
                          .AddQueryParameter("language", Language)
                          .AddQueryParameter("search", Search)
                          .AddQueryParameter("sort", Sort.ToString())
                          .AddQueryParameter("sortDescending", SortDescending.ToString())
                          .AddQueryParameter("approved", "1");
    }
}