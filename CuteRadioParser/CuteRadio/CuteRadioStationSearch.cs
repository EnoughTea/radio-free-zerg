using RestSharp;

namespace CuteRadioParser.CuteRadio
{
    /// <summary>
    ///     Represents CuteRadio search capabilites.
    /// </summary>
    public record CuteRadioStationSearch(int Offset = 0,
                                              int Limit = 20,
                                              string Search = "",
                                              string Country = "",
                                              string Genre = "",
                                              string Language = "",
                                              string Id = "",
                                              CuteRadioStationSort Sort = CuteRadioStationSort.Title,
                                              bool SortDescending = false)
    {
        /// <summary> Restricts results to those matching the specified country. </summary>
        public string Country { get; init; } = Country;

        /// <summary> Restricts results to those matching the specified genre. </summary>
        public string Genre { get; init; } = Genre;

        /// <summary> Restricts results to those matching the comma-separated list of station ids. </summary>
        public string Id { get; init; } = Id;

        /// <summary> Restricts results to those matching the specified language. </summary>
        public string Language { get; init; } = Language;

        /// <summary>
        ///     The maximum number of results that should be returned. Must be between 1 and 50. The default is 20.
        /// </summary>
        public int Limit { get; init; } = Limit;

        /// <summary> The index of the first result to be returned. The index is 0-based. </summary>
        public int Offset { get; init; } = Offset;

        /// <summary> The keyword(s) to be used to filter results. </summary>
        public string Search { get; init; } = Search;

        /// <summary> The property used to sort the results. The default is 'title'. </summary>
        public CuteRadioStationSort Sort { get; init; } = Sort;

        /// <summary> Whether result should be sorted in descending order. The default is false. </summary>
        public bool SortDescending { get; init; } = SortDescending;

        /// <summary>
        ///     Restricts results to those created by the authenticated user. Requires a valid access token.
        /// </summary>
        //public bool Mine { get; init; }
        public static CuteRadioStationSearch FromSearch(string search, int offset = 0, int limit = 10) =>
            new(offset, limit, search);

        public RestRequest ToRequest() =>
            (RestRequest) new RestRequest("/stations", Method.GET)
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