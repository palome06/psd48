using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace PSD.ClientAo.Voice
{
    //[DebuggerDisplay("Count = {Count}")]
    public class AoVoice : IDisposable
    {
        private ResourceDictionary rs;
        // queue of voices to be played
        private BlockingCollection<string> voiceQueue;
        // dictionary from a voice collection to the seq number
        private IDictionary<string, int> voiceSeqDict;
        // current voice entries        
        private List<VoiceEntry> currentActiveVoiceEntry;

        private Random randSeed;
        private Thread runningThread;

        public bool IsMute { private set; get; }

        public AoVoice(bool isMute)
        {
            rs = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/PSDRisoLib;component/Resources/VocalRes.xaml",
                UriKind.RelativeOrAbsolute)
            };
            voiceQueue = new BlockingCollection<string>();
            voiceSeqDict = new Dictionary<string, int>();
            randSeed = new Random();
            IsMute = isMute;
            currentActiveVoiceEntry = new List<VoiceEntry>();
        }

        public void Speak(string name)
        {
            voiceQueue.Add(name);
        }
        public void Speak(string name, int type)
        {
            string entry = name + "_" + type;
            // JNT3501_0_1.sound;
            int soundTrack = voiceSeqDict.ContainsKey(entry) ?
                (1 - voiceSeqDict[entry]) : randSeed.Next(2);
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
                        VoiceEntry currentVoiceEntry = new VoiceEntry(rs);
                        lock (currentActiveVoiceEntry)
                        {
                            currentActiveVoiceEntry.Add(currentVoiceEntry);
                        }
                        currentVoiceEntry.Play("voice" + voice);
                        currentVoiceEntry.OnPlayFinished +=
                            () => currentActiveVoiceEntry.Remove(currentVoiceEntry);
                    }
                }
            });
            runningThread.Start();
        }

        public void Mute()
        {
            IsMute = true;
            lock (currentActiveVoiceEntry)
            {
                currentActiveVoiceEntry.ForEach(p => p.Stop());
            }
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
        // Source: http://moriya.ca/oggextract/
    }
}
