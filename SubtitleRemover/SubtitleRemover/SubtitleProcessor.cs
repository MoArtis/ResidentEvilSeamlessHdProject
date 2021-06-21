using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Tesseract;
using ImageMagick;

namespace SubtitleRemover
{
    public class SubtitleProcessor : IDisposable
    {
        private readonly TesseractEngine _tessEngine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);

        private float _preprocessScale = 2f;

        private static byte[] PreprocessImage(IMagickImage sourceImage, Rectangle cropRegion, float scale,
            string cropRegionPath = "")
        {
            using var mImage = new MagickImage(MagickColors.White, cropRegion.Width, cropRegion.Height);

            mImage.Format = MagickFormat.Png;

            if (cropRegion.Width > 0 && cropRegion.Height > 0)
                mImage.CopyPixels(sourceImage,
                    new MagickGeometry(cropRegion.X, cropRegion.Y, cropRegion.Width, cropRegion.Height));
            else
                mImage.CopyPixels(sourceImage);

            mImage.Threshold(new Percentage(65f)); // 60 is OK 
            mImage.Depth = 1;

            // mImage.GammaCorrect(0.20); //0.20
            // mImage.BlackThreshold(new Percentage(50f), Channels.RGB);

            mImage.ColorSpace = ColorSpace.Gray;

            mImage.Negate(Channels.RGB);

            if (scale != 0f && !(Math.Abs(scale - 1f) < float.Epsilon))
            {
                mImage.FilterType = FilterType.Cubic;
                mImage.Resize(new Percentage(scale * 100.0f));
            }

            mImage.BlackThreshold(new Percentage(55f), Channels.RGB);
            mImage.GaussianBlur(0.85, 0.85);

            // mImage.BlackThreshold(new Percentage(50f), Channels.RGB);
            // mImage.MedianFilter(2);
            // mImage.BlackThreshold(new Percentage(20f), Channels.RGB);

            if (!string.IsNullOrEmpty(cropRegionPath))
                mImage.Write(cropRegionPath);

            return mImage.ToByteArray();
        }

        public List<Rectangle> GetSubtitleBoundingBoxes(IMagickImage mImage, Rectangle subtitleRegion,
            PageIteratorLevel pageIteratorLevel, string cropRegionPath = "")
        {
            //Preprocess
            var imageBytes = PreprocessImage(mImage, subtitleRegion, _preprocessScale, cropRegionPath);

            List<Rectangle> boundingBoxes = new List<Rectangle>();

            try
            {
                _tessEngine.SetVariable("user_defined_dpi", 300);
                _tessEngine.DefaultPageSegMode = PageSegMode.Auto;

                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = _tessEngine.Process(img);

                //The Iterator way - Same results...
                using var iter = page.GetIterator();
                iter.Begin();

                do
                {
                    if (!iter.TryGetBoundingBox(pageIteratorLevel, out var boundingBoxRect))
                        continue;

                    var boundingBox = new Rectangle(
                        (int) (boundingBoxRect.X1 / _preprocessScale) + subtitleRegion.X,
                        (int) (boundingBoxRect.Y1 / _preprocessScale) + subtitleRegion.Y,
                        (int) (boundingBoxRect.Width / _preprocessScale),
                        (int) (boundingBoxRect.Height / _preprocessScale));

                    boundingBoxes.Add(boundingBox);

                    Console.WriteLine($"Text:{iter.GetText(pageIteratorLevel).Trim()} - Bounding Box:{boundingBox}");
                } while (iter.Next(pageIteratorLevel));

                return boundingBoxes;
            }
            catch (Exception error)
            {
                Console.WriteLine("Tesseract Error: " + error.Message);
            }

            Console.WriteLine(boundingBoxes.Count);

            return boundingBoxes;
        }

        public void DrawInPaintingMasks(MagickImage mImage, List<Rectangle> maskRegions, MagickColor maskColor,
            double maskOversize = 0.0)
        {
            var drawables = new Drawables();

            drawables.StrokeColor(maskColor);
            drawables.StrokeOpacity(new Percentage(0));
            drawables.FillColor(maskColor);

            foreach (var maskRegion in maskRegions)
            {
                var (x, y, w, h) = (maskRegion.X, maskRegion.Y, maskRegion.Width, maskRegion.Height);
                drawables.Rectangle(x - maskOversize, y - maskOversize, x + w + maskOversize * 2.0,
                    y + h + maskOversize * 2.0);
            }

            drawables.Draw(mImage);
        }

        public void Dispose()
        {
            _tessEngine?.Dispose();
        }
    }
}


// Check if the region is correct
// var pixToBitmapConverter = new PixToBitmapConverter();
// pixToBitmapConverter.Convert(page.Image);

// var regionBitmap = PixConverter.ToBitmap(page.Image);
// regionBitmap = regionBitmap.Clone(new Rectangle(region.X1, region.Y1, region.Width, region.Height), regionBitmap.PixelFormat);
// regionBitmap.Save(imagePath + ".region.png", System.Drawing.Imaging.ImageFormat.Png);

/*Console.WriteLine($"{img.Width} - {img.Height}");

var text = page.GetText();
Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

Console.WriteLine("Text (GetText): \r\n{0}", text);*/

//The Simple way
/*boundingBoxes = page.GetSegmentedRegions(pageIteratorLevel);

for (var i = 0; i < boundingBoxes.Count; i++)
{
    var boundingBox = boundingBoxes[i];

    //Rescale the box
    boundingBox.X = (int) (boundingBox.X / _preprocessScale);
    boundingBox.Y = (int) (boundingBox.Y / _preprocessScale);
    boundingBox.Width = (int) (boundingBox.Width / _preprocessScale);
    boundingBox.Height = (int) (boundingBox.Height / _preprocessScale);

    //Add back the cropping
    boundingBox.X += subtitleRegion.X;
    boundingBox.Y += subtitleRegion.Y;

    boundingBoxes[i] = boundingBox;
}*/