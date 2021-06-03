using NLog;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class NowPlayingComponent : GuiComponent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Label nowPlayingLabel;

        public NowPlayingComponent(RadioStationManager radioStationManager,
                                   UserState userState,
                                   Window window,
                                   StationsListComponent stationsListComponent)
            : base(radioStationManager, userState, window) {
            var nowPlayingColors = new ColorScheme {
                Normal = new Attribute(Color.BrightYellow, window.ColorScheme.Normal.Background)
            };
            nowPlayingLabel = new Label {
                X = 1,
                Y = Pos.Bottom(stationsListComponent.StationsListView),
                Width = Dim.Fill(1),
                Height = 1,
                CanFocus = false,
                ColorScheme = nowPlayingColors,
                Text = RadioFreeZerg.MainScreen.NowPlayingNothingText
            };
            window.Add(nowPlayingLabel);
            RadioStations.NowPlayingChanged += _ => Refresh();
        }

        public void ToggleCurrentStation() {
            RadioStations.TogglePlay(RadioStation.Empty);
            Refresh();
        }

        public virtual void Refresh() {
            Log.Trace($"Triggered NowPlaying refresh for: {RadioStations.NowPlaying}");
            var stationTitle = RadioStations.CurrentStation != RadioStation.Empty
                ? $"{RadioStations.CurrentStation.Id}: {RadioStations.CurrentStation.Title}"
                : "";
            var hasNowPlaying = RadioStations.CurrentStation != RadioStation.Empty &&
                !string.IsNullOrWhiteSpace(RadioStations.NowPlaying) &&
                RadioStations.NowPlaying.Trim().Length > 2;
            nowPlayingLabel.Text = hasNowPlaying
                ? string.Format(RadioFreeZerg.MainScreen.NowPlayingSongText, RadioStations.NowPlaying, stationTitle)
                : !string.IsNullOrEmpty(stationTitle)
                    ? string.Format(RadioFreeZerg.MainScreen.NowPlayingUnknownText, stationTitle)
                    : RadioFreeZerg.MainScreen.NowPlayingNothingText;
            Window.SetNeedsDisplay();

            State.ToggledStationId = RadioStations.ToggledStation.Id;
            State.CurrentStationId = RadioStations.CurrentStation.Id;
            State.Save();
        }
    }
}