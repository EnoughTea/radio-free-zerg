using System;
using System.Collections.Generic;
using System.Linq;

namespace RadioFreeZerg.Windows
{
    public class RadioStationsPagination
    {
        private readonly int limit;
        private IReadOnlyCollection<RadioStation> allStations;
        private int currentPage;

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

        public int MaxPage => (int) Math.Ceiling(allStations.Count / (float) limit) - 1;

        public bool HasNext() => currentPage * limit + limit < allStations.Count;

        public bool HasPrevious() => currentPage > 0;

        public bool Next() {
            if (!HasNext()) return false;

            currentPage++;
            UpdateCurrentPageStations();
            return true;
        }

        public bool Previous() {
            if (!HasPrevious()) return false;

            currentPage--;
            UpdateCurrentPageStations();
            return true;
        }

        public bool GoTo(int page) {
            if (page < 0 || page > MaxPage) return false;

            currentPage = page;
            UpdateCurrentPageStations();
            return true;
        }

        private void UpdateCurrentPageStations() => CurrentPageStations = Page(currentPage).ToList();

        private IEnumerable<RadioStation> Page(int page) => allStations.Skip(page * limit).Take(limit);
    }
}