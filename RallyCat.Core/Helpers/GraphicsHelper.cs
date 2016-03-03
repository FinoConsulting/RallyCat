using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.Helpers
{
    public static class GraphicsHelper
    {
        const Int32 CHANNELS = 4;
        public static Bitmap CreateShadow(this Bitmap bitmap, Int32 radius, Single opacity)
        {
            
            // Alpha mask with opacity
            var matrix = new ColorMatrix(new Single[][] {
            new Single[] {  0F,  0F,  0F, 0F,      0F }, 
            new Single[] {  0F,  0F,  0F, 0F,      0F }, 
            new Single[] {  0F,  0F,  0F, 0F,      0F }, 
            new Single[] { -1F, -1F, -1F, opacity, 0F },
            new Single[] {  1F,  1F,  1F, 0F,      1F }
        });

            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            var shadow = new Bitmap(bitmap.Width + 4 * radius, bitmap.Height + 4 * radius);
            using (var graphics = Graphics.FromImage(shadow))
                graphics.DrawImage(bitmap, new Rectangle(2 * radius, 2 * radius, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);

            // Gaussian blur
            var clone = shadow.Clone() as Bitmap;
            var shadowData = shadow.LockBits(new Rectangle(0, 0, shadow.Width, shadow.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var cloneData = clone.LockBits(new Rectangle(0, 0, clone.Width, clone.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            var boxes = DetermineBoxes(radius, 3);
            BoxBlur(shadowData, cloneData, shadow.Width, shadow.Height, (boxes[0] - 1) / 2);
            BoxBlur(shadowData, cloneData, shadow.Width, shadow.Height, (boxes[1] - 1) / 2);
            BoxBlur(shadowData, cloneData, shadow.Width, shadow.Height, (boxes[2] - 1) / 2);

            shadow.UnlockBits(shadowData);
            clone.UnlockBits(cloneData);
            return shadow;
        }

        private static unsafe void BoxBlur(BitmapData data1, BitmapData data2, Int32 width, Int32 height, Int32 radius)
        {
            Byte* p1 = (Byte*)(void*)data1.Scan0;
            
            Byte* p2 = (Byte*)(void*)data2.Scan0;

            Int32 radius2 = 2 * radius + 1;
            Int32[] sum = new Int32[CHANNELS];
            Int32[] FirstValue = new Int32[CHANNELS];
            Int32[] LastValue = new Int32[CHANNELS];

            // Horizontal
            Int32 stride = data1.Stride;
            for (var row = 0; row < height; row++)
            {
                Int32 start = row * stride;
                Int32 left = start;
                Int32 right = start + radius * CHANNELS;

                for (Int32 channel = 0; channel < CHANNELS; channel++)
                {
                    FirstValue[channel] = p1[start + channel];
                    LastValue[channel] = p1[start + (width - 1) * CHANNELS + channel];
                    sum[channel] = (radius + 1) * FirstValue[channel];
                }
                for (var column = 0; column < radius; column++)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                        sum[channel] += p1[start + column * CHANNELS + channel];
                for (var column = 0; column <= radius; column++, right += CHANNELS, start += CHANNELS)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += p1[right + channel] - FirstValue[channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                for (var column = radius + 1; column < width - radius; column++, left += CHANNELS, right += CHANNELS, start += CHANNELS)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += p1[right + channel] - p1[left + channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                for (var column = width - radius; column < width; column++, left += CHANNELS, start += CHANNELS)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += LastValue[channel] - p1[left + channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
            }

            // Vertical
            stride = data2.Stride;
            for (Int32 column = 0; column < width; column++)
            {
                Int32 start = column * CHANNELS;
                Int32 top = start;
                Int32 bottom = start + radius * stride;

                for (Int32 channel = 0; channel < CHANNELS; channel++)
                {
                    FirstValue[channel] = p2[start + channel];
                    LastValue[channel] = p2[start + (height - 1) * stride + channel];
                    sum[channel] = (radius + 1) * FirstValue[channel];
                }
                for (Int32 row = 0; row < radius; row++)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                        sum[channel] += p2[start + row * stride + channel];
                for (Int32 row = 0; row <= radius; row++, bottom += stride, start += stride)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += p2[bottom + channel] - FirstValue[channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                for (Int32 row = radius + 1; row < height - radius; row++, top += stride, bottom += stride, start += stride)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += p2[bottom + channel] - p2[top + channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                for (Int32 row = height - radius; row < height; row++, top += stride, start += stride)
                    for (Int32 channel = 0; channel < CHANNELS; channel++)
                    {
                        sum[channel] += LastValue[channel] - p2[top + channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
            }
        }

        private static Int32[] DetermineBoxes(Double Sigma, Int32 BoxCount)
        {
            Double IdealWidth = Math.Sqrt((12 * Sigma * Sigma / BoxCount) + 1);
            Int32 Lower = (Int32)Math.Floor(IdealWidth);
            if (Lower % 2 == 0)
                Lower--;
            Int32 Upper = Lower + 2;

            Double MedianWidth = (12 * Sigma * Sigma - BoxCount * Lower * Lower - 4 * BoxCount * Lower - 3 * BoxCount) / (-4 * Lower - 4);
            Int32 Median = (Int32)Math.Round(MedianWidth);

            Int32[] BoxSizes = new Int32[BoxCount];
            for (Int32 i = 0; i < BoxCount; i++)
                BoxSizes[i] = (i < Median) ? Lower : Upper;
            return BoxSizes;
        }
    }
}
