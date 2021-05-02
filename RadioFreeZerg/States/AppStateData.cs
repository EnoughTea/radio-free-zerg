namespace RadioFreeZerg.States
{
    /// <summary> Combined with the <see cref="AppStateMachine"/>, contains all of the app state. </summary>
    public class AppStateData
    {
        public AppStateSearchData Search { get; } = new();

        public RadioStation? CurrentRadioStation { get; set; }
    }
}