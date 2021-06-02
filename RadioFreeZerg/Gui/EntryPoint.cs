using LibVLCSharp.Shared;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public static class EntryPoint
    {
        private static MainScreen? mainScreen;

        public static void Main(string[] args) {
            Core.Initialize();
            Application.Init();

            var radioStationsProvider = new CuteRadioStationProviderJson("stations.json");
            var radioStations = new RadioStationManager(radioStationsProvider);

            mainScreen = new MainScreen(radioStations);
            mainScreen.RefreshStationList();

            Application.Run();
        }
    }
}