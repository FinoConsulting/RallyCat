using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace RallyCat.Core.Helpers
{
    public static class GraphicsHelper
    {
        private const Int32 c_Channels = 4;

        public static Bitmap CreateShadow(this Bitmap bitmap, Int32 radius, Single opacity)
        {
            // Alpha mask with opacity
            var matrix = new ColorMatrix(new[]
            {
                new[] {  0F,  0F,  0F, 0F     , 0F },
                new[] {  0F,  0F,  0F, 0F     , 0F },
                new[] {  0F,  0F,  0F, 0F     , 0F },
                new[] { -1F, -1F, -1F, opacity, 0F },
                new[] {  1F,  1F,  1F, 0F     , 1F }
            });

            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var shadow = new Bitmap(bitmap.Width + 4 * radius, bitmap.Height + 4 * radius);
            using (var graphics = Graphics.FromImage(shadow))
            {
                var rectangle = new Rectangle(2 * radius, 2 * radius, bitmap.Width, bitmap.Height);
                graphics.DrawImage(bitmap, rectangle, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            // Gaussian blur
            var shadowBox  = new Rectangle(0, 0, shadow.Width, shadow.Height);
            var shadowData = shadow.LockBits(shadowBox, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            var clone      = shadow.Clone() as Bitmap;
            if (clone == null) { throw new NullReferenceException(); }
            var cloneData  = clone.LockBits(shadowBox, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

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
            var p1 = (Byte*)(void*)data1.Scan0;
            var p2 = (Byte*)(void*)data2.Scan0;

            var radius2    = 2 * radius + 1;
            var sum        = new Int32[c_Channels];
            var firstValue = new Int32[c_Channels];
            var lastValue  = new Int32[c_Channels];

            // Horizontal
            var stride = data1.Stride;
            for (var row = 0; row < height; row++)
            {
                var start = row * stride;
                var left  = start;
                var right = start + radius * c_Channels;

                for (var channel = 0; channel < c_Channels; channel++)
                {
                    firstValue[channel] = p1[start + channel];
                    lastValue[channel]  = p1[start + (width - 1) * c_Channels + channel];
                    sum[channel]        = (radius + 1) * firstValue[channel];
                }

                for (var column = 0; column < radius; column++)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p1[start + column * c_Channels + channel];
                    }
                }

                for (var column = 0; column <= radius; column++, right += c_Channels, start += c_Channels)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p1[right + channel] - firstValue[channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }

                for (var column = radius + 1; column < width - radius; column++, left += c_Channels, right += c_Channels, start += c_Channels)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p1[right + channel] - p1[left + channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }

                for (var column = width - radius; column < width; column++, left += c_Channels, start += c_Channels)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += lastValue[channel] - p1[left + channel];
                        p2[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }
            }

            // Vertical
            stride = data2.Stride;
            for (var column = 0; column < width; column++)
            {
                var start  = column * c_Channels;
                var top    = start;
                var bottom = start + radius * stride;

                for (var channel = 0; channel < c_Channels; channel++)
                {
                    firstValue[channel] = p2[start + channel];
                    lastValue[channel]  = p2[start + (height - 1) * stride + channel];
                    sum[channel]        = (radius + 1) * firstValue[channel];
                }

                for (var row = 0; row < radius; row++)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p2[start + row * stride + channel];
                    }
                }

                for (var row = 0; row <= radius; row++, bottom += stride, start += stride)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p2[bottom + channel] - firstValue[channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }

                for (var row = radius + 1; row < height - radius; row++, top += stride, bottom += stride, start += stride)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += p2[bottom + channel] - p2[top + channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }

                for (var row = height - radius; row < height; row++, top += stride, start += stride)
                {
                    for (var channel = 0; channel < c_Channels; channel++)
                    {
                        sum[channel] += lastValue[channel] - p2[top + channel];
                        p1[start + channel] = (Byte)(sum[channel] / radius2);
                    }
                }
            }
        }

        private static Int32[] DetermineBoxes(Double sigma, Int32 boxCount)
        {
            var idealWidth = Math.Sqrt(12 * sigma * sigma / boxCount + 1);
            var lower = (Int32)Math.Floor(idealWidth);
            if (lower % 2 == 0) { lower--; }

            var upper       = lower + 2;
            var medianWidth = (12 * sigma * sigma - boxCount * lower * lower - 4 * boxCount * lower - 3 * boxCount) / (-4 * lower - 4);
            var median      = (Int32)Math.Round(medianWidth);
            var boxSizes    = new Int32[boxCount];

            for (var i = 0; i < boxCount; i++)
            {
                boxSizes[i] = i < median ? lower : upper;
            }

            return boxSizes;
        }
    }
}