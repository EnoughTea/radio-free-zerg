using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class StatusBarComponent : GuiComponent
    {
        private readonly StatusBar statusBar;

        public StatusBarComponent(RadioStationManager radioStationManager, UserState userState, Window window, View top)
            : base(radioStationManager, userState, window) {
            statusBar = new StatusBar() {
                Items = new StatusItem[] {
                    new(Key.CtrlMask | Key.S, "", () => StationsList?.ListPreviousStations()),
                    new(Key.CharMask, @$"{(StationsList != null
                        ? $"{StationsList.Pagination.CurrentPage}/{StationsList.Pagination.MaxPage}"
                        : "1/1")}", null),
                    new(Key.CtrlMask | Key.D, "", () => StationsList?.ListNextStations()),
                    new(Key.CtrlMask | Key.F, $"~^F~ {RadioFreeZerg.MainScreen.FindStationsText}",
                        () => FindStations?.Prompt()),
                    new(Key.CtrlMask | Key.R, $"~^R~ {RadioFreeZerg.MainScreen.PlayRandomStationText}",
                        () => StationsList?.PlayRandomStation()),
                    new(Key.CtrlMask | Key.T, $"~^T~ {RadioFreeZerg.MainScreen.ToggleCurrentStationText}",
                        () => NowPlaying?.ToggleCurrentStation()),
                    new(Key.CtrlMask | Key.B, $"~^B~ {RadioFreeZerg.MainScreen.VolumeDownText}",
                        () => Volume?.VolumeDown()),
                    new(Key.CharMask, "", null),
                    new(Key.CtrlMask | Key.G, $"~^G~ {RadioFreeZerg.MainScreen.VolumeUpText}",
                        () => Volume?.VolumeUp()),
                    new(Key.CtrlMask | Key.Q, $"~^Q~ {RadioFreeZerg.MainScreen.QuitAppText}", Application.RequestStop)
                }
            };
            top.Add(statusBar);
        }

        public StatusItem PrevItem => statusBar.Items[0];
        public StatusItem PagesItem => statusBar.Items[1];
        public StatusItem NextItem => statusBar.Items[2];
        public StatusItem CurrentVolumeItem => statusBar.Items[7];

        public VolumeComponent? Volume { get; set; }

        public NowPlayingComponent? NowPlaying { get; set; }

        public StationsListComponent? StationsList { get; set; }

        public FindStationsComponent? FindStations { get; set; }

        public virtual void Refresh() {
            if (StationsList != null) {
                PrevItem.Title = StationsList.Pagination.HasPrevious()
                    ? $"~^S~ {RadioFreeZerg.MainScreen.PrevItemsText}"
                    : RadioFreeZerg.MainScreen.NoPreviousItemsText;
                NextItem.Title = StationsList.Pagination.HasNext()
                    ? $"~^D~ {RadioFreeZerg.MainScreen.NextItemsText}"
                    : RadioFreeZerg.MainScreen.NoNextItemsText;
                PagesItem.Title = $"{StationsList.Pagination.CurrentPage + 1}/{StationsList.Pagination.MaxPage + 1}";
            }
            
            statusBar.SetNeedsDisplay();
        }
    }
}