﻿using System;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using NLog;

namespace RadioFreeZerg
{
    public class AudioPlayer : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly LibVLC LibVlc = new(false,
            ":quiet", ":no-keyboard-events", ":no-mouse-events", ":no-disable-screensaver", ":verbose=-1");

        private readonly object locker = new();
        private readonly MediaPlayer mediaPlayer = new(LibVlc);
        private string nowPlaying = "";
        private int volume = 100;

        public AudioPlayer() => mediaPlayer.AudioDevice += UpdateVolume;

        public string NowPlaying {
            get => nowPlaying;
            private set {
                if (nowPlaying != value) {
                    nowPlaying = value;
                    Log.Trace($"AudioPlayer.NowPlaying changed to: {nowPlaying}");
                    NowPlayingChanged(value);
                }
            }
        }

        public int Volume {
            get {
                lock (locker) {
                    // Don't use MediaPlayer.Volume as a backend, it will return -1 after current media is disposed.
                    return volume;
                }
            }

            set {
                lock (locker) {
                    volume = Math.Clamp(value, 0, 100);
                    mediaPlayer.Volume = volume;
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event Action<string> NowPlayingChanged = delegate { };

        public void Play(Uri source) {
            // I don't know when VLC actually ends its play-related buffering and stuff,
            // so lets keep this method sync with wait on media parsing.
            Log.Debug($"AudioPlayer starts playing '{source.AbsoluteUri}'...");
            lock (locker) {
                ClearMedia();
                mediaPlayer.Media = new Media(LibVlc, source, ":no-video");
                var parseTask = mediaPlayer.Media.Parse();
                mediaPlayer.Media.MetaChanged += MetaChanged;
                mediaPlayer.Play();
                parseTask.GetAwaiter().GetResult();
                Log.Debug($"Playing of '{source.AbsoluteUri}' has been started.");
            }
        }

        private void UpdateVolume(object? sender, MediaPlayerAudioDeviceEventArgs args) {
            // This way volume can be set even without media playing. As for the delay,
            // volume actually requires audio device, and real audio device becomes ready to set
            // slightly after this callback activates. Thanks VLC.
            Task.Delay(100).ContinueWith(_ => Volume = volume);
        }

        private void MetaChanged(object? sender, MediaMetaChangedEventArgs args) {
            if (args.MetadataType == MetadataType.NowPlaying)
                NowPlaying = mediaPlayer.Media?.Meta(MetadataType.NowPlaying) ?? "";
        }

        public void Stop() {
            lock (locker) {
                if (mediaPlayer.Media != null) Log.Debug($"AudioPlayer stops playing '{mediaPlayer.Media.Mrl}'.");

                mediaPlayer.Stop();
                mediaPlayer.Volume = volume;
                ClearMedia();
            }
        }

        private void ClearMedia() {
            var media = mediaPlayer.Media;
            mediaPlayer.Media = null;
            if (media != null) {
                media.MetaChanged -= MetaChanged;
                media.Dispose();
            }
        }

        private void ReleaseUnmanagedResources() {
            lock (locker) {
                ClearMedia();
                mediaPlayer.Dispose();
            }
        }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
        }

        ~AudioPlayer() {
            Dispose(false);
        }
    }
}