using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RadioFreeZerg
{
    public class RadioStationManager : IEnumerable<RadioStation>
    {
        private readonly RadioStationFinder finder;
        private readonly RadioStationPlayer player;
        private readonly Dictionary<int, RadioStation> stations;
        private RadioStation toggledStation;

        public RadioStationManager(ICuteRadioStationProvider radioStationsProvider) {
            stations = radioStationsProvider.Select(_ => _.ToRadioStation())
                                            .ToDictionary(_ => _.Id);
            toggledStation = RadioStation.Empty;
            finder = new RadioStationFinder();
            player = new RadioStationPlayer(this);
        }

        public string NowPlaying => player.NowPlaying;

        public RadioStation CurrentStation => player.CurrentStation;

        public IReadOnlyCollection<RadioStation> All => stations.Values;

        public IEnumerator<RadioStation> GetEnumerator() =>
            ((IEnumerable<RadioStation>) stations.Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public event Action<string> NowPlayingChanged {
            add => player.NowPlayingChanged += value;
            remove => player.NowPlayingChanged -= value;
        }

        public RadioStation Find(int stationId) {
            if (!stations.TryGetValue(stationId, out var foundStation)) foundStation = RadioStation.Empty;

            return foundStation;
        }

        public IEnumerable<RadioStation> Find(string userInput) => finder.Find(userInput, All);

        public void Play(RadioStation stationToPlay) {
            player.Play(stationToPlay);
        }

        public void Stop() {
            player.Stop();
        }

        public void TogglePlay(RadioStation stationToPlay) {
            if (CurrentStation != RadioStation.Empty) {
                toggledStation = CurrentStation;
                if (stationToPlay != CurrentStation) Play(stationToPlay);
                else Stop();
            } else {
                Play(stationToPlay != RadioStation.Empty ? stationToPlay : toggledStation);
            }
        }
    }
}