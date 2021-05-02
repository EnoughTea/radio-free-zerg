using NLog;

namespace RadioFreeZerg.States
{
    public abstract class AppState : State<AppStateId, AppStateData, string?>
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected AppState(AppStateId stateId) : base(stateId) { }
    }
}