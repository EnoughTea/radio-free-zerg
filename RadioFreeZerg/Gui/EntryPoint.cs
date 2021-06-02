using System;
using LibVLCSharp.Shared;
using NLog;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public static class EntryPoint
    {
        private static MainScreen? mainScreen;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args) {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

            Log.Info("RFG started.");
            Core.Initialize();
            Log.Debug("LibVLC initialized.");
            Application.Init();
            Log.Debug("Terminal.Gui initialized.");
            
            var radioStationsProvider = new CuteRadioStationProviderJson("stations.json");
            Log.Debug($"Created radio stations provider with {radioStationsProvider.All().Count} stations.");
            var radioStations = new RadioStationManager(radioStationsProvider);
            Log.Debug("Created radio stations manager.");
            
            Log.Debug("Creating main screen...");
            var userSettings = UserState.Load();
            mainScreen = new MainScreen(radioStations, userSettings);
            mainScreen.RefreshStationList();

            Log.Debug("Running GUI...");
            Application.Run();
            Log.Info("RFG exited.");
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Log.Error((Exception) e.ExceptionObject, "Unhandled exception has occurred!");
        }
    }
}