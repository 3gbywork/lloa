using System.Drawing;
using System.Drawing.Imaging;

namespace ValidateCodeProcessor
{
    public class PreProcessor
    {
        public static unsafe Bitmap ConvertTo8BitBitmap(Bitmap img)
        {
            var bit = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                           PixelFormat.Format24bppRgb);
            var bp = (byte*)data.Scan0.ToPointer();
            BitmapData data2 = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite,
                                            PixelFormat.Format8bppIndexed);
            var bp2 = (byte*)data2.Scan0.ToPointer();
            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    //0.3R+0.59G+0.11B
                    float value = 0.11F * bp[i * data.Stride + j * 3] + 0.59F * bp[i * data.Stride + j * 3 + 1] +
                                  0.3F * bp[i * data.Stride + j * 3 + 2];
                    bp2[i * data2.Stride + j] = (byte)value;
                }
            }
            img.UnlockBits(data);
            bit.UnlockBits(data2);
            ColorPalette palette = bit.Palette;
            for (int i = 0; i != palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bit.Palette = palette;
            //bit.Save(destFile, ImageFormat.Bmp);
            img.Dispose();
            return bit;
            //bit.Dispose();
        }

        /// <summary>
        /// 去噪声
        /// </summary>
        /// <param name="img">灰度图像</param>
        /// <param name="threshold">阈值</param>
        /// <param name="bg">背景色</param>
        /// <returns></returns>
        public static unsafe Bitmap DeNoise(Bitmap img, byte threshold = 30, byte bg = byte.MaxValue)
        {
            byte bgValue = (byte)(bg - threshold);

            var bit = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                           PixelFormat.Format8bppIndexed);
            var bp = (byte*)data.Scan0.ToPointer();
            BitmapData data2 = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite,
                                            PixelFormat.Format8bppIndexed);
            var bp2 = (byte*)data2.Scan0.ToPointer();
            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    var value = bp[i * data.Stride + j];
                    bp2[i * data2.Stride + j] = value > bgValue ? bg : value;
                }
            }

            for (int i = 1; i < data2.Height - 1; i++)
            {
                for (int j = 1; j < data2.Width - 1; j++)
                {
                    if (bp2[i * data2.Stride + j] != bg)
                    {
                        var preIsBg = bp2[i * data2.Stride + j - 1] == bg;
                        var nextIsBg = bp2[i * data2.Stride + j + 1] == bg;

                        if (preIsBg && nextIsBg)
                        {
                            bp2[i * data2.Stride + j] = bg;
                        }
                    }
                }
            }
            img.UnlockBits(data);
            bit.UnlockBits(data2);
            ColorPalette palette = bit.Palette;
            for (int i = 0; i != palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bit.Palette = palette;
            //bit.Save(destFile, ImageFormat.Bmp);
            img.Dispose();
            return bit;
        }

        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="img">灰度图</param>
        /// <param name="threshold">阈值</param>
        /// <param name="bg">前景色</param>
        /// <param name="fg">背景色</param>
        /// <returns></returns>
        public static unsafe Bitmap Binarize(Bitmap img, byte threshold = byte.MaxValue , byte bg = byte.MaxValue, byte fg = byte.MinValue)
        {
            var bit = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                           PixelFormat.Format8bppIndexed);
            var bp = (byte*)data.Scan0.ToPointer();
            BitmapData data2 = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite,
                                            PixelFormat.Format8bppIndexed);
            var bp2 = (byte*)data2.Scan0.ToPointer();
            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    var value = bp[i * data.Stride + j];
                    bp2[i * data2.Stride + j] = value >= threshold ? bg : fg;
                }
            }

            img.UnlockBits(data);
            bit.UnlockBits(data2);
            ColorPalette palette = bit.Palette;
            for (int i = 0; i != palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bit.Palette = palette;
            //bit.Save(destFile, ImageFormat.Bmp);
            img.Dispose();
            return bit;
        }
    }
}
