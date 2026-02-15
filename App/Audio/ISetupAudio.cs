using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace App.Audio
{
    internal interface ISetupAudio
    {
        internal AudioFileReader? AudioFileReader { get; set; }
        internal WaveOutEvent? WaveOutEvent { get; set; }
        internal void InitVariable(string inputFullPathVideo, string outputFullPathAudio);
        internal void WriteAudio();
        internal void ReadAudio();
        internal void PlayAudio();
        internal void Resume();
        internal void Pause();
        internal void Forward10();
        internal void BackWard10();
        internal void Reset();
        public long GetBytePosition();
    }
}
