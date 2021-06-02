using System;
using System.Globalization;
using System.Linq;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace RadioFreeZerg.Gui
{
    public class MainScreen
    {
        private static readonly Random Rng = new();
        private readonly View mainWindow;
        private readonly Label nowPlayingLabel;
        private readonly RadioStationsPagination pagination;
        private readonly RadioStationManager radioStations;
        private readonly ListView stationsListView;
        private readonly StatusBar statusBar;

        private readonly Toplevel top;

        public MainScreen(RadioStationManager radioStationManager) {
            radioStations = radioStationManager;
            pagination = new RadioStationsPagination(radioStations.All);
            pagination.GoTo(Rng.Next(0, pagination.MaxPage + 1));

            top = Application.Top;
            mainWindow = CreateMainWindow("Radio stations:");
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
            pagination.Previous();
            RefreshStationList();
        }

        public void ListNextStations() {
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
                radioStations.Play(chosenStation);
                RefreshNowPlaying();
            }
        }

        public void RefreshNowPlaying() {
            var stationTitle = radioStations.CurrentStation != RadioStation.Empty
                ? $"{radioStations.CurrentStation.Id}: {radioStations.CurrentStation.Title}"
                : "";
            var hasNowPlaying = radioStations.CurrentStation != RadioStation.Empty &&
                !string.IsNullOrWhiteSpace(radioStations.NowPlaying) &&
                radioStations.NowPlaying.Trim().Length > 2;
            nowPlayingLabel.Text = hasNowPlaying
                ? $"'{radioStations.NowPlaying}' at {stationTitle}"
                : !string.IsNullOrEmpty(stationTitle)
                    ? $"Unknown song at {stationTitle}"
                    : "Nothing is playing";
            mainWindow.SetNeedsDisplay();
        }

        public void RefreshStationList() {
            PrevItem.Title = pagination.HasPrevious() ? "~^A~ Previous" : "No previous items";
            NextItem.Title = pagination.HasNext() ? "~^S~ Next" : "No next";
            PagesItem.Title = $"{pagination.CurrentPage}/{pagination.MaxPage}";
            statusBar.SetNeedsDisplay();
            stationsListView.Source = new RadioStationListSource(pagination.CurrentPageStations);
        }

        public void PromptFindStations() {
            var (input, canceled) = InputPrompt.Display("Enter title keywords or station ID", "Go");
            if (!canceled) {
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    pagination.AllStations = new[] {radioStations.Find(id)};
                else
                    pagination.AllStations = radioStations.Find(input).ToList();

                RefreshStationList();
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
                Text = "Nothing is playing"
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
                    new(Key.CtrlMask | Key.A, "", ListPreviousStations),
                    new(Key.CharMask, $"{pagination.CurrentPage}/{pagination.MaxPage}", null),
                    new(Key.CtrlMask | Key.S, "", ListNextStations),
                    new(Key.CtrlMask | Key.F, "~^F~ Find", PromptFindStations),
                    new(Key.CtrlMask | Key.R, "~^R~ Random", PlayRandomStation),
                    new(Key.CtrlMask | Key.T, "~^T~ Toggle", ToggleCurrentStation),
                    new(Key.CtrlMask | Key.Q, "~^Q~ Quit", Application.RequestStop)
                }
            };
    }
}