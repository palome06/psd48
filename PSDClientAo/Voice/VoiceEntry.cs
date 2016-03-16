using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace PSD.ClientAo.Voice
{
    public class VoiceEntry : IDisposable
    {
        private bool mStop;
        private ResourceDictionary rs;

        public VoiceEntry()
        {
            rs = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/PSDRisoLib;component/Resources/VocalRes.xaml",
                    UriKind.RelativeOrAbsolute)
            };
        }
        public VoiceEntry(ResourceDictionary rs) { this.rs = rs; }

        private void Play(Stream stream)
        {
            mStop = false;
            new Thread(() =>
            {
                using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(stream))
                using (var waveOut = new NAudio.Wave.WaveOutEvent())
                {
                    waveOut.Init(vorbisStream);
                    waveOut.Play();
                    SpinWait.SpinUntil(() => vorbisStream.Position >= vorbisStream.Length || mStop);
                    Thread.Sleep(200);
                    if (OnPlayFinished != null)
                        OnPlayFinished();
                }
            }).Start();
        }

        public void Play(string resourceKey)
        {
            if (rs.Contains(resourceKey))
            {
                string mp3Path = rs[resourceKey] as string;
                mp3Path = "pack://siteoforigin:,,,/Resources/" + mp3Path;
                Uri uri = new Uri(mp3Path, UriKind.RelativeOrAbsolute);
                Play(Application.GetRemoteStream(uri).Stream);
            }
        }

        public void Stop() { mStop = true; }

        public delegate bool FinishHandler();
        public FinishHandler OnPlayFinished;

        public void Dispose()
        {
            mStop = true;
        }
    }
}
