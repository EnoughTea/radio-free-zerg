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

        public StationsListComponent(RadioStationManager radioStationManager,
                                     UserState userState,
                                     Window window)
            : base(radioStationManager, userState, window) {
            Pagination = new RadioStationsPagination();
            StationsListView = new ListView {
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

        public NowPlayingComponent? NowPlaying { get; set; }

        public StatusBarComponent? StatusBar { get; set; }

        public ListView StationsListView { get; }

        public RadioStationsPagination Pagination { get; }

        public void SetStations(IReadOnlyCollection<RadioStation> stations, int page = 0) {
            Pagination.AllStations = stations;
            Pagination.GoTo(page);
            Refresh();
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
            StatusBar?.Refresh();
            SaveState(false);
        }

        public void SaveState(bool writeBackingStationIds) {
            if (writeBackingStationIds) {
                State.AvailableStationsIds.Clear();
                foreach (var station in Pagination.AllStations) {
                    State.AvailableStationsIds.Add(station.Id);
                }
            }

            State.CurrentPage = Pagination.CurrentPage;
            State.Save();
        }
    }
}