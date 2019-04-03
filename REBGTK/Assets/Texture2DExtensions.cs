using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtensions
{
    public static void Fill(this Texture2D tex2, Color32 color)
    {
        var fillColorArray = tex2.GetPixels32();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = color;
        }

        tex2.SetPixels32(fillColorArray);
        tex2.Apply();
    }

    public static void Blur(this Texture2D image, int radius, int iterations)
    {
        Vector4 colorSum = new Vector4();

        int _windowSize = radius * 2 + 1;
        int _sourceWidth = image.width;
        int _sourceHeight = image.height;

        //var tex = image;

        var blurred = new Texture2D(image.width, image.height, image.format, false);

        for (var i = 0; i < iterations; i++)
        {
            //HORIZONTAL
            for (int imgY = 0; imgY < _sourceHeight; ++imgY)
            {
                colorSum = new Vector4();

                for (int imgX = 0; imgX < _sourceWidth; imgX++)
                {
                    if (imgX == 0)
                    {
                        for (int x = radius * -1; x <= radius; ++x)
                        {
                            var color = GetPixelWithXCheck(image, x, imgY, image.width);
                            colorSum += (Vector4)color;
                        }
                    }
                    else
                    {
                        var toExclude = GetPixelWithXCheck(image, imgX - radius - 1, imgY, image.width);
                        var toInclude = GetPixelWithXCheck(image, imgX + radius, imgY, image.width);

                        colorSum -= (Vector4)toExclude;
                        colorSum += (Vector4)toInclude;
                    }

                    blurred.SetPixel(imgX, imgY, colorSum / _windowSize);
                }
            }

            //VERTICAL
            for (int imgX = 0; imgX < _sourceWidth; imgX++)
            {
                colorSum = new Vector4();

                for (int imgY = 0; imgY < _sourceHeight; ++imgY)
                {
                    if (imgY == 0)
                    {
                        for (int y = radius * -1; y <= radius; ++y)
                        {
                            var color = GetPixelWithYCheck(blurred, imgX, y, image.height);
                            colorSum += (Vector4)color;
                        }
                    }
                    else
                    {
                        var toExclude = GetPixelWithYCheck(blurred, imgX, imgY - radius - 1, image.height);
                        var toInclude = GetPixelWithYCheck(blurred, imgX, imgY + radius, image.height);

                        colorSum -= (Vector4)toExclude;
                        colorSum += (Vector4)toInclude;
                    }

                    blurred.SetPixel(imgX, imgY, colorSum / _windowSize);
                }
            }
        }

        Graphics.CopyTexture(blurred, image);
        Object.Destroy(blurred);
    }

    private static Color GetPixelWithXCheck(Texture2D img, int x, int y, int w)
    {
        if (x <= 0) return img.GetPixel(0, y);
        if (x >= w) return img.GetPixel(w - 1, y);
        return img.GetPixel(x, y);
    }

    private static Color GetPixelWithYCheck(Texture2D img, int x, int y, int h)
    {
        if (y <= 0) return img.GetPixel(x, 0);
        if (y >= h) return img.GetPixel(x, h - 1);
        return img.GetPixel(x, y);
    }
}

