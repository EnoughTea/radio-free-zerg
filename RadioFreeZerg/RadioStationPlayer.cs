using System;

namespace RadioFreeZerg
{
    public class RadioStationPlayer
    {
        private readonly object locker = new();
        private readonly RadioStationManager manager;
        private readonly AudioPlayer player;

        public RadioStationPlayer(RadioStationManager radioStationManagermanager) {
            manager = radioStationManagermanager;
            player = new AudioPlayer();
        }

        public RadioStation CurrentStation { get; private set; } = RadioStation.Empty;

        public string NowPlaying => player.NowPlaying;

        public int Volume {
            get => player.Volume;
            set => player.Volume = value;
        }

        public event Action<string> NowPlayingChanged {
            add {
                lock (locker) {
                    player.NowPlayingChanged += value;
                }
            }
            remove {
                lock (locker) {
                    player.NowPlayingChanged -= value;
                }
            }
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