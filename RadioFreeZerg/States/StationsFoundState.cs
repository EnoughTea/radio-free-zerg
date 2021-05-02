using System;

namespace RadioFreeZerg.States
{
    public class StationsFoundState : AppState
    {
        public StationsFoundState() : base(AppStateId.StationsFound) { }

        public override void StateEnter(AppStateId previousStateId, AppStateData data) {
            string nextPart = data.Search.HasNext ? "Press 'n' for next search results. " : "";
            string prevPart = data.Search.HasPrevious ? "Press 'p' for previous search results. " : "";
            Console.WriteLine(nextPart + prevPart + "Type station id to start listening.");
        }

        public override AppStateId HandleEvent(string? stateEvent, AppStateData data) {
            if (stateEvent?.Trim() == "n" && data.Search.HasNext) {
                data.Search.GoNext();
                Refresh(data);
                return AppStateId.StationsFound;
            }

            if (stateEvent?.Trim() == "p" && data.Search.HasPrevious) {
                data.Search.GoPrevious();
                Refresh(data);
                return AppStateId.StationsFound;
            }

            return AppStateId.StationsSearch;
        }

        private void Refresh(AppStateData data) {
            if (data.Search.CurrentPage is null) {
                var stationResources = StationsSearchState.PerformSearch(data);
                data.Search.CurrentPage = stationResources;
                data.Search.Prefetch(); // Fire and forget by design.
            }

            StationsSearchState.PrintRadiosIfAny(data.Search.CurrentPage);
        }
    }
}