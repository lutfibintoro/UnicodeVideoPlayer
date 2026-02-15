using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Xabe.FFmpeg;

namespace App.Audio
{
    internal class SetupAudio : ISetupAudio
    {
        private static readonly object _audioLock = new();
        private long _byteOffset = 0L;
        private string? InputFullPathVideo { get; set; }
        private string? OutputFullPathAudio { get; set; }
        public AudioFileReader? AudioFileReader { get; set; }
        public WaveOutEvent? WaveOutEvent { get; set; }

        public SetupAudio() { }

        public SetupAudio(string inputFullPathVideo, string outputFullPathAudio)
        {
            InputFullPathVideo = inputFullPathVideo;
            OutputFullPathAudio = outputFullPathAudio;
        }

        public void InitVariable(string inputFullPathVideo, string outputFullPathAudio)
        {
            InputFullPathVideo = inputFullPathVideo;
            OutputFullPathAudio = outputFullPathAudio;
            _byteOffset = 0L;

        }

        public void WriteAudio()
        {
            Console.WriteLine("audio preparation. . .");

            IConversion conversion =
                Xabe.FFmpeg.FFmpeg.Conversions.FromSnippet
                .ExtractAudio(InputFullPathVideo, OutputFullPathAudio)
                .GetAwaiter().GetResult();

            conversion.Start().GetAwaiter().GetResult();
        }

        public void ReadAudio()
        {
            Console.WriteLine("audio reading process. . .");

            AudioFileReader = new(OutputFullPathAudio);
            AudioFileReader.Volume = 1.0f;
            WaveOutEvent = new();
            WaveOutEvent.Init(AudioFileReader);
        }

        public void PlayAudio()
        {
            using (AudioFileReader!)
            using (WaveOutEvent!)
            {
                WaveOutEvent?.Play();
                while (WaveOutEvent?.PlaybackState != PlaybackState.Stopped)
                    Thread.Sleep(4000);
            }

            AudioFileReader = null;
            WaveOutEvent = null;
        }

        public void Resume()
        {
            lock (_audioLock)
            {
                if (WaveOutEvent is null)
                    return;

                if (WaveOutEvent.PlaybackState == PlaybackState.Paused)
                    WaveOutEvent.Play();
            }
        }

        public void Pause()
        {
            lock (_audioLock)
            {
                if (WaveOutEvent is null)
                    return;

                if (WaveOutEvent.PlaybackState == PlaybackState.Playing)
                    WaveOutEvent.Pause();
            }
        }

        public void Forward10()
        {
            if (WaveOutEvent is null || AudioFileReader is null)
                return;

            lock (_audioLock)
            {
                TimeSpan time = AudioFileReader.CurrentTime + TimeSpan.FromSeconds(10);
                if (time >= AudioFileReader.TotalTime)
                    AudioFileReader.CurrentTime = AudioFileReader.TotalTime;
                else
                    AudioFileReader.CurrentTime = time;

                long bytePos = AudioFileReader.Position;
                long deviceBytes = WaveOutEvent.GetPosition();

                _byteOffset = bytePos - deviceBytes;
            }
        }

        public void BackWard10()
        {
            if (WaveOutEvent is null || AudioFileReader is null)
                return;

            lock (_audioLock)
            {
                TimeSpan time = AudioFileReader.CurrentTime - TimeSpan.FromSeconds(10);
                if (time <= TimeSpan.FromSeconds(0))
                    AudioFileReader.CurrentTime = TimeSpan.FromSeconds(0);
                else
                    AudioFileReader.CurrentTime = time;

                long bytePos = AudioFileReader.Position;
                long deviceBytes = WaveOutEvent.GetPosition();

                _byteOffset = bytePos - deviceBytes;
            }
        }

        public long GetBytePosition()
        {
            if (WaveOutEvent is null || AudioFileReader is null)
                throw new InvalidOperationException("GetBytePosition");

            long deviceBytes = WaveOutEvent.GetPosition();
            return _byteOffset + deviceBytes;
        }

        public void Reset()
        {

        }
    }
}
