using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace UnicodeToImage
{
    internal static class Unicode
    {

        internal static void UnicodeImageSave(string unicode, string outputPath)
        {
            outputPath = outputPath + "unicode/";
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            outputPath = outputPath + unicode.Replace("+", "") + ".png";
            unicode = unicode.Replace("U+", "");
            int code = int.Parse(unicode, System.Globalization.NumberStyles.HexNumber);
            unicode = char.ConvertFromUtf32(code);

            SKImageInfo info = new(200, 200);
            using (SKSurface surface = SKSurface.Create(info))
            using (FileStream stream = File.OpenWrite(outputPath))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                SKPaint paint = new()
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                };

                SKFont font = new()
                {
                    Size = 150,
                    Typeface = SKTypeface.FromFamilyName("Consolas")
                };

                if (!font.ContainsGlyphs(unicode))
                    font.Typeface = SKTypeface.FromFamilyName("Segoe UI Symbol");

                font.MeasureText(unicode, out SKRect textBounds);
                float x = 200 / 2f;
                float y = 200 / 2f - textBounds.MidY;

                canvas.DrawText(unicode, x, y, SKTextAlign.Center, font, paint);


                using (SKImage image = surface.Snapshot())
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                    data.SaveTo(stream);

            }
        }
    }
}
