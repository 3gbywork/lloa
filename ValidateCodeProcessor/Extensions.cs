using System.Drawing;
using System.IO;

namespace ValidateCodeProcessor
{
    public static class Extensions
    {
        public static Bitmap ToBitmap(this Stream stream)
        {
            return Image.FromStream(stream) as Bitmap;
        }

        public static Bitmap Gray(this Bitmap bitmap)
        {
            return PreProcessor.ConvertTo8BitBitmap(bitmap);
        }

        public static Bitmap DeNoise(this Bitmap bitmap, byte threshold = 30, byte bg = byte.MaxValue)
        {
            return PreProcessor.DeNoise(bitmap, threshold, bg);
        }

        public static Bitmap Binarize(this Bitmap bitmap, byte threshold = byte.MaxValue , byte bg = byte.MaxValue, byte fg = byte.MinValue)
        {
            return PreProcessor.Binarize(bitmap, threshold, bg, fg);
        }

        public static string Text(this Bitmap bitmap)
        {
            return OcrProcessor.Instance.GetTextFromImage(bitmap);
        }
    }
}
