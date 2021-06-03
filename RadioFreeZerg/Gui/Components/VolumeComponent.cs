using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class VolumeComponent : GuiComponent
    {
        private const int VolumeIncrement = 1;

        public VolumeComponent(RadioStationManager radioStationManager,
                               UserState userState,
                               Window window) : base(radioStationManager, userState, window) { }

        public StatusBarComponent? StatusBar { get; set; }

        public void VolumeDown() {
            RadioStations.Volume -= VolumeIncrement;
            SaveState();
            Refresh();
        }

        public void VolumeUp() {
            RadioStations.Volume += VolumeIncrement;
            SaveState();
            Refresh();
        }

        private void SaveState() {
            State.Volume = RadioStations.Volume;
            State.Save();
        }

        public virtual void Refresh() {
            if (StatusBar != null) {
                StatusBar.CurrentVolumeItem.Title = $"{RadioStations.Volume} %";
                StatusBar.Refresh();
            }
        }
    }
}