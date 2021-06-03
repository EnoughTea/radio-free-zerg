using System.Linq;
using NLog;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class MainScreen
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public MainScreen(RadioStationManager radioStations, UserState state) {
            Toplevel top = Application.Top;
            Log.Debug("Creating main window...");
            Window window = CreateMainWindow(RadioFreeZerg.MainScreen.MainScreenTitleText);
            top.Add(window);

            Log.Debug("Creating components...");
            FindStations = new FindStationsComponent(radioStations, state, window);
            StationsList = new StationsListComponent(radioStations, state, window,
                () => state.AvailableStationsIds);
            NowPlaying = new NowPlayingComponent(radioStations, state, window, StationsList);
            StatusBar = new StatusBarComponent(radioStations, state, window, top);
            Volume = new VolumeComponent(radioStations, state, window);

            Log.Debug("Assigning component dependencies...");   // Eh, maybe use some container?
            StationsList.NowPlaying = NowPlaying;
            StationsList.StatusBar = StatusBar;
            StatusBar.Volume = Volume;
            StatusBar.FindStations = FindStations;
            StatusBar.NowPlaying = NowPlaying;
            StatusBar.StationsList = StationsList;
            FindStations.StationsList = StationsList;
            Volume.StatusBar = StatusBar;

            Log.Debug("Setting initial stations...");
            Volume.Refresh();
            StationsList.SetStations(state.AvailableStationsIds.Count == 0
                ? radioStations.All
                : radioStations.Where(_ => state.AvailableStationsIds.Contains(_.Id)).ToList());
        }

        public FindStationsComponent FindStations { get; }
        public NowPlayingComponent NowPlaying { get; }
        public StationsListComponent StationsList { get; }
        public StatusBarComponent StatusBar { get; }
        public VolumeComponent Volume { get; }

        private static Window CreateMainWindow(string title) =>
            new(title) {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
    }
}