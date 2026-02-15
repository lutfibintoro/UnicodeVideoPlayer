using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using App.Audio;
using Emgu.CV.Reg;
using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using FFmpeg.AutoGen;
using NAudio.Wave;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace App.Video
{
    internal class SetupVideoMemoryFrame : ISetupVideo
    {
        private static readonly object _locker = new();
        private bool _buildFrameIsStoped = false;
        private string? InputFullPathVideo { get; set; }
        public int HeightVideo { get; set; }
        private int _totalFrame { get; set; }
        private List<string>? _frame;
        private string[]? _unicodePixel;
        private ISetupAudio? _setupAudion;
        private double _fps;

        internal SetupVideoMemoryFrame() { }
        internal SetupVideoMemoryFrame(string inputFullPathVideo, List<string> frame, string[] unicodePixel, ISetupAudio? setupAudion)
        {
            InputFullPathVideo = inputFullPathVideo;
            _frame = frame;
            _unicodePixel = unicodePixel;
            _setupAudion = setupAudion;
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                HeightVideo = file.Video.Info.FrameSize.Height;
                _totalFrame = file.Video.Info.NumberOfFrames ?? 0;
            }
        }


        public void InitVariable(string inputFullPathVideo, List<string> frame, string[] unicodePixel, ISetupAudio? setupAudion)
        {
            InputFullPathVideo = inputFullPathVideo;
            _frame = frame;
            _unicodePixel = unicodePixel;
            _setupAudion = setupAudion;
            _buildFrameIsStoped = false;
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                HeightVideo = file.Video.Info.FrameSize.Height;
                _totalFrame = file.Video.Info.NumberOfFrames ?? 0;
            }
        }


        /// <summary>
        /// setup frame
        /// </summary>
        public void SetupFrameWithoutUnicode()
        {
            Console.WriteLine("frame preparation. . .");
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                _fps = file.Video.Info.RealFrameRate.num / file.Video.Info.RealFrameRate.den;
                int width = file.Video.Info.FrameSize.Width;
                int height = file.Video.Info.FrameSize.Height;

                if (height > 186)
                    throw new InvalidOperationException("maximal tinggi pixel di video tidak boleh lebih dari 186 pixel");

                //int i = 0;
                //double frameDurationMs = 1000.0 / _fps;
                //Stopwatch stopwatch = Stopwatch.StartNew();
                while (file.Video.TryGetNextFrame(out ImageData bitmapFrame))
                {
                    StringBuilder sbFrame = ImageDataToPixelNoUnicode(bitmapFrame, height, width);
                    _frame?.Add(sbFrame.ToString());

                    //double targetTime = i * frameDurationMs;
                    //while (stopwatch.Elapsed.TotalMilliseconds < targetTime) { }
                    //i++;

                    if (_buildFrameIsStoped)
                        break;
                }
            }
        }


        /// <summary>
        /// setuo frame
        /// </summary>
        public void SetupFrameWithUnicode()
        {
            Console.WriteLine("frame preparation. . .");
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                _fps = file.Video.Info.RealFrameRate.num / file.Video.Info.RealFrameRate.den;
                int width = file.Video.Info.FrameSize.Width;
                int height = file.Video.Info.FrameSize.Height;

                if (height > 80)
                    throw new InvalidOperationException("maximal tinggi pixel di video tidak boleh lebih dari 80 pixel");

                int i = 0;
                double frameDurationMs = 1000.0 / _fps;
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (file.Video.TryGetNextFrame(out ImageData bitmapFrame))
                {
                    StringBuilder sbFrame = ImageDataToPixelYesUnicode(bitmapFrame, height, width);
                    _frame?.Add(sbFrame.ToString());

                    double targetTime = i * frameDurationMs;
                    while (stopwatch.Elapsed.TotalMilliseconds < targetTime) { }
                    i++;

                    if (_buildFrameIsStoped)
                        break;
                }
            }
        }


        /// <summary>
        /// setup frame
        /// </summary>
        [Obsolete("gagal")]
        internal void SetupFrameWithSKBitmap()
        {
            Console.WriteLine("frame preparation. . .");
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Rgba32 }))
            using (SKBitmap skBitmap = new(file.Video.Info.FrameSize.Width, file.Video.Info.FrameSize.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul))
            {
                _fps = file.Video.Info.RealFrameRate.num / file.Video.Info.RealFrameRate.den;
                int width = file.Video.Info.FrameSize.Width;
                int height = file.Video.Info.FrameSize.Height;
                Span<byte> pixelBuffer = skBitmap.GetPixelSpan();

                if (height > 186)
                    throw new InvalidOperationException("maximal tinggi pixel di video tidak boleh lebih dari 186 pixel");

                while (file.Video.TryGetNextFrame(pixelBuffer))
                {
                    StringBuilder sbFrame = new();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            SKColor skColor = skBitmap.GetPixel(x, y);
                            double grayscale = 0.299d * skColor.Red + 0.587d * skColor.Green + 0.114d * skColor.Blue;

                            // .,:;-~=≡3B[@▒▓█
                            if (grayscale <= 15)
                                sbFrame.Append("  ");

                            else if (grayscale <= 31)
                                sbFrame.Append(". ");

                            else if (grayscale <= 47)
                                sbFrame.Append(", ");

                            else if (grayscale <= 63)
                                sbFrame.Append(": ");

                            else if (grayscale <= 79)
                                sbFrame.Append("; ");

                            else if (grayscale <= 95)
                                sbFrame.Append("- ");

                            else if (grayscale <= 111)
                                sbFrame.Append("~ ");

                            else if (grayscale <= 127)
                                sbFrame.Append("= ");

                            else if (grayscale <= 143)
                                sbFrame.Append("≡ ");

                            else if (grayscale <= 159)
                                sbFrame.Append("3 ");

                            else if (grayscale <= 175)
                                sbFrame.Append("B ");

                            else if (grayscale <= 191)
                                sbFrame.Append("[ ");

                            else if (grayscale <= 207)
                                sbFrame.Append("@ ");

                            else if (grayscale <= 223)
                                sbFrame.Append("▒ ");

                            else if (grayscale <= 239)
                                sbFrame.Append("▓ ");

                            else
                                sbFrame.Append("█ ");
                        }
                        sbFrame.Append('\n');
                    }

                    _frame?.Add(sbFrame.ToString());
                }

            }

            Console.SetCursorPosition(0, 0);
            foreach (char item in _frame?[0] ?? "")
                Console.Write(item);
        }


        public void PlayVideo()
        {
            //double frameDurationMs = 1000.0 / _fps;
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //for (int i = 0; i < _frame.Count; i++)
            //{
            //    double targetTime = i * frameDurationMs;
            //    while (stopwatch.Elapsed.TotalMilliseconds < targetTime) { }

            //    Console.SetCursorPosition(0, 0);
            //    Console.WriteLine(_frame[i]);
            //}


            //int i = 1;ArgumentOutOfRangeException
            int totalFrame = _totalFrame;
            while (true)
            {
                if (_setupAudion?.WaveOutEvent is null || _setupAudion.AudioFileReader is null)
                    return;

                float Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                int frame = (int)(Fpercentage * totalFrame);
                if (frame >= totalFrame)
                    break;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine(_frame?[frame]);

                //Task.Run(() =>
                //{
                //    int tFrame = frame;
                //    if (i < tFrame)
                //    {
                //        lock (_locker)
                //        {
                //            _frame[i] = string.Empty;
                //            i = tFrame;
                //        }
                //    }
                //});

                if (Console.KeyAvailable)
                {
                    ConsoleKey ky = Console.ReadKey().Key;
                    if (ky == ConsoleKey.Spacebar)
                    {
                        _setupAudion.Pause();

                        while (_setupAudion.WaveOutEvent.PlaybackState == PlaybackState.Paused)
                        {
                            ConsoleKey resumeKey = Console.ReadKey().Key;
                            if (resumeKey == ConsoleKey.Spacebar)
                            {
                                _setupAudion.Resume();
                                break;
                            }
                            else if (resumeKey == ConsoleKey.RightArrow)
                            {
                                _setupAudion.Forward10();

                                Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                                frame = (int)(Fpercentage * totalFrame);

                                Console.SetCursorPosition(0, 0);
                                Console.WriteLine(_frame?[frame]);
                            }
                            else if (resumeKey == ConsoleKey.LeftArrow)
                            {
                                _setupAudion.BackWard10();

                                Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                                frame = (int)(Fpercentage * totalFrame);

                                Console.SetCursorPosition(0, 0);
                                Console.WriteLine(_frame?[frame]);
                            }
                        }
                    }
                    else if (ky == ConsoleKey.Escape)
                    {
                        _setupAudion.WaveOutEvent.Stop();
                        break;
                    }
                    else if (ky == ConsoleKey.RightArrow)
                    {
                        _setupAudion.Forward10();

                        Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                        frame = (int)(Fpercentage * totalFrame);
                        if (!(frame < _frame?.Count))
                            _setupAudion.BackWard10();
                    }
                    else if (ky == ConsoleKey.LeftArrow)
                        _setupAudion.BackWard10();
                }
            }

            _buildFrameIsStoped = true;
            ClearConsole(_frame?[0] ?? "".ToString());
        }


        private void ClearConsole(string frame)
        {
            Console.SetCursorPosition(0, 0);
            foreach (char item in frame)
            {
                if (item == '\n')
                    Console.Write('\n');
                else
                    Console.Write(' ');
            }
        }


        public void InitConsole()
        {
            Console.SetCursorPosition(0, 0);
            foreach (char item in _frame?[0] ?? "")
                Console.Write(item);
        }


        private StringBuilder ImageDataToPixelYesUnicode(ImageData imageData, int height, int width)
        {
            StringBuilder sbFrame = new();

            int line = 0;
            for (int i = 0; i < width * height; i++)
            {
                int row = i / width;
                int col = i % width;
                int index = row * imageData.Stride + col * 1;

                int px = imageData.Data[index];

                // .,-*:;~>=?|[#&$
                if (px <= 15)
                    px = 0;
                else if (px <= 31)
                    px = 16;
                else if (px <= 47)
                    px = 32;
                else if (px <= 63)
                    px = 48;
                else if (px <= 79)
                    px = 63;
                else if (px <= 95)
                    px = 80;
                else if (px <= 111)
                    px = 96;
                else if (px <= 127)
                    px = 112;
                else if (px <= 143)
                    px = 128;
                else if (px <= 159)
                    px = 144;
                else if (px <= 175)
                    px = 160;
                else if (px <= 191)
                    px = 176;
                else if (px <= 207)
                    px = 192;
                else if (px <= 223)
                    px = 208;
                else if (px <= 239)
                    px = 224;
                else
                    px = 240;


                if (!(line == row))
                {
                    line = row;
                    sbFrame.Append('\n');
                    sbFrame.Append(_unicodePixel?[px]);
                }
                else
                    sbFrame.Append(_unicodePixel?[px]);
            }

            return sbFrame;
        }


        private StringBuilder ImageDataToPixelNoUnicode(ImageData imageData, int height, int width)
        {
            StringBuilder sbFrame = new();

            int line = 0;
            for (int i = 0; i < width * height; i++)
            {
                int row = i / width;
                int col = i % width;
                int index = row * imageData.Stride + col * 1;

                double px = imageData.Data[index];


                if (!(line == row))
                {
                    line = row;
                    sbFrame.Append('\n');
                }

                //11
                if (px <= 23.272727272727273d)
                    sbFrame.Append("  ");
                else if (px <= 46.54545454545455d)
                    sbFrame.Append(". ");
                else if (px <= 69.81818181818181d)
                    sbFrame.Append(",,");
                else if (px <= 93.0909090909091d)
                    sbFrame.Append(";;");
                else if (px <= 116.36363636363637d)
                    sbFrame.Append("--");
                else if (px <= 139.63636363636365d)
                    sbFrame.Append("~~");
                else if (px <= 162.90909090909093d)
                    sbFrame.Append("++");
                else if (px <= 186.18181818181822d)
                    sbFrame.Append("((");
                else if (px <= 209.4545454545455d)
                    sbFrame.Append("JJ");
                else if (px <= 232.72727272727278d)
                    sbFrame.Append("HH");
                else
                    sbFrame.Append("&&");
            }

            return sbFrame;
        }


        internal static void LikeSubscribe(string fullPathFrame, string[] unicodePixel, string isOutroVideo)
        {
            if (!(isOutroVideo.Split('\\')[^1] == "zkuru_outro.mp4"))
                return;

            if (!File.Exists(fullPathFrame))
                return;

            StringBuilder sbFrame = new();
            using (Bitmap bmp = new(fullPathFrame))
            {

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        System.Drawing.Color pixel = bmp.GetPixel(x, y);
                        byte px = (byte)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                        if (px <= 15)
                            px = 0;
                        else if (px <= 31)
                            px = 16;
                        else if (px <= 47)
                            px = 32;
                        else if (px <= 63)
                            px = 48;
                        else if (px <= 79)
                            px = 63;
                        else if (px <= 95)
                            px = 80;
                        else if (px <= 111)
                            px = 96;
                        else if (px <= 127)
                            px = 112;
                        else if (px <= 143)
                            px = 128;
                        else if (px <= 159)
                            px = 144;
                        else if (px <= 175)
                            px = 160;
                        else if (px <= 191)
                            px = 176;
                        else if (px <= 207)
                            px = 192;
                        else if (px <= 223)
                            px = 208;
                        else if (px <= 239)
                            px = 224;
                        else
                            px = 240;

                        sbFrame.Append(unicodePixel[px]);
                    }

                    sbFrame.Append('\n');
                }
            }

            Console.SetCursorPosition(0, 0);
            foreach (char item in sbFrame.ToString())
                Console.Write(item);
        }


        internal static void GradientCheck(string fullPathFrame)
        {

            if (!File.Exists(fullPathFrame))
                return;

            StringBuilder sbFrame = new();
            using (Bitmap bmp = new(fullPathFrame))
            {

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        System.Drawing.Color pixel = bmp.GetPixel(x, y);
                        double r = pixel.R;
                        double g = pixel.G;
                        double b = pixel.B;
                        double px = (r * 0.299d) + (g * 0.587d) + (b * 0.114);

                        if (px <= 23.272727272727273d)
                            sbFrame.Append("  ");
                        else if (px <= 46.54545454545455d)
                            sbFrame.Append(". ");
                        else if (px <= 69.81818181818181d)
                            sbFrame.Append(",,");
                        else if (px <= 93.0909090909091d)
                            sbFrame.Append(";;");
                        else if (px <= 116.36363636363637d)
                            sbFrame.Append("--");
                        else if (px <= 139.63636363636365d)
                            sbFrame.Append("~~");
                        else if (px <= 162.90909090909093d)
                            sbFrame.Append("++");
                        else if (px <= 186.18181818181822d)
                            sbFrame.Append("((");
                        else if (px <= 209.4545454545455d)
                            sbFrame.Append("JJ");
                        else if (px <= 232.72727272727278d)
                            sbFrame.Append("HH");
                        else
                            sbFrame.Append("&&");
                    }

                    sbFrame.Append('\n');
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(sbFrame.ToString());
            //foreach (char item in sbFrame.ToString())
            //    Console.Write(item);
        }
    }
}
