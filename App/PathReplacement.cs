using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMediaToolkit.Graphics;
using SkiaSharp;

namespace App
{
    internal static class PathReplacement
    {
        internal static string VideoFullPathToAudioFullPath(string videoPath)
        {
            return AppContext.BaseDirectory + @"audio\" + videoPath.Split('\\')[^1].Split('.')[0] + ".mp3";
        }

        internal static string SaveFrameVideo(SKBitmap imageData, int inFrame, string videoPath)
        {
            string outputPath = AppContext.BaseDirectory + "frame\\" + videoPath.Split('\\')[^1].Split('.')[0];
            string fullPathFrame = outputPath + $"\\frame_{inFrame}.png";
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);


            using (FileStream fs = File.OpenWrite(fullPathFrame))
                imageData.Encode(fs, SKEncodedImageFormat.Png, 100);


            return outputPath;
        }
    }
}
