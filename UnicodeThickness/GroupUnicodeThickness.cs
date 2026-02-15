using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace UnicodeThickness
{
    public static class GroupUnicodeThickness
    {
        private static readonly List<UnicodeThickness> _groups = [];

        public static IEnumerable<string> Groups
        {
            get
            {
                Console.WriteLine("unicode character selection. . .");

                double interval = 5.5546875d;
                List<string> unicodeGroups = [];
                string[] unicodeSelect = new string[256];

                foreach (UnicodeThickness item in _groups.OrderBy(e => e.Thicknerss))
                    foreach (string unicode in item.Unicode)
                        unicodeGroups.Add(unicode);

                for(int i = 0; i < unicodeSelect.Length; i++)
                {
                    int index = (int)(i * interval);
                    unicodeSelect[i] = unicodeGroups[index];
                }

                _groups.Clear();
                return unicodeSelect;
            }
        }

        internal static void CountPixelUnicode(string fullName)
        {
            using (Mat image = CvInvoke.Imread(fullName, ImreadModes.Grayscale))
            using (Mat threshold = new())
            {
                CvInvoke.Threshold(image, threshold, 245, 255, ThresholdType.Binary);

                string unicode =
                    fullName
                    .Split('\\')
                    .Where(x =>
                    {
                        return x.EndsWith(".png");
                    })
                    .Single()
                    .Split('.')[0]
                    .Insert(1, "+");
                int zeroPixelCount = threshold.Width * threshold.Height - CvInvoke.CountNonZero(threshold);

                Grouping(unicode, zeroPixelCount);
            }
        }

        internal static void Grouping(string unicode, int zeroPixelCount)
        {
            UnicodeThickness? group = _groups.Where(unicodeThickness => unicodeThickness.Thicknerss == zeroPixelCount).SingleOrDefault();

            if (group is not null)
                group.Unicode.Add(unicode);
            else
            {
                group = new() { Thicknerss = zeroPixelCount };
                group.Unicode.Add(unicode);
                _groups.Add(group);
            }

        }

        internal static void WriteAllGroups()
        {
            List<UnicodeThickness> groups = _groups.OrderBy(e => e.Thicknerss).ToList();

            foreach (UnicodeThickness unicode in groups)
            {
                foreach (string str in unicode.Unicode)
                {
                    string stru = str.Replace("U+", "");
                    stru = char.ConvertFromUtf32(int.Parse(stru, System.Globalization.NumberStyles.HexNumber));
                    Console.WriteLine(unicode.Thicknerss + "\t:\t" + str + "\t:\t" + stru);
                }
            }
        }


        public static void UnicodePX(string unicodeDir)
        {
            Console.WriteLine("check valid unicode characters. . .");
            string[] files = new DirectoryInfo(unicodeDir).GetFiles().Where(file => file.Extension == ".png").Select(info => info.FullName).ToArray();
            if (files.Length < 1422)
            {
                Console.WriteLine("unicode characters are not enough. . .");
                return;
            }

            Console.WriteLine("reading unicode characters. . .");
            foreach (string file in files)
                GroupUnicodeThickness.CountPixelUnicode(file);
        }
    }
}
