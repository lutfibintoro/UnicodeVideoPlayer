using System.Text;

namespace UnicodeThickness
{
    public static class Utama
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            string path = AppContext.BaseDirectory + "unicode\\";
            string[] files = new DirectoryInfo(path).GetFiles().Where(file => file.Extension == ".png").Select(info => info.FullName).ToArray();

            foreach (string file in files)
                GroupUnicodeThickness.CountPixelUnicode(file);

            GroupUnicodeThickness.WriteAllGroups();
        }
    }
}
