using System;
using System.Drawing;
using System.IO;
using ImageMagick;
using Tesseract;

namespace SubtitleRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new[]
            {
                @"D:\Projects\ResidentEvilSeamlessHdProject\ResidentEvilSeamlessHdProject\SubtitleRemover\SubtitleRemover\Errors"
            };

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var inputPath = args.Length != 1 ? "./input" : args[0];

            var program = new Program();
            program.Run(inputPath);
            // program.RunTest();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private void Run(string inputPath)
        {
            var inputDi = new DirectoryInfo(inputPath);

            if (!inputDi.Exists)
            {
                Console.WriteLine(
                    "No input folder - make sure to drag & drop a folder or have an input folder next to the exe.");
                return;
            }

            var inputFiles = inputDi.GetFiles("*.png", SearchOption.TopDirectoryOnly);

            if (inputFiles.Length <= 0)
            {
                Console.WriteLine(
                    "No png files in the input folder.");
                return;
            }

            //Create the output folder
            var outputDi = new DirectoryInfo(Path.Combine(inputDi.FullName, "Result"));
            if (!outputDi.Exists)
                outputDi.Create();

            var subtitleRegion = new Rectangle(140, 375, 360, 80);

            using var subtitleProcessor = new SubtitleProcessor();

            var maskColor = new MagickColor("#0F0F");

            foreach (var inputFile in inputFiles)
            {
                Console.WriteLine($"Processing {inputFile.Name}");

                if (!inputFile.Exists)
                    continue;

                using var mImage = new MagickImage(inputFile);

                var imageName = Path.GetFileNameWithoutExtension(inputFile.FullName);

                var boundingBoxes =
                    subtitleProcessor.GetSubtitleBoundingBoxes(mImage, subtitleRegion, PageIteratorLevel.Block, Path.Combine(outputDi.FullName, $"{imageName}_crop.png"));

                var resultImagePath = Path.Combine(outputDi.FullName, $"{imageName}.png");

                if (boundingBoxes == null || boundingBoxes.Count <= 0)
                {
                    //Just copy
                    inputFile.CopyTo(resultImagePath, true);
                    mImage.Dispose();
                    continue;
                }

                subtitleProcessor.DrawInPaintingMasks(mImage, boundingBoxes, maskColor, 2.0);

                mImage.Write(resultImagePath);
                mImage.Dispose();
            }
        }

        private void RunTest()
        {
            var testImagesFolderPath =
                @"D:\Projects\ResidentEvilSeamlessHdProject\ResidentEvilSeamlessHdProject\SubtitleRemover\SubtitleRemover\TestImages";

            var testImagePath = Path.Combine(testImagesFolderPath, "000001494.png");
            // var testImagePath = Path.Combine(testImagesFolderPath, "000001507.png");
            // var testImagePath = Path.Combine(testImagesFolderPath, "000000203.png");

            var imageFi = new FileInfo(testImagePath);

            if (imageFi.Exists == false)
                return;

            using var mImage = new MagickImage(imageFi);

            var subtitleRegion = new Rectangle(140, 375, 360, 80);

            using var subtitleProcessor = new SubtitleProcessor();

            var folderPath = imageFi.DirectoryName;
            var imageName = Path.GetFileNameWithoutExtension(imageFi.FullName);

            var boundingBoxes =
                subtitleProcessor.GetSubtitleBoundingBoxes(mImage, subtitleRegion, PageIteratorLevel.Block,
                    Path.Combine(folderPath, $"{imageName}_crop.png"));

            if (boundingBoxes == null)
                return;

            //Generate a random color
            // var rand = new Random();
            // var r = (ushort) rand.Next(0, ushort.MaxValue);
            // var g = (ushort) rand.Next(0, ushort.MaxValue);
            // var b = (ushort) rand.Next(0, ushort.MaxValue);
            // var color = new MagickColor(r, g, b, ushort.MaxValue);

            subtitleProcessor.DrawInPaintingMasks(mImage, boundingBoxes, new MagickColor("#0F0F"), 2.0);

            var resultPath = Path.Combine(folderPath, $"{imageName}_result.png");
            mImage.Write(resultPath);

            Console.WriteLine(resultPath);

            mImage.Dispose();
        }
    }
}