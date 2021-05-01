using System;
using System.Collections.Immutable;
using RadioFreeZerg.CuteRadio;

namespace RadioFreeZerg.States
{
    public class StationSearchState : AppState
    {
        private int offset = 0;
        private int limit = 10;
        
        public StationSearchState() : base(AppStateId.StationSearch) { }

        public override void StateEnter(AppStateId previousStateId, AppStateData data) {
            Console.WriteLine("Type something to search radio station titles or press Enter to browse everything. Type 'b' to go back.");
        }

        public override AppStateId HandleEvent(string? stateEvent, AppStateData data) {
            if (stateEvent?.Trim() == "b") {
                return AppStateId.Initial;
            } else {
                var stationResources = PerformSearch(stateEvent);
                data.CurrentSearchPage = stationResources;
                if (stationResources.Items is not null) {
                    var radios = stationResources.ToRadioStations();
                    foreach (var radio in radios) {
                        Console.WriteLine(radio);
                    }
                } else {
                    Console.WriteLine("CuteRadio returned empty content for this search.");
                }
            }
            
            return AppStateId.StationSearch;
        }

        private CuteRadioStationResources PerformSearch(string? searchTarget) {
            CuteRadioStationResources searchResults = new();
            var search = CuteRadioStationSearchModel.FromSearch(searchTarget ?? "", offset, limit);
            try {
                var stationsPageTask = CuteRadioStationResources.FetchAsync(search)
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