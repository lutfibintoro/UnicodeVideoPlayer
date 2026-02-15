using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Audio;

namespace App.Video
{
    internal interface ISetupVideo
    {
        internal int HeightVideo { get; set; }
        internal void InitVariable(string inputFullPathVideo, List<string> frame, string[] unicodePixel, ISetupAudio? setupAudion);
        internal void SetupFrameWithoutUnicode();
        internal void SetupFrameWithUnicode();
        internal void PlayVideo();
        internal void InitConsole();
    }
}
