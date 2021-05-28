using System;
using LibVLCSharp.Shared;

namespace RadioFreeZerg
{
    public class AudioPlayer : IDisposable
    {
        private static readonly LibVLC LibVlc = new(false,
            ":quiet", ":no-keyboard-events", ":no-mouse-events", ":no-disable-screensaver", ":verbose=-1");

        private readonly object locker = new();
        private readonly MediaPlayer mediaPlayer = new(LibVlc);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Play(Uri source) {
            lock (locker) {
                mediaPlayer.Media?.Dispose();
                mediaPlayer.Media = new Media(LibVlc, source, ":no-video");
                mediaPlayer.Play();
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
                mediaPlayer.Media?.Dispose();
                mediaPlayer.Media = null;
            }
        }

        private void ReleaseUnmanagedResources() {
            mediaPlayer.Media?.Dispose();
            mediaPlayer.Dispose();
        }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
        }

        ~AudioPlayer() {
            Dispose(false);
        }
    }
}