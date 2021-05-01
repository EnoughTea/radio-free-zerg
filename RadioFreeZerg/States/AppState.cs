using NLog;

namespace RadioFreeZerg.States
{
    public abstract class AppState : State<AppStateId, AppStateData, string?>
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        protected AppState(AppStateId stateId) : base(stateId) {
        }
    }
}