using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Audio;
using App.LocalDb;
using Emgu.CV.Ocl;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using FFmpeg.AutoGen;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;

namespace App.Video
{
    internal class SetupVideoDbFrame : ISetupVideo
    {
        private static readonly object _videoLock = new();
        private bool _buildFrameIsStoped = false;
        private string? InputFullPathVideo { get; set; }
        public int HeightVideo { get; set; }
        private int _totalFrame { get; set; }
        private string[]? _unicodePixel;
        private ISetupAudio? _setupAudion;
        private string? _nameOfVideo { get; set; }
        private double _fps;

        internal SetupVideoDbFrame() { }
        internal SetupVideoDbFrame(string inputFullPathVideo, List<string> _, string[] unicodePixel, ISetupAudio? setupAudion)
        {
            InputFullPathVideo = inputFullPathVideo;
            _unicodePixel = unicodePixel;
            _setupAudion = setupAudion;
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                HeightVideo = file.Video.Info.FrameSize.Height;
                _totalFrame = file.Video.Info.NumberOfFrames ?? 0;
            }
        }

        public void InitVariable(string inputFullPathVideo, List<string> _, string[] unicodePixel, ISetupAudio? setupAudion)
        {
            InputFullPathVideo = inputFullPathVideo;
            _unicodePixel = unicodePixel;
            _setupAudion = setupAudion;
            _buildFrameIsStoped = false;
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            {
                HeightVideo = file.Video.Info.FrameSize.Height;
                _totalFrame = file.Video.Info.NumberOfFrames ?? 0;
            }
        }

        public void SetupFrameWithoutUnicode()
        {
            Console.WriteLine("frame preparation. . .");
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            using (VideoDbContext context = new())
            {
                int width = file.Video.Info.FrameSize.Width;
                int height = file.Video.Info.FrameSize.Height;
                _fps = file.Video.Info.RealFrameRate.num / file.Video.Info.RealFrameRate.den;
                _nameOfVideo = InputFullPathVideo!.Split('\\')[^1].Split('.')[0];



                if (height > 186)
                    throw new InvalidOperationException("maximal tinggi pixel di video tidak boleh lebih dari 186 pixel");

                int i = 0;
                while (file.Video.TryGetNextFrame(out ImageData bitmapFrame))
                {
                    i++;
                    context.Frames.Add(new FrameModel()
                    {
                        Name = _nameOfVideo,
                        Id = i,
                        Frame = ImageDataToPixelNoUnicode(bitmapFrame, height, width)
                    });
                    context.SaveChanges();

                    if (_buildFrameIsStoped)
                        break;
                }
            }
        }

        public void SetupFrameWithUnicode()
        {
            Console.WriteLine("frame preparation. . .");
            using (MediaFile file = MediaFile.Open(InputFullPathVideo, new MediaOptions { VideoPixelFormat = ImagePixelFormat.Gray8, StreamsToLoad = MediaMode.Video }))
            using (VideoDbContext context = new())
            {
                int width = file.Video.Info.FrameSize.Width;
                int height = file.Video.Info.FrameSize.Height;
                _fps = file.Video.Info.RealFrameRate.num / file.Video.Info.RealFrameRate.den;
                _nameOfVideo = InputFullPathVideo!.Split('\\')[^1].Split('.')[0];


                if (height > 186)
                    throw new InvalidOperationException("maximal tinggi pixel di video tidak boleh lebih dari 186 pixel");

                int i = 0;
                while (file.Video.TryGetNextFrame(out ImageData bitmapFrame))
                {
                    i++;
                    context.Frames.Add(new FrameModel()
                    {
                        Name = _nameOfVideo,
                        Id = i,
                        Frame = ImageDataToPixelYesUnicode(bitmapFrame, height, width)
                    });
                    context.SaveChanges();

                    if (_buildFrameIsStoped)
                        break;
                }
            }
        }

        public void PlayVideo()
        {
            using (VideoDbContext context = new())
            {
                int fps = (int)_fps;
                int totalFrame = _totalFrame;

                Dictionary<int, string> currentFrame
                    = context.Frames
                    .Where(x => x.Name == _nameOfVideo)
                    .OrderBy(x => x.Id)
                    .Skip(0)
                    .Take(fps * 3)
                    .ToDictionary(x => x.Id, x => x.Frame);

                Dictionary<int, string> nextFrame
                    = context.Frames
                    .Where(x => x.Name == _nameOfVideo)
                    .OrderBy(x => x.Id)
                    .Skip(fps * 3)
                    .Take(fps * 3)
                    .ToDictionary(x => x.Id, x => x.Frame);
                string remove = currentFrame[1];

                while (true)
                {
                    if (_setupAudion?.WaveOutEvent is null || _setupAudion.AudioFileReader is null)
                        return;

                    float Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                    int frame = (int)(Fpercentage * totalFrame);

                    if (frame >= totalFrame)
                        break;

                    if (frame == 0)
                        continue;

                    if (!currentFrame.ContainsKey(frame))
                    {
                        currentFrame = nextFrame;
                        Task.Run(() =>
                        {
                            lock (_videoLock)
                            {
                                nextFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .OrderBy(x => x.Id)
                                    .Skip(frame + (fps * 3) - 1)
                                    .Take(fps * 3)
                                    .AsNoTracking()
                                    .ToDictionaryAsync(x => x.Id, x => x.Frame)
                                    .GetAwaiter().GetResult();
                            }
                        });
                    }

                    if (!currentFrame.ContainsKey(frame))
                    {
                        _setupAudion.Pause();
                        lock (_videoLock)
                        {
                            currentFrame
                                = context.Frames
                                .Where(x => x.Name == _nameOfVideo)
                                .OrderBy(x => x.Id)
                                .Skip(frame - 1)
                                .Take(fps * 3)
                                .AsNoTracking()
                                .ToDictionary(x => x.Id, x => x.Frame);

                            nextFrame
                                = context.Frames
                                .Where(x => x.Name == _nameOfVideo)
                                .OrderBy(x => x.Id)
                                .Skip(frame + (fps * 3) - 1)
                                .Take(fps * 3)
                                .AsNoTracking()
                                .ToDictionary(x => x.Id, x => x.Frame);
                        }
                        _setupAudion.Resume();
                    }

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(currentFrame[frame]);


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
                                    int lastReadyFrame;

                                    lock (_videoLock)
                                    {
                                        lastReadyFrame
                                            = context.Frames
                                            .Where(x => x.Name == _nameOfVideo)
                                            .Where(x => x.Id == frame)
                                            .Select(x => x.Id)
                                            .SingleOrDefault();
                                    }

                                    if (frame >= totalFrame || lastReadyFrame == 0)
                                    {
                                        _setupAudion.BackWard10();
                                        Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                                        frame = (int)(Fpercentage * totalFrame);
                                    }
                                    int frameN = frame <= 0 ? 1 : frame;

                                    lock (_videoLock)
                                    {
                                        currentFrame
                                            = context.Frames
                                            .Where(x => x.Name == _nameOfVideo)
                                            .OrderBy(x => x.Id)
                                            .Skip(frameN - 1)
                                            .Take(fps * 3)
                                            .AsNoTracking()
                                            .ToDictionary(x => x.Id, x => x.Frame);

                                        nextFrame
                                            = context.Frames
                                            .Where(x => x.Name == _nameOfVideo)
                                            .OrderBy(x => x.Id)
                                            .Skip(frameN + (fps * 3) - 1)
                                            .Take(fps * 3)
                                            .AsNoTracking()
                                            .ToDictionary(x => x.Id, x => x.Frame);
                                    }

                                    Console.SetCursorPosition(0, 0);
                                    Console.WriteLine(currentFrame[frameN]);
                                }
                                else if (resumeKey == ConsoleKey.LeftArrow)
                                {
                                    _setupAudion.BackWard10();

                                    Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                                    frame = (int)(Fpercentage * totalFrame);
                                    int frameN = frame <= 0 ? 1 : frame;

                                    lock (_videoLock)
                                    {
                                        currentFrame
                                            = context.Frames
                                            .Where(x => x.Name == _nameOfVideo)
                                            .OrderBy(x => x.Id)
                                            .Skip(frameN - 1)
                                            .Take(fps * 3)
                                            .AsNoTracking()
                                            .ToDictionary(x => x.Id, x => x.Frame);

                                        nextFrame
                                            = context.Frames
                                            .Where(x => x.Name == _nameOfVideo)
                                            .OrderBy(x => x.Id)
                                            .Skip(frameN + (fps * 3) - 1)
                                            .Take(fps * 3)
                                            .AsNoTracking()
                                            .ToDictionary(x => x.Id, x => x.Frame);
                                    }

                                    Console.SetCursorPosition(0, 0);
                                    Console.WriteLine(currentFrame[frameN]);
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
                            _setupAudion.Pause();
                            _setupAudion.Forward10();

                            Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                            frame = (int)(Fpercentage * totalFrame);
                            int lastReadyFrame;

                            lock (_videoLock)
                            {
                                lastReadyFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .Where(x => x.Id == frame)
                                    .Select(x => x.Id)
                                    .SingleOrDefault();
                            }

                            if (frame >= totalFrame || lastReadyFrame == 0)
                            {
                                _setupAudion.BackWard10();
                                Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                                frame = (int)(Fpercentage * totalFrame);
                            }
                            int frameN = frame <= 0 ? 1 : frame;

                            lock (_videoLock)
                            {
                                currentFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .OrderBy(x => x.Id)
                                    .Skip(frameN - 1)
                                    .Take(fps * 3)
                                    .AsNoTracking()
                                    .ToDictionary(x => x.Id, x => x.Frame);

                                nextFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .OrderBy(x => x.Id)
                                    .Skip(frameN + (fps * 3) - 1)
                                    .Take(fps * 3)
                                    .AsNoTracking()
                                    .ToDictionary(x => x.Id, x => x.Frame);
                            }
                            _setupAudion.Resume();
                        }
                        else if (ky == ConsoleKey.LeftArrow)
                        {
                            _setupAudion.Pause();
                            _setupAudion.BackWard10();

                            Fpercentage = _setupAudion.GetBytePosition() / (float)_setupAudion.AudioFileReader.Length;
                            frame = (int)(Fpercentage * totalFrame);
                            int frameN = frame <= 0 ? 1 : frame;

                            lock (_videoLock)
                            {
                                currentFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .OrderBy(x => x.Id)
                                    .Skip(frameN - 1)
                                    .Take(fps * 3)
                                    .AsNoTracking()
                                    .ToDictionary(x => x.Id, x => x.Frame);

                                nextFrame
                                    = context.Frames
                                    .Where(x => x.Name == _nameOfVideo)
                                    .OrderBy(x => x.Id)
                                    .Skip(frameN + (fps * 3) - 1)
                                    .Take(fps * 3)
                                    .AsNoTracking()
                                    .ToDictionary(x => x.Id, x => x.Frame);
                            }
                            _setupAudion.Resume();
                        }
                    }
                }

                _buildFrameIsStoped = true;
                lock (_videoLock)
                {
                    foreach (FrameModel frame in context.Frames.Where(x => x.Name == _nameOfVideo).AsEnumerable())
                        context.Frames.Remove(frame);
                    context.SaveChanges();
                }
                ClearConsole(remove);
            }
        }

        public void InitConsole()
        {
            using (VideoDbContext context = new())
            {
                string fr = context.Frames.Where(x => x.Id == 1).Single().Frame;
                Console.SetCursorPosition(0, 0);
                foreach (char item in fr)
                    Console.Write(item);
            }
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

        private string ImageDataToPixelNoUnicode(ImageData imageData, int height, int width)
        {
            StringBuilder sbFrame = new();

            int line = 0;
            for (int i = 0; i < width * height; i++)
            {
                int row = i / width;
                int col = i % width;
                int index = row * imageData.Stride + col * 1;

                int px = imageData.Data[index];


                if (!(line == row))
                {
                    line = row;
                    sbFrame.Append('\n');
                }

                //16
                // .,:;-~=≡?|[@▒▓█
                if (px <= 15)
                    sbFrame.Append("  ");
                else if (px <= 31)
                    sbFrame.Append(". ");
                else if (px <= 47)
                    sbFrame.Append(", ");
                else if (px <= 63)
                    sbFrame.Append(": ");
                else if (px <= 79)
                    sbFrame.Append("; ");
                else if (px <= 95)
                    sbFrame.Append("- ");
                else if (px <= 111)
                    sbFrame.Append("~ ");
                else if (px <= 127)
                    sbFrame.Append("! ");
                else if (px <= 143)
                    sbFrame.Append("= ");
                else if (px <= 159)
                    sbFrame.Append("+ ");
                else if (px <= 175)
                    sbFrame.Append("? ");
                else if (px <= 191)
                    sbFrame.Append("| ");
                else if (px <= 207)
                    sbFrame.Append("[ ");
                else if (px <= 223)
                    sbFrame.Append("U ");
                else if (px <= 239)
                    sbFrame.Append("0 ");
                else
                    sbFrame.Append("@ ");
            }

            return sbFrame.ToString();
        }

        private string ImageDataToPixelYesUnicode(ImageData imageData, int height, int width)
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

            return sbFrame.ToString();
        }
    }
}
