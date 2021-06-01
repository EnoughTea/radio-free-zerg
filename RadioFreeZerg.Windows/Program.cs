using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using LibVLCSharp.Shared;
using Terminal.Gui;

namespace RadioFreeZerg.Windows
{
    internal class Program
    {
        private static readonly Random Rng = new();
        
        private static void Main(string[] args) {
            Core.Initialize();
            var radioStationsProvider = new CuteRadioStationProviderJson("stations.json");
            var radioStations = new RadioStationManager(radioStationsProvider);
            var pagination = new RadioStationsPagination(radioStations.All);
            pagination.GoTo(Rng.Next(0, pagination.MaxPage + 1));

            Application.Init();
            var top = Application.Top;
            var mainWindow = new Window("Radio stations:") {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(mainWindow);
            var stationsListViewRef = new Reference<ListView>();
            var statusBar = CreateStatusBar(top, stationsListViewRef, radioStations, pagination);
            var stationsListView = CreateStationsListView(mainWindow, statusBar, radioStations, pagination);
            stationsListViewRef.Value = stationsListView;
            Application.Run();
        }

        private static StatusItem PrevItem(StatusBar statusBar) => statusBar.Items[0];
        private static StatusItem PagesItem(StatusBar statusBar) => statusBar.Items[1];
        private static StatusItem NextItem(StatusBar statusBar) => statusBar.Items[2];
        private static StatusItem PlayingItem(StatusBar statusBar) => statusBar.Items[5];

        private static StatusBar CreateStatusBar(View top,
                                                 Reference<ListView> stationsListView,
                                                 RadioStationManager radioStations,
                                                 RadioStationsPagination pagination) {
            var statusBar = new StatusBar();
            statusBar.Items = new StatusItem[] {
                new(Key.CtrlMask | Key.A, "",
                    () => {
                        pagination.Previous();
                        Debug.Assert(stationsListView.Value != null, "stationsListView.Value != null");
                        ChangeAvailableStations(statusBar, stationsListView.Value, pagination);
                    }),
                new(Key.CharMask, $"{pagination.CurrentPage}/{pagination.MaxPage}", null),
                new(Key.CtrlMask | Key.S, "",
                    () => {
                        pagination.Next();
                        Debug.Assert(stationsListView.Value != null, "stationsListView.Value != null");
                        ChangeAvailableStations(statusBar, stationsListView.Value, pagination);
                    }),
                new(Key.CtrlMask | Key.F, "~^F~ Find",
                    () => {
                        Debug.Assert(stationsListView.Value != null, "stationsListView.Value != null");
                        FindStations(statusBar, stationsListView.Value, radioStations, pagination);
                    }),
                new(Key.CtrlMask | Key.R, "~^R~ Random", () => {
                    var stationsToChooseFrom = pagination.AllStations;
                    if (stationsToChooseFrom.Count > 0) {
                        var rngStationIndex = Rng.Next(0, stationsToChooseFrom.Count);
                        var chosenStation = stationsToChooseFrom.ElementAt(rngStationIndex);
                        radioStations.TogglePlay(chosenStation);
                    }
                }),
                new(Key.CharMask, "Nothing is playing", null),
                new(Key.CtrlMask | Key.Q, "~^Q~ Quit", Application.RequestStop)
            };
            top.Add(statusBar);

            radioStations.NowPlayingChanged += nowPlaying => {
                var hasNowPlaying = radioStations.CurrentStation != RadioStation.Empty &&
                    !string.IsNullOrEmpty(nowPlaying);
                var stationTitle = $"{radioStations.CurrentStation.Id}: {radioStations.CurrentStation.Title}";
                PlayingItem(statusBar).Title = hasNowPlaying ? $"'{nowPlaying}' at {stationTitle}" : $"Unknown song at {stationTitle}";
                statusBar.SetNeedsDisplay();
            };

            return statusBar;
        }

        private static void FindStations(StatusBar statusBar,
                                         ListView stationsListView,
                                         RadioStationManager radioStations,
                                         RadioStationsPagination pagination) {
            var (input, canceled) = InputPrompt.Display("Enter title keywords or station ID", "Go");
            if (!canceled) {
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    pagination.AllStations = new[] {radioStations.Find(id)};
                else
                    pagination.AllStations = radioStations.Find(input).ToList();

                ChangeAvailableStations(statusBar, stationsListView, pagination);
            }
        }

        private static void ChangeAvailableStations(StatusBar statusBar,
                                                    ListView stationsListView,
                                                    RadioStationsPagination pagination) {
            PrevItem(statusBar).Title = pagination.HasPrevious() ? "~^A~ Previous" : "No previous items";
            NextItem(statusBar).Title = pagination.HasNext() ? "~^S~ Next" : "No next";
            PagesItem(statusBar).Title = $"{pagination.CurrentPage}/{pagination.MaxPage}";
            statusBar.SetNeedsDisplay();
            stationsListView.Source = new RadioStationListSource(pagination.CurrentPageStations);
        }

        private static ListView CreateStationsListView(View window,
                                                       StatusBar statusBar,
                                                       RadioStationManager radioStations,
                                                       RadioStationsPagination pagination) {
            var stationsListView = new ListView {
                X = 1,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            stationsListView.OpenSelectedItem +=
                eventArgs => radioStations.TogglePlay((RadioStation) eventArgs.Value);
            window.Add(stationsListView);
            ChangeAvailableStations(statusBar, stationsListView, pagination);
            GuiHelper.SetupScrollBars(stationsListView);
            return stationsListView;
        }

        public class Reference<T>
        {
            public T? Value { get; set; }
        }
    }
}