using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AccessColor
{
    static class ColorNamer
    {
        public static Dictionary<UInt32, String> ColorSpace { get; set; } = new();

        public static Boolean SaveColorData()
        {
            var file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AccessColor\\colorSpace.txt";

            var lines = new String[ColorSpace.Count];

            var i = 0;
            foreach (KeyValuePair<UInt32, String> pair in ColorSpace) //The :X is a neat way of specifying what format to .ToString() something with (Hexcode)
                lines[i] = $"{pair.Key:X} {pair.Value}";

            File.WriteAllLines(file, lines);

            return true;
        }

        public static Boolean LoadColorData()
        {
            ColorSpace.Clear();

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AccessColor\\";
            var file = dir + "colorSpace.txt";

            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);

            if (!File.Exists(file))
                File.Create(file).Close();

            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var slices = line.Split(' ');
                var name = "";
                UInt32? code = null;
                foreach (var slice in slices)
                {
                    if (slice.Contains("0x"))
                    {
                        code = Convert.ToUInt32(slice, 16);
                        break;
                    }
                    name += slice + " ";
                }
                name = name[..^1];
                ColorSpace[code!.Value] = name;
            }

            return true;
        }

        public static String GetClosestColorName(UInt32 colorBytes)
        {
            if (ColorSpace.Count == 0)
                return "No colors in dictionary";

            UInt32 closestColor = 0;
            var shortestDist = UInt32.MaxValue;
            (Byte r, Byte g, Byte b) colorTup = colorBytes.ToColorTuple();
            foreach (var color in ColorSpace.Keys)
            {
                var dist = colorTup.GetDistance(color.ToColorTuple());
                if (dist < shortestDist)
                {
                    closestColor = color;
                    shortestDist = dist;
                }
            }

            return ColorSpace[closestColor];
        }
    }
}
