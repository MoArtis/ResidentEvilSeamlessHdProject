using ImageMagick;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Tesseract;

//TODO:
//1 - Async or at least refresh between files
//2 - How to refresh UI without setting again the data binding source?? I remember something about special collection, check bookmark
//3 - HAlign to center the text under the status header of the list... 
namespace MiniOCR
{
    public partial class MainWindow : Window
    {
        public static readonly List<string> ImageExtensions = new List<string> { ".TIFF", ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

        ObservableCollection<OcrFile> files;

        string ocrButtonNormalContent = "";

        public struct OcrFile
        {
            public OcrFile(FileInfo fi)
            {
                FileInfo = fi;
                Status = "";
                Confidence = 0f;
            }

            public FileInfo FileInfo { get; set; }
            public string Status { get; set; }
            public float Confidence { get; set; }

            public void ClearStatus()
            {
                Status = "";
                Confidence = 0f;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            files = new ObservableCollection<OcrFile>();
            lvFiles.ItemsSource = files;
        }

        private string GetInvertedImageName(FileInfo fi)
        {
            return string.Concat(fi.Name.Remove(fi.Name.Length - fi.Extension.Length), "_inverted", fi.Extension);
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (filePaths.Length > 50)
                    return;

                files.Clear();

                for (int i = 0; i < filePaths.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(filePaths[i]);

                    //If its extension if valid (image), add it to the list
                    if (ImageExtensions.Contains(fileInfo.Extension.ToUpper()))
                    {
                        files.Add(new OcrFile(fileInfo));
                    }
                }

            }
        }

        private delegate void ConvertImageDelegate();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (files.Count <= 0)
                return;

            OcrButton.IsEnabled = false;
            ocrButtonNormalContent = OcrButton.Content.ToString();
            OcrButton.Content = "Please wait!";
            lvFiles.AllowDrop = false;

            for (int i = 0; i < files.Count; i++)
            {
                OcrFile file = files[i];
                file.ClearStatus();
                files[i] = file;
            }

            //Start a new thread for the image processing and ocr stuff
            ConvertImageDelegate convertImageDelegate = new ConvertImageDelegate(ConvertImages);
            convertImageDelegate.BeginInvoke(null, null);
        }

        private void ConvertImages()
        {
            for (int i = 0; i < files.Count; i++)
            {
                OcrFile file = files[i];

                Bitmap bitmap = new Bitmap(file.FileInfo.FullName);
                if (bitmap == null)
                {
                    file.Status = "Not an image...";

                    //Refresh the UI
                    lvFiles.Dispatcher.BeginInvoke(DispatcherPriority.Normal, null, null);

                    continue;
                }

                string invertedImagePath = Path.Combine(file.FileInfo.DirectoryName, GetInvertedImageName(file.FileInfo));

                //First remove transparency by drawing a black background and drawing the text on it
                //Then invert the colors of the whole thing. It's better for the OCR
                //Finally scale it, also better for the OCR
                using (var b = new Bitmap(bitmap.Width, bitmap.Height))
                {
                    b.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

                    using (var g = Graphics.FromImage(b))
                    {
                        g.Clear(Color.Black);
                        g.DrawImageUnscaled(bitmap, 0, 0);
                    }

                    MagickImage magickImage = new MagickImage(b);
                    magickImage.Negate(Channels.RGB);

                    //magickImage.FilterType = FilterType.Lanczos2;
                    magickImage.Resize(1024, 1024);

                    Bitmap inverted = magickImage.ToBitmap();
                    inverted.Save(invertedImagePath);
                    inverted.Dispose();

                    magickImage.Dispose();
                }

                try
                {
                    using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromFile(invertedImagePath))
                        {
                            using (var page = engine.Process(img))
                            {
                                File.WriteAllText(string.Concat(file.FileInfo.FullName, ".txt"), page.GetText());
                                file.Confidence = page.GetMeanConfidence();
                                file.Status = string.Concat("Done! (", file.Confidence.ToString("0.00"), ")");
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    file.Status = "Tesseract Error: " + error.Message;
                }

                File.Delete(invertedImagePath);

                //Refresh the UI
                lvFiles.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateFileDelegate(UpdateFile), file, i);
            }

            //I have no idea what I'm doing...
            //lvFiles.ItemsSource = null;
            //lvFiles.ItemsSource = files;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConversionDoneDelegate(ConversionDone));
        }

        private delegate void UpdateFileDelegate(OcrFile file, int index);

        private void UpdateFile(OcrFile file, int index)
        {
            files[index] = file;
        }


        private delegate void ConversionDoneDelegate();

        private void ConversionDone()
        {
            OcrButton.IsEnabled = true;
            OcrButton.Content = ocrButtonNormalContent;
            lvFiles.AllowDrop = true;
        }
    }
}
