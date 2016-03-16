using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace PSD.ClientAo.Voice
{
    public class AoVoice : IDisposable
    {
        private ResourceDictionary rs;
        // queue of voices to be played
        private BlockingCollection<string> voiceQueue;
        // dictionary from a voice collection to the seq number
        private IDictionary<string, int> voiceSeqDict;
        // current voice entry
        private VoiceEntry currentVoiceEntry;

        private Random randSeed;
        private Thread runningThread;

        public bool IsMute { private set; get; }

        public AoVoice()
        {
            rs = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/PSDRisoLib;component/Resources/VocalRes.xaml",
                UriKind.RelativeOrAbsolute)
            };
            currentVoiceEntry = null;
            voiceQueue = new BlockingCollection<string>();
            voiceSeqDict = new Dictionary<string, int>();
            randSeed = new Random();
            IsMute = false;
        }

        public void Speak(string name, int type)
        {
            string entry = name + "_" + type;
            // JNT3501_0_1.sound;
            int soundTrack = voiceSeqDict.ContainsKey(entry) ?
                (1 - voiceSeqDict[entry]) : randSeed.Next(0, 2);
            voiceSeqDict[entry] = soundTrack;
            voiceQueue.Add(entry + "_" + soundTrack);
        }

        public void Init()
        {
            runningThread = new Thread(() =>
            {
                while (true)
                {
                    string voice = voiceQueue.Take();
                    if (!IsMute)
                    {
                        currentVoiceEntry = new VoiceEntry(rs);
                        currentVoiceEntry.Play("voice" + voice);
                        currentVoiceEntry = null;
                    }
                }
            });
            runningThread.Start();
        }

        public void Mute()
        {
            IsMute = true;
            if (currentVoiceEntry != null)
                currentVoiceEntry.Stop();
        }

        public void Resume()
        {
            IsMute = false;
        }

        public void Dispose()
        {
            if (runningThread != null && runningThread.IsAlive)
                runningThread.Abort();
            if (voiceQueue != null)
                voiceQueue.Dispose();
        }
    }
}
