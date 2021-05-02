using System;
using RadioFreeZerg.CuteRadio;

namespace RadioFreeZerg.States
{
    public class StationsSearchState : AppState
    {
        public StationsSearchState() : base(AppStateId.StationsSearch) { }

        public override void StateEnter(AppStateId previousStateId, AppStateData data) {
            Console.WriteLine("Type something to search radio station titles or press Enter to browse everything. " +
                "Type 'b' to go back.");
        }

        public override AppStateId HandleEvent(string? stateEvent, AppStateData data) {
            if (stateEvent?.Trim() == "b") return AppStateId.Initial;

            if (data.Search.CurrentPage is null) {
                data.Search.SearchModel = data.Search.SearchModel with {Search = stateEvent ?? ""};
                var stationResources = PerformSearch(data);
                data.Search.CurrentPage = stationResources;
            }

            data.Search.Prefetch(); // Fire and forget by design
            return PrintRadiosIfAny(data.Search.CurrentPage) ? AppStateId.StationsFound : AppStateId.StationsSearch;
        }

        internal static bool PrintRadiosIfAny(CuteRadioStationResources stationResources) {
            if (stationResources.Items is null) {
                Console.WriteLine("CuteRadio returned empty content for this search.");
                return false;
            }

            var radios = stationResources.ToRadioStations();
            foreach (var radio in radios) {
                Console.WriteLine(radio);
            }

            return true;
        }

        internal static CuteRadioStationResources PerformSearch(AppStateData data) {
            CuteRadioStationResources searchResults = new();

            try {
                var stationsPageTask = CuteRadioStationResources.FetchAsync(data.Search.SearchModel)
                                                                .ConfigureAwait(false);
                searchResults = stationsPageTask.GetAwaiter().GetResult();
            } catch (Exception e) {
                Console.WriteLine($"Cannot perform radio station search: {e.Message}.");
                Log.Error(e);
            }

            return searchResults;
        }
    }
}