﻿using System;
using System.Globalization;
using System.Linq;
using NLog;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace RadioFreeZerg.Gui
{
    public class MainScreen
    {
        private static readonly Random Rng = new();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly View mainWindow;
        private readonly Label nowPlayingLabel;
        private readonly RadioStationsPagination pagination;
        private readonly RadioStationManager radioStations;
        private readonly ListView stationsListView;
        private readonly StatusBar statusBar;

        public MainScreen(RadioStationManager radioStationManager) {
            radioStations = radioStationManager;
            pagination = new RadioStationsPagination(radioStations.All);
            pagination.GoTo(Rng.Next(0, pagination.MaxPage + 1));

            Toplevel top = Application.Top;
            mainWindow = CreateMainWindow(RadioFreeZerg.MainScreen.MainScreenTitleText);
            top.Add(mainWindow);

            stationsListView = CreateStationsListView();
            mainWindow.Add(stationsListView);
            GuiHelper.SetupScrollBars(stationsListView);

            nowPlayingLabel = CreateNowPlaying(stationsListView);
            mainWindow.Add(nowPlayingLabel);

            statusBar = CreateStatusBar();
            top.Add(statusBar);

            stationsListView.OpenSelectedItem += args => {
                radioStations.TogglePlay((RadioStation) args.Value);
                RefreshNowPlaying();
            };
            radioStations.NowPlayingChanged += _ => RefreshNowPlaying();
        }

        private StatusItem PrevItem => statusBar.Items[0];
        private StatusItem PagesItem => statusBar.Items[1];
        private StatusItem NextItem => statusBar.Items[2];

        public void ListPreviousStations() {
            Log.Trace("Listing previous stations.");
            pagination.Previous();
            RefreshStationList();
        }

        public void ListNextStations() {
            Log.Trace("Listing next stations.");
            pagination.Next();
            RefreshStationList();
        }

        public void ToggleCurrentStation() {
            radioStations.TogglePlay(RadioStation.Empty);
            RefreshNowPlaying();
        }

        public void PlayRandomStation() {
            var stationsToChooseFrom = pagination.AllStations;
            if (stationsToChooseFrom.Count > 0) {
                var rngStationIndex = Rng.Next(0, stationsToChooseFrom.Count);
                var chosenStation = stationsToChooseFrom.ElementAt(rngStationIndex);
                Log.Info($"Requested to play random station, chosen station ${chosenStation.Id}.");
                radioStations.Play(chosenStation);
                RefreshNowPlaying();
            } else {
                Log.Info("Requested to play random station, but no stations were listed.");
            }
        }

        public void RefreshNowPlaying() {
            Log.Trace("Triggered NowPlaying label refresh.");
            var stationTitle = radioStations.CurrentStation != RadioStation.Empty
                ? $"{radioStations.CurrentStation.Id}: {radioStations.CurrentStation.Title}"
                : "";
            var hasNowPlaying = radioStations.CurrentStation != RadioStation.Empty &&
                !string.IsNullOrWhiteSpace(radioStations.NowPlaying) &&
                radioStations.NowPlaying.Trim().Length > 2;
            nowPlayingLabel.Text = hasNowPlaying
                ? string.Format(RadioFreeZerg.MainScreen.NowPlayingSongText, radioStations.NowPlaying, stationTitle)
                : !string.IsNullOrEmpty(stationTitle)
                    ? string.Format(RadioFreeZerg.MainScreen.NowPlayingUnknownText, stationTitle)
                    : RadioFreeZerg.MainScreen.NowPlayingNothingText;
            mainWindow.SetNeedsDisplay();
        }

        public void RefreshStationList() {
            Log.Trace($"Triggered station list refresh for {pagination.CurrentPageStations} stations " +
                $"at page {pagination.CurrentPage + 1}.");
            PrevItem.Title = pagination.HasPrevious()
                ? $"~^S~ {RadioFreeZerg.MainScreen.PrevItemsText}"
                : RadioFreeZerg.MainScreen.NoPreviousItemsText;
            NextItem.Title = pagination.HasNext()
                ? $"~^D~ {RadioFreeZerg.MainScreen.NextItemsText}"
                : RadioFreeZerg.MainScreen.NoNextItemsText;
            PagesItem.Title = $"{pagination.CurrentPage + 1}/{pagination.MaxPage + 1}";
            statusBar.SetNeedsDisplay();
            stationsListView.Source = new RadioStationListSource(pagination.CurrentPageStations);
        }

        public void PromptFindStations() {
            Log.Debug("Showing find stations prompt...");
            var (input, canceled) = InputPrompt.Display(RadioFreeZerg.MainScreen.FindStationsPromptText,
                RadioFreeZerg.MainScreen.FindStationsConfirmationText, RadioFreeZerg.MainScreen.FindStationsCancelText);
            if (!canceled) {
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) {
                    pagination.AllStations = new[] {radioStations.Find(id)};
                } else {
                    var foundStations = radioStations.Find(input).ToList();
                    Log.Info($"Found {foundStations.Count} stations.");
                    pagination.AllStations = foundStations;
                }

                RefreshStationList();
            } else {
                Log.Debug("Canceled find stations prompt.");
            }
        }

        private static View CreateMainWindow(string title) =>
            new Window(title) {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

        private static Label CreateNowPlaying(ListView stationsListView) {
            var nowPlayingColors = new ColorScheme {
                Normal = new Attribute(Color.BrightYellow, stationsListView.ColorScheme.Normal.Background)
            };
            return new Label {
                X = 1,
                Y = Pos.Bottom(stationsListView),
                Width = Dim.Fill(1),
                Height = 1,
                CanFocus = false,
                ColorScheme = nowPlayingColors,
                Text = RadioFreeZerg.MainScreen.NowPlayingNothingText
            };
        }

        private static ListView CreateStationsListView() =>
            new() {
                X = 1,
                Y = 0,
                Height = Dim.Fill() - 1,
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

        private StatusBar CreateStatusBar() =>
            new() {
                Items = new StatusItem[] {
                    new(Key.CtrlMask | Key.S, "", ListPreviousStations),
                    new(Key.CharMask, $"{pagination.CurrentPage}/{pagination.MaxPage}", null),
                    new(Key.CtrlMask | Key.D, "", ListNextStations),
                    new(Key.CtrlMask | Key.G, $"~^G~ {RadioFreeZerg.MainScreen.FindStationsText}", PromptFindStations),
                    new(Key.CtrlMask | Key.R, $"~^R~ {RadioFreeZerg.MainScreen.PlayRandomStationText}",
                        PlayRandomStation),
                    new(Key.CtrlMask | Key.T, $"~^T~ {RadioFreeZerg.MainScreen.ToggleCurrentStationText}",
                        ToggleCurrentStation),
                    new(Key.CtrlMask | Key.Q, $"~^Q~ {RadioFreeZerg.MainScreen.QuitAppText}", Application.RequestStop)
                }
            };
    }
}