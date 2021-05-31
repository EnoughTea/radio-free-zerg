using System.Linq;
using LibVLCSharp.Shared;
using Terminal.Gui;

namespace RadioFreeZerg.Windows
{
    internal class Program
    {
        private static bool Quit() {
            var result = MessageBox.Query(50, 7, "Quit this application",
                "Are you sure you want to quit?", "Yes", "No");
            return result == 0;
        }

        private static void Main(string[] args) {
            Core.Initialize();
            var radioStationsProvider = new CuteRadioStationProviderJson("stations.json");
            var radioStations = new RadioStationManager(radioStationsProvider, new AudioPlayer());

            Application.Init();
            var top = Application.Top;
            var mainWindow = new Window("RadioFreeZerg") {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(mainWindow);

            var stationsListView = CreateStationsListView(mainWindow, radioStations);
            CreateStatusBar(top, stationsListView, radioStations);
            Application.Run();
        }

        private static void CreateStatusBar(View top, ListView stationsListView, RadioStationManager radioStations) {
            var statusBar = new StatusBar(new StatusItem[] {
                new(Key.CtrlMask | Key.F, "~^F~ Find stations",
                    () => {
                        var (input, canceled) = InputPrompt.Display("This is a test prompt", "Push", "Something");
                    })
            });
            top.Add(statusBar);
        }

        private static ListView CreateStationsListView(View window, RadioStationManager radioStations) {
            var stationsTitle = new Label(1, 0, "Stations:");
            window.Add(stationsTitle);

            var stationsListView = new ListView {
                X = 1,
                Y = Pos.Bottom(stationsTitle) + 1,
                Height = Dim.Fill(),
                Width = Dim.Fill(1),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            stationsListView.OpenSelectedItem += eventArgs => {
                var station = (RadioStation) eventArgs.Value;
                if (radioStations.CurrentStation == station) radioStations.Stop();
                else radioStations.Play(station);
            };
            window.Add(stationsListView);
            stationsListView.Source = new RadioStationListSource(radioStations.Take(20));
            SetupScrollBars(stationsListView);
            return stationsListView;
        }

        private static void SetupScrollBars(ListView stationsListView) {
            var stationsScrollBar = new ScrollBarView(stationsListView, true);

            stationsScrollBar.ChangedPosition += () => {
                stationsListView.TopItem = stationsScrollBar.Position;
                if (stationsListView.TopItem != stationsScrollBar.Position)
                    stationsScrollBar.Position = stationsListView.TopItem;
                stationsListView.SetNeedsDisplay();
            };

            stationsScrollBar.OtherScrollBarView.ChangedPosition += () => {
                stationsListView.LeftItem = stationsScrollBar.OtherScrollBarView.Position;
                if (stationsListView.LeftItem != stationsScrollBar.OtherScrollBarView.Position)
                    stationsScrollBar.OtherScrollBarView.Position = stationsListView.LeftItem;
                stationsListView.SetNeedsDisplay();
            };

            stationsListView.DrawContent += _ => {
                stationsScrollBar.Size = stationsListView.Source.Count - 1;
                stationsScrollBar.Position = stationsListView.TopItem;
                stationsScrollBar.OtherScrollBarView.Size = stationsListView.Maxlength - 1;
                stationsScrollBar.OtherScrollBarView.Position = stationsListView.LeftItem;
                stationsScrollBar.Refresh();
            };
        }
    }
}