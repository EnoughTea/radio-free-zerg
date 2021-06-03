using System;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public abstract class GuiComponent
    {
        protected GuiComponent(RadioStationManager radioStationManager,
                               UserState userState,
                               Window window) {
            RadioStations = radioStationManager;
            State = userState;
            Window = window;
        }

        protected static Random Rng { get; } = new();

        protected UserState State { get; }

        protected RadioStationManager RadioStations { get; }

        protected Window Window { get; }
    }
}