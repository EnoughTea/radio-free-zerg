using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using LibVLCSharp.Shared;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace RadioFreeZerg.Gui
{
    public static class EntryPoint
    {
        private static readonly Random Rng = new();

        public static void Main(string[] args) {
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
            var nowPlayingLabelRef = new Reference<Label>();
            var statusBar = CreateStatusBar(top, mainWindow, stationsListViewRef, nowPlayingLabelRef, radioStations,
                pagination);
            var stationsListView =
                CreateStationsListView(mainWindow, statusBar, nowPlayingLabelRef, radioStations, pagination);
            stationsListViewRef.Value = stationsListView;
            var nowPlayingLabel = CreateNowPlaying(mainWindow, stationsListView);
            nowPlayingLabelRef.Value = nowPlayingLabel;

            Application.Run();
        }
        
        private static Label CreateNowPlaying(View mainWindow, ListView stationsListView) {
            var nowPlayingColors = new ColorScheme {
                Normal = new Attribute(Color.BrightYellow, stationsListView.ColorScheme.Normal.Background)
            };
            var nowPlayingLabel = new Label {
                X = 1,
                Y = Pos.Bottom(stationsListView) - 1,
                Width = Dim.Fill(1),
                Height = 1,
                CanFocus = false,
                ColorScheme = nowPlayingColors,
                Text = "Nothing is playing"
            };
            mainWindow.Add(nowPlayingLabel);
            return nowPlayingLabel;
        }

        private static StatusBar CreateStatusBar(View top,
                                                 View mainWindow,
                                                 Reference<ListView> stationsListView,
                                                 Reference<Label> nowPlayingLabel,
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
                        radioStations.Play(chosenStation);
                        RefreshNowPlaying(mainWindow, nowPlayingLabel, radioStations);
                    }
                }),
                new(Key.CtrlMask | Key.T, "~^T~ Toggle", () => {
                    radioStations.TogglePlay(RadioStation.Empty);
                    RefreshNowPlaying(mainWindow, nowPlayingLabel, radioStations);
                }),
                new(Key.CtrlMask | Key.Q, "~^Q~ Quit", Application.RequestStop)
            };
            top.Add(statusBar);

            radioStations.NowPlayingChanged += nowPlaying => {
                RefreshNowPlaying(mainWindow, nowPlayingLabel, radioStations);
                var hasNowPlaying = radioStations.CurrentStation != RadioStation.Empty &&
                    !string.IsNullOrEmpty(nowPlaying);
                var stationTitle = $"{radioStations.CurrentStation.Id}: {radioStations.CurrentStation.Title}";
                if (nowPlayingLabel.Value != null) {
                    nowPlayingLabel.Value.Text = hasNowPlaying
                        ? $"'{nowPlaying}' at {stationTitle}"
                        : $"Unknown song at {stationTitle}";
                    mainWindow.SetNeedsDisplay();
                }
            };

            return statusBar;
        }

        private static StatusItem PrevItem(StatusBar statusBar) => statusBar.Items[0];
        private static StatusItem PagesItem(StatusBar statusBar) => statusBar.Items[1];
        private static StatusItem NextItem(StatusBar statusBar) => statusBar.Items[2];

        private static ListView CreateStationsListView(View mainWindow,
                                                       StatusBar statusBar,
                                                       Reference<Label> nowPlayingLabelRef,
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

            stationsListView.OpenSelectedItem += eventArgs => {
                radioStations.TogglePlay((RadioStation) eventArgs.Value);
                RefreshNowPlaying(mainWindow, nowPlayingLabelRef, radioStations);
            };
            mainWindow.Add(stationsListView);
            ChangeAvailableStations(statusBar, stationsListView, pagination);
            GuiHelper.SetupScrollBars(stationsListView);
            return stationsListView;
        }

        private static void RefreshNowPlaying(View mainWindow,
                                              Reference<Label> nowPlayingLabel,
                                              RadioStationManager radioStations) {
            if (nowPlayingLabel.Value != null) {
                var stationTitle = radioStations.CurrentStation != RadioStation.Empty
                    ? $"{radioStations.CurrentStation.Id}: {radioStations.CurrentStation.Title}"
                    : "";
                var hasNowPlaying = radioStations.CurrentStation != RadioStation.Empty &&
                    !string.IsNullOrWhiteSpace(radioStations.NowPlaying) &&
                    radioStations.NowPlaying.Trim().Length > 2;
                nowPlayingLabel.Value.Text = hasNowPlaying
                    ? $"'{radioStations.NowPlaying}' at {stationTitle}"
                    : !string.IsNullOrEmpty(stationTitle)
                        ? $"Unknown song at {stationTitle}"
                        : "Nothing is playing";
                mainWindow.SetNeedsDisplay();
            }
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
    }
}