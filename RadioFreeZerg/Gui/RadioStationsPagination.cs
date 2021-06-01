using System;
using System.Collections.Generic;
using System.Linq;

namespace RadioFreeZerg.Gui
{
    public class RadioStationsPagination
    {
        private readonly int limit;
        private IReadOnlyCollection<RadioStation> allStations;

        public RadioStationsPagination() : this(Array.Empty<RadioStation>()) { }

        public RadioStationsPagination(IReadOnlyCollection<RadioStation> allRadioStations, int pageLimit = 20) {
            allStations = allRadioStations;
            limit = pageLimit;
            GoTo(0);
        }

        public IReadOnlyCollection<RadioStation> CurrentPageStations { get; private set; } =
            Array.Empty<RadioStation>();

        public IReadOnlyCollection<RadioStation> AllStations {
            get => allStations;
            set {
                allStations = value;
                GoTo(0);
            }
        }

        public int MaxPage => (int) Math.Max(0, Math.Ceiling(allStations.Count / (float) limit) - 1);

        public int CurrentPage { get; private set; }

        public bool HasNext() => CurrentPage * limit + limit < allStations.Count;

        public bool HasPrevious() => CurrentPage > 0;

        public bool Next() {
            if (!HasNext()) return false;

            CurrentPage++;
            UpdateCurrentPageStations();
            return true;
        }

        public bool Previous() {
            if (!HasPrevious()) return false;

            CurrentPage--;
            UpdateCurrentPageStations();
            return true;
        }

        public bool GoTo(int page) {
            if (page < 0 || page > MaxPage) return false;

            CurrentPage = page;
            UpdateCurrentPageStations();
            return true;
        }

        private void UpdateCurrentPageStations() => CurrentPageStations = Page(CurrentPage).ToList();

        private IEnumerable<RadioStation> Page(int page) => allStations.Skip(page * limit).Take(limit);
    }
}