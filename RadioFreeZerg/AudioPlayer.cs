using System;
using System.Linq;
using System.Threading;
using LibVLCSharp.Shared;

namespace RadioFreeZerg
{
    public class AudioPlayer : IDisposable
    {
        private static readonly LibVLC LibVlc = new(false,
            ":quiet", ":no-keyboard-events", ":no-mouse-events", ":no-disable-screensaver", ":verbose=-1");

        private readonly object locker = new();
        private readonly MediaPlayer mediaPlayer = new(LibVlc);
        private string nowPlaying = "";

        public string NowPlaying {
            get => nowPlaying;
            private set {
                if (nowPlaying != value) {
                    nowPlaying = value;
                    NowPlayingChanged(value);
                }
            }
        }

        public event Action<string> NowPlayingChanged = delegate {};
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Play(Uri source) {
            lock (locker) {
                ClearMedia();
                mediaPlayer.Media = new Media(LibVlc, source, ":no-video");
                mediaPlayer.Media.MetaChanged += MetaChanged;
                mediaPlayer.Play();
            }
        }

        private void MetaChanged(object? sender, MediaMetaChangedEventArgs args) {
            if (args.MetadataType == MetadataType.NowPlaying) {
                NowPlaying = mediaPlayer.Media?.Meta(MetadataType.NowPlaying) ?? "";
            }
        }

        public void SetVolume(int percent) {
            lock (locker) {
                var clampedVolume = Math.Clamp(percent, 0, 100);
                mediaPlayer.Volume = clampedVolume;
            }
        }

        public void Stop() {
            lock (locker) {
                mediaPlayer.Stop();
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