using System;
using System.Threading.Tasks;
using NLog;
using RadioFreeZerg.CuteRadio;

namespace RadioFreeZerg.States
{
    public class AppStateSearchData
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public CuteRadioStationResources? CurrentPage { get; set; }

        public CuteRadioStationResources? PreviousPage { get; set; }

        public CuteRadioStationResources? NextPage { get; set; }

        public CuteRadioStationSearchModel SearchModel { get; set; } = new();

        public bool HasNext => !string.IsNullOrWhiteSpace(CurrentPage?.Next);

        public bool HasPrevious => !string.IsNullOrWhiteSpace(CurrentPage?.Previous);

        public void GoNext() {
            if (HasNext) {
                CurrentPage = NextPage;
                SearchModel = SearchModel with { Offset = SearchModel.Offset + SearchModel.Limit };
            }
        }
        
        public void GoPrevious() {
            if (HasPrevious) {
                CurrentPage = PreviousPage;
                SearchModel = SearchModel with { Offset = SearchModel.Offset - SearchModel.Limit };
            }
        }

        public Task Prefetch() {
            if (CurrentPage is null) return Task.CompletedTask;

            var fetchPrevTask = Task.Run(() => {
                try {
                    PreviousPage ??= CurrentPage.FetchPreviousOrNullAsync().ConfigureAwait(false)
                                                .GetAwaiter().GetResult();
                } catch (Exception e) {
                    Log.Error(e, $"Exception when prefetching previous search page {CurrentPage?.Previous}");
                }
            });

            var fetchNextTask = Task.Run(() => {
                try {
                    NextPage ??= CurrentPage.FetchNextOrNullAsync().ConfigureAwait(false)
                                            .GetAwaiter().GetResult();
                } catch (Exception e) {
                    Log.Error(e, $"Exception when prefetching next search page {CurrentPage?.Next}");
                }
            });

            return Task.WhenAll(fetchPrevTask, fetchNextTask);
        }
    }
}