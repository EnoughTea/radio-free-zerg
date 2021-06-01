﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RadioFreeZerg
{
    public class RadioStationManager : IEnumerable<RadioStation>
    {
        private readonly Dictionary<int, RadioStation> stations;
        private readonly RadioStationFinder finder;
        private readonly RadioStationPlayer player;

        public RadioStationManager(ICuteRadioStationProvider radioStationsProvider) {
            stations = radioStationsProvider.Select(_ => _.ToRadioStation())
                                            .ToDictionary(_ => _.Id);
            finder = new RadioStationFinder();
            player = new RadioStationPlayer(this);
        }

        public string NowPlaying => player.NowPlaying;

        public event Action<string> NowPlayingChanged {
            add => player.NowPlayingChanged += value;
            remove => player.NowPlayingChanged -= value;
        }
        
        public RadioStation CurrentStation => player.CurrentStation;
        
        public IReadOnlyCollection<RadioStation> All => stations.Values;

        public IEnumerator<RadioStation> GetEnumerator() =>
            ((IEnumerable<RadioStation>) stations.Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RadioStation Find(int stationId) {
            if (!stations.TryGetValue(stationId, out var foundStation)) foundStation = RadioStation.Empty;

            return foundStation;
        }

        public IEnumerable<RadioStation> Find(string userInput) => finder.Find(userInput, All);

        public void Play(RadioStation station) {
            player.Play(station);
        }
        
        public void Stop() {
            player.Stop();
        }
    }
}