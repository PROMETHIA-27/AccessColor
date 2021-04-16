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
            return PlainTextSerialization.SaveFromDictionary(
                ColorSpace,
                "colorSpace.txt",
                (x) => x.ToString("X"),
                (x) => x.ToString().Replace(' ', '_')
            );
        }

        public static Boolean LoadColorData()
        {
            ColorSpace.Clear();

            return PlainTextSerialization.LoadToDictionary(
                "colorSpace",
                ColorSpace,
                (s) => Convert.ToUInt32(s, 16),
                (s) => s.Replace('_', ' '),
                AppContext.BaseDirectory + "\\colorSpace.txt"
            );
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
