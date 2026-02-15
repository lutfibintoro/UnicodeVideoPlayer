using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Text;
using App.Audio;
using App.LocalDb;
using App.SetUp;
using App.Video;
using Emgu.CV.Ocl;
using FFMediaToolkit.Graphics;
using FFmpeg.AutoGen;
using Microsoft.EntityFrameworkCore;

namespace App
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            Xabe.FFmpeg.FFmpeg.SetExecutablesPath(AppContext.BaseDirectory + "ffmpeg\\bin");
            FFMediaToolkit.FFmpegLoader.FFmpegPath = AppContext.BaseDirectory + "ffmpeg\\bin";
            Console.OutputEncoding = Encoding.Unicode;
            if (!VideoOrUnicodeDirExists())
                return;

            Setup setup = new()
            {
                UnicodeDir = AppContext.BaseDirectory + @"unicode\",
                VideoDir = AppContext.BaseDirectory + @"video\",
                AudioDir = AppContext.BaseDirectory + @"audio\"
            };

            //using (VideoDbContext context = new())
            //{
            //    foreach (FrameModel frame in context.Frames.AsEnumerable())
            //        context.Frames.Remove(frame);
            //    context.SaveChanges();
            //}

            Play play = new(setup, new SetupAudio(), new SetupVideoMemoryFrame());
            play.Start();
        }


        private static bool VideoOrUnicodeDirExists()
        {
            Console.WriteLine("check valid directory. . .");
            string unicodeDir = AppContext.BaseDirectory + @"unicode\";
            string videoDir = AppContext.BaseDirectory + @"video\";
            string audioDir = AppContext.BaseDirectory + @"audio\";
            bool exists = true;

            if (!Directory.Exists(unicodeDir))
            {
                Console.WriteLine("unicode folder not found");
                exists = false;
            }

            if (!Directory.Exists(videoDir))
            {
                Console.WriteLine("video folder not found");
                exists = false;
            }

            if (!Directory.Exists(audioDir))
            {
                Console.WriteLine("audio folder not found");
                exists = false;
            }

            GroupUnicodeThickness.UnicodePX(unicodeDir);
            return exists;
        }
    }
}
