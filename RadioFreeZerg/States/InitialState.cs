using System;
using System.Threading.Tasks;
using NLog;

namespace RadioFreeZerg.States
{
    public class InitialState : AppState
    {
        public InitialState() : base(AppStateId.Initial) {
        }

        public override void StateEnter(AppStateId previousStateId, AppStateData data) {
            Console.WriteLine("Type 's' to search for radio stations");
        }

        public override AppStateId HandleEvent(string? stateEvent, AppStateData data) {
            return stateEvent switch {
                "s" => AppStateId.StationSearch,
                _ => AppStateId.Initial
            };
        }
    }
}