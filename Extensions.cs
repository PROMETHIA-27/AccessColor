using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AccessColor
{
    static class Extensions
    {
        public static System.Drawing.Point ToDrawingPoint(this System.Windows.Point point)
        {
            return new System.Drawing.Point((Int32)point.X, (Int32)point.Y);
        }

        public static System.Drawing.Point ToDrawingPoint(this (Int32 x, Int32 y) point)
        {
            return new System.Drawing.Point(point.x, point.y);
        }

        public static Int32Rect ToZoomedSlice(this System.Drawing.Point point, Int32 zoom)
        {
            return new Int32Rect(point.X - (Int32)Math.Floor(zoom / 2f), point.Y - (Int32)Math.Floor(zoom / 2f), zoom, zoom); //Might be able to swap floors for just plain int operations
        }

        public static System.Drawing.Point StartToDrawingPoint(this Int32Rect rect)
        {
            return new(rect.X, rect.Y);
        }

        public static System.Drawing.Point SizeToDrawingPoint(this Int32Rect rect)
        {
            return new(rect.Width, rect.Height);
        }

        public static System.Drawing.Size SizeToDrawingSize(this Int32Rect rect)
        {
            return new(rect.Width, rect.Height);
        }

        public static System.Drawing.Color ReadPixelInfo(this Byte[] byteArr, Int32 stride, Int32 x, Int32 y)
        {
            var bytes = byteArr[((x * stride) + (4 * y))..((x * stride) + (4 * y) + 4)];
            return System.Drawing.Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
        }

        public static System.Windows.Media.Color ReadPixelInfoMedia(this Byte[] byteArr, Int32 stride, Int32 x, Int32 y)
        {
            var bytes = byteArr[((x * stride) + (4 * y))..((x * stride) + (4 * y) + 4)];
            return System.Windows.Media.Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
        }

        public static UInt32 To32Bit(this System.Drawing.Color color)
        {
            var bits = (UInt32)color.B;
            bits += (UInt32)(color.G << 8);
            bits += (UInt32)(color.R << 16);
            bits += (UInt32)(color.A << 24);
            return bits;
        }

        public static System.Drawing.Color ToDrawingColor(this UInt32 integer)
        {
            var bytes = BitConverter.GetBytes(integer);
            return System.Drawing.Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public static (Byte r, Byte g, Byte b) ToColorTuple(this UInt32 integer)
        {
            var bytes = BitConverter.GetBytes(integer);
            return (bytes[2], bytes[1], bytes[0]);
        }

        public static UInt32 GetDistance(this (Byte x, Byte y, Byte z) lhs, (Byte x, Byte y, Byte z) rhs)
        {
            return (UInt32)Math.Sqrt(Math.Pow(lhs.x - rhs.x, 2) + Math.Pow(lhs.y - rhs.y, 2) + Math.Pow(lhs.z - rhs.z, 2));
        }
    }
}
