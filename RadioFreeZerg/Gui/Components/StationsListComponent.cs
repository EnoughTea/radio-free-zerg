using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class StationsListComponent : GuiComponent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Func<ISet<int>> backingStationIds;

        public StationsListComponent(RadioStationManager radioStationManager,
                                     UserState userState,
                                     Window window,
                                     Func<ISet<int>> getStateBackingStationIds)
            : base(radioStationManager, userState, window) {
            backingStationIds = getStateBackingStationIds;

            Pagination = new RadioStationsPagination();
            StationsListView = new ListView() {
                X = 1,
                Y = 0,
                Height = Dim.Fill(1),
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };
            window.Add(StationsListView);
            GuiHelper.SetupScrollBars(StationsListView);

            StationsListView.OpenSelectedItem += args => {
                RadioStations.TogglePlay((RadioStation) args.Value);
                NowPlaying?.Refresh();
            };
        }

        public ListView StationsListView { get; }

        public RadioStationsPagination Pagination { get; }

        public NowPlayingComponent? NowPlaying { get; set; }

        public StatusBarComponent? StatusBar { get; set; }

        public void SetStations(IReadOnlyCollection<RadioStation> stations) {
            Pagination.AllStations = stations;
            Pagination.GoTo(0);
            Refresh();
            SaveState(true);
        }

        public void ListPreviousStations() {
            Log.Trace("Listing previous stations.");
            Pagination.Previous();
            Refresh();
        }

        public void ListNextStations() {
            Log.Trace("Listing next stations.");
            Pagination.Next();
            Refresh();
        }

        public void PlayRandomStation() {
            var stationsToChooseFrom = Pagination.AllStations;
            if (stationsToChooseFrom.Count > 0) {
                var rngStationIndex = Rng.Next(0, stationsToChooseFrom.Count);
                var chosenStation = stationsToChooseFrom.ElementAt(rngStationIndex);
                Log.Info($"Requested to play random station, chosen station ${chosenStation.Id}.");
                RadioStations.Play(chosenStation);
                NowPlaying?.Refresh();
            } else {
                Log.Info("Requested to play random station, but no stations were listed.");
            }
        }

        public virtual void Refresh() {
            Log.Trace($"Triggered station list refresh for {Pagination.CurrentPageStations} stations " +
                $"at page {Pagination.CurrentPage + 1}.");
            StationsListView.Source = new RadioStationListSource(Pagination.CurrentPageStations, State);

            if (StatusBar != null) {
                StatusBar.PrevItem.Title = Pagination.HasPrevious()
                    ? $"~^S~ {RadioFreeZerg.MainScreen.PrevItemsText}"
                    : RadioFreeZerg.MainScreen.NoPreviousItemsText;
                StatusBar.NextItem.Title = Pagination.HasNext()
                    ? $"~^D~ {RadioFreeZerg.MainScreen.NextItemsText}"
                    : RadioFreeZerg.MainScreen.NoNextItemsText;
                StatusBar.PagesItem.Title = $"{Pagination.CurrentPage + 1}/{Pagination.MaxPage + 1}";
                StatusBar.Refresh();
            }

            SaveState(false);
        }

        private void SaveState(bool writeBackingStationIds) {
            if (writeBackingStationIds) {
                var stateStationIds = backingStationIds();
                stateStationIds.Clear();
                foreach (var station in Pagination.AllStations) {
                    stateStationIds.Add(station.Id);
                }
            }

            State.CurrentPage = Pagination.CurrentPage;
            State.Save();
        }
    }
}