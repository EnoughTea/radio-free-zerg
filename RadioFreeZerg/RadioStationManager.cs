using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RadioFreeZerg
{
    public class RadioStationManager : IEnumerable<RadioStation>
    {
        private readonly AudioPlayer player;
        private readonly Dictionary<int, RadioStation> stations;

        public RadioStationManager(ICuteRadioStationProvider radioStationsProvider,
                                   AudioPlayer audioPlayer) {
            player = audioPlayer;
            stations = radioStationsProvider.Select(_ => _.ToRadioStation())
                                            .ToDictionary(_ => _.Id);
        }

        public IReadOnlyCollection<RadioStation> AllStations => stations.Values;
        
        public RadioStation CurrentStation { get; private set; } = RadioStation.Empty;

        public IEnumerator<RadioStation> GetEnumerator() =>
            ((IEnumerable<RadioStation>) stations.Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Play(int stationId) {
            if (!stations.TryGetValue(stationId, out var station))
                throw new ArgumentException("Invalid station ID", nameof(stationId));

            Play(station);
        }

        public void Play(RadioStation station) {
            if (station == RadioStation.Empty)
                Stop();
            else
                lock (player) {
                    if (station != CurrentStation) {
                        CurrentStation = station;
                        player.Play(station.Source);
                    }
                }
        }

        public void Stop() {
            lock (player) {
                player.Stop();
                CurrentStation = RadioStation.Empty;
            }
        }
    }
}