using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using App.Audio;
using App.LocalDb;
using App.MariiaanCmdPlay;
using App.SetUp;
using App.Video;

namespace App
{
    internal class Play
    {
        private ISetupAudio? _setupAudio;
        private ISetupVideo? _setupVideo;
        private readonly Setup _setup;
        private readonly string[] _videoList;

        internal Play(Setup setup, ISetupAudio setupAudio, ISetupVideo setupVideo)
        {
            setup.UnicodePxToSymbolPx();
            _setup = setup;

            _videoList = new DirectoryInfo(setup.VideoDir).GetFiles().Where(file => file.Extension == ".mp4").Select(info => info.FullName).ToArray();

            _setupAudio = setupAudio;
            _setupVideo = setupVideo;
        }

        internal void Start()
        {
            foreach (string videoFullName in _videoList)
            {
                SetupVideoMemoryFrame.LikeSubscribe(AppContext.BaseDirectory + @"video\LIKE_SUBS.png", _setup.UnicodePixel, videoFullName);

                _setupAudio?.InitVariable(videoFullName, PathReplacement.VideoFullPathToAudioFullPath(videoFullName));
                _setupVideo?.InitVariable(videoFullName, _setup.UnicodeFrame, _setup.UnicodePixel, _setupAudio);

                _setupAudio?.WriteAudio();
                _setupAudio?.ReadAudio();

                new Thread(() =>
                {
                    //if (_setupVideo?.HeightVideo > 80)
                    _setupVideo?.SetupFrameWithoutUnicode();
                    //else
                    //    _setupVideo?.SetupFrameWithUnicode();
                }).Start();
                Thread.Sleep(1000);

                _setupVideo?.InitConsole();
                Thread videoPlay = new(() => _setupVideo?.PlayVideo());
                Thread audioPlay = new(() => _setupAudio?.PlayAudio());

                videoPlay.Start();
                audioPlay.Start();

                videoPlay.Join();
                audioPlay.Join();

                _setup.UnicodeFrame.Clear();
            }

            SetupVideoMemoryFrame.LikeSubscribe(AppContext.BaseDirectory + @"video\LIKE_SUBS_smile.png", _setup.UnicodePixel, AppContext.BaseDirectory + @"video\zkuru_outro.mp4");
            //SetupVideoMemoryFrame.GradientCheck(AppContext.BaseDirectory + @"video\grayscale_gradient.png");
        }
    }
}
