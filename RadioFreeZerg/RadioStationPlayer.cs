using System;

namespace RadioFreeZerg
{
    public class RadioStationPlayer
    {
        private readonly RadioStationManager manager;
        private readonly AudioPlayer player;
        private readonly object locker = new();
        
        public RadioStation CurrentStation { get; private set; } = RadioStation.Empty;

        public RadioStationPlayer(RadioStationManager radioStationManagermanager) {
            manager = radioStationManagermanager;
            player = new AudioPlayer();
        }
        
        
        public void Play(int stationId) {
            var foundStation = manager.Find(stationId);
            if (foundStation == RadioStation.Empty)
                throw new ArgumentException("Invalid station ID", nameof(stationId));

            Play(foundStation);
        }

        public void Play(RadioStation station) {
            if (station == RadioStation.Empty)
                Stop();
            else
                lock (locker) {
                    if (station != CurrentStation) {
                        CurrentStation = station;
                        player.Play(station.Source);
                    }
                }
        }

        public void Stop() {
            lock (locker) {
                player.Stop();
                CurrentStation = RadioStation.Empty;
            }
        }
    }
}