using System.Globalization;
using System.Linq;
using NLog;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class FindStationsComponent : GuiComponent
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public FindStationsComponent(RadioStationManager radioStationManager,
                                     UserState userState,
                                     Window window) : base(radioStationManager, userState, window) { }

        public StationsListComponent? StationsList { get; set; }

        public void Prompt() {
            if (StationsList == null) {
                Log.Error("No assigned StationsListComponent for this FindStationsComponent");
                return;
            }

            Log.Debug("Showing find stations prompt...");
            var (input, canceled) = InputPrompt.Display(RadioFreeZerg.MainScreen.FindStationsPromptText,
                RadioFreeZerg.MainScreen.FindStationsConfirmationText, RadioFreeZerg.MainScreen.FindStationsCancelText);
            if (!canceled) {
                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) {
                    StationsList.SetStations(new[] {RadioStations.Find(id)});
                } else {
                    var foundStations = RadioStations.Find(input).ToList();
                    Log.Info($"Found {foundStations.Count} stations.");
                    StationsList.SetStations(foundStations);
                }
            } else {
                Log.Debug("Canceled find stations prompt.");
            }
        }
    }
}