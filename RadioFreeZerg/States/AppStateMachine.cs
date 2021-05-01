namespace RadioFreeZerg.States
{
    public class AppStateMachine : StateMachineBase<AppStateId, AppStateData, string?> {
        public AppStateMachine(AppStateData initialStateData) : base(initialStateData) {
        }
    }
}