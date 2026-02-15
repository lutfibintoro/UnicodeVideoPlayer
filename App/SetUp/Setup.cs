using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.SetUp
{
    internal class Setup
    {
        internal required string VideoDir { get; set; }
        internal required string AudioDir { get; set; }
        internal required string UnicodeDir { get; init; }
        internal string[] UnicodePixel { get; } = [.. GroupUnicodeThickness.Groups];
        internal List<string> UnicodeFrame { get; } = [];


        internal void UnicodePxToSymbolPx()
        {
            for (int i = 0; i < UnicodePixel.Length; i++)
            {
                string str = char.ConvertFromUtf32(int.Parse(UnicodePixel[i].Replace("U+", ""), System.Globalization.NumberStyles.HexNumber));
                UnicodePixel[i] = str;
            }
        }
    }
}
