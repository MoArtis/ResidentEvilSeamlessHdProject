using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Media;
using ImageMagick;
using System.Drawing;

namespace RESHDP_FmvFilesManager
{
    public enum FrameskipRatio
    {
        Zero,
        OneOverTwo,
        OneOverThree,
        OneOverFour
        //TwoOverThree,
        //ThreeOverFour
    }

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public FrameskipRatio CurrentFrameskipRatio { get; set; }
        public bool IgnoreUniques { get; set; }
        public bool MarkTexture { get; set; }

        public string CurrentPackPath { get; set; } = "Pick a folder first...";

        private FileManager fm = new FileManager();

        private Dictionary<string, List<FileInfo>> textures = new Dictionary<string, List<FileInfo>>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string arg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
        }

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        private void Button_FrameNames(object sender, RoutedEventArgs e)
        {
            //No directory picked
            DirectoryInfo di = new DirectoryInfo(CurrentPackPath);
            if (di.Exists == false)
            {
                MessageBox.Show("Please enter the root directory of the pack first...", "No directory", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int frameCount = fm.LoadFiles(CurrentPackPath, "png");

            if (frameCount <= 0)
            {
                MessageBox.Show("There is no png files in the selected folder...", "Missing frame names file", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
                return;
            }


            //fm.fileInfos = fm.fileInfos.OrderBy(x => x.LastWriteTimeUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToArray();
            fm.fileInfos = fm.fileInfos.OrderBy(x => x.LastWriteTimeUtc).ToArray();

            string textFilePath = Path.Combine(di.Parent.FullName, di.Parent.Name + ".txt");
            TextWriter tw = File.CreateText(textFilePath);

            for (int i = 0; i < frameCount - 1; i++)
            {
                //Console.WriteLine(fm.fileInfos[i].LastWriteTimeUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                tw.WriteLine(fm.fileInfos[i].Name);
            }

            tw.Write(fm.fileInfos[frameCount - 1].Name);

            tw.Close();
        }

        private void Button_RemoveAlpha(object sender, RoutedEventArgs e)
        {
            //No directory picked
            DirectoryInfo di = new DirectoryInfo(CurrentPackPath);
            if (di.Exists == false)
            {
                MessageBox.Show("Please enter the root directory of the pack first...", "No directory", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SwitchInterface(false);

            //Start a new thread
            RemoveAlphaDelegate removeAlphaDelegate = new RemoveAlphaDelegate(RemoveAlpha);
            removeAlphaDelegate.BeginInvoke(null, null);

        }

        private delegate void RemoveAlphaDelegate();

        private void RemoveAlpha()
        {
            int fileCount = fm.LoadFiles(CurrentPackPath, "png", SearchOption.AllDirectories);

            for (int i = 0; i < fileCount; i++)
            {
                FileInfo fi = fm.fileInfos[i];

                DateTime creationTime = fi.CreationTime;
                DateTime writeTime = fi.LastWriteTime;
                DateTime accessTime = fi.LastAccessTime;

                using (MagickImage image = new MagickImage(fi))
                {
                    image.Alpha(AlphaOption.Opaque);
                    image.Write(fi, MagickFormat.Png24);

                    fi.CreationTime = creationTime;
                    fi.LastWriteTime = writeTime;
                    fi.LastAccessTime = accessTime;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskProgressDelegate(() => TaskProgress(i, fileCount)));
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
        }

        private void Button_Generate(object sender, RoutedEventArgs e)
        {
            //No directory picked
            DirectoryInfo di = new DirectoryInfo(CurrentPackPath);
            if (di.Exists == false)
            {
                MessageBox.Show("Please enter the root directory of the pack first...", "No directory", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SwitchInterface(false);

            //Start a new thread for the image processing and ocr stuff
            GenerateFmvFilesDelegate generateFmvFilesDelegate = new GenerateFmvFilesDelegate(GenerateFmvFiles);
            generateFmvFilesDelegate.BeginInvoke(null, null);
        }

        private delegate void GenerateFmvFilesDelegate();

        private void GenerateFmvFiles()
        {
            fm.LoadFiles(CurrentPackPath, "txt");

            if (fm.fileInfos.Length <= 0)
            {
                MessageBox.Show("The folder must include the list of frame names.", "Missing frame names file", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
                return;
            }

            string fmvName = fm.RemoveExtensionFromFileInfo(fm.fileInfos[0]);
            string[] frameNames = File.ReadAllText(fm.fileInfos[0].FullName).Split('\n');

            string upscalePath = Path.Combine(CurrentPackPath, "Upscale");

            if (Directory.Exists(upscalePath) == false)
            {
                MessageBox.Show("The upscale folder is missing.", "Upscale folder missing.", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
                return;
            }

            int upscaleImgCount = fm.LoadFiles(upscalePath, "png");

            if (upscaleImgCount != frameNames.Length)
            {
                MessageBox.Show("Inconsistent number of frames between the Frame names file and the upscale folder.", "Inconsistent number of frames", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
                return;
            }

            fmvName = string.Format("{0}_{1}{2}", fmvName,
                 fm.GetBitmapFromFileInfo(fm.fileInfos[0]).PixelHeight,
                 (CurrentFrameskipRatio == FrameskipRatio.Zero ? "" : "_" + CurrentFrameskipRatio));

            string fmvPath = Path.Combine(CurrentPackPath, fmvName);

            fm.CreateDirectory(fmvPath);


            for (int i = 0; i < upscaleImgCount; i++)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskProgressDelegate(() => TaskProgress(i, upscaleImgCount)));

                switch (CurrentFrameskipRatio)
                {
                    case FrameskipRatio.OneOverTwo:
                        if (i % 2 == 1)
                            continue;
                        break;

                    case FrameskipRatio.OneOverThree:
                        if (i % 3 == 2)
                            continue;
                        break;

                    case FrameskipRatio.OneOverFour:
                        if (i % 4 == 3)
                            continue;
                        break;
                }

                string frameFullName = Path.Combine(fmvPath, frameNames[i].Trim());

                //Attempt to optimize the PNG files by changing the format and removing the alpha layer.
                //Result: the size difference is minimal while the processing time is 1000% longer.
                //using (MagickImage image = new MagickImage(fm.fileInfos[i]))
                //{
                //    image.Alpha(AlphaOption.Off);
                //    image.Write(frameFullName, MagickFormat.Png24);
                //}

                fm.fileInfos[i].CopyTo(frameFullName, true);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskProgressDelegate(() => TaskProgress(1, 1)));
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TaskDoneDelegate(TaskDone));
        }

        private void Button_Browse(object sender, RoutedEventArgs e)
        {
            bool hasFolderPath = fm.SelectFolder(out string packPath, string.Empty, AppDomain.CurrentDomain.BaseDirectory);

            if (hasFolderPath)
            {
                CurrentPackPath = packPath;
                OnPropertyChanged(nameof(CurrentPackPath));
            }
        }

        private delegate void TaskDoneDelegate();

        private void TaskDone()
        {
            SwitchInterface(true);

            SystemSounds.Exclamation.Play();
        }

        private void SwitchInterface(bool isEnable)
        {
            buttonBrowse.IsEnabled = isEnable;
            buttonGenerate.IsEnabled = isEnable;
            packFolderPath.IsEnabled = isEnable;
            dropDownFrameskipRatio.IsEnabled = isEnable;
            buttonRemoveAlpha.IsEnabled = isEnable;
        }

        private delegate void TaskProgressDelegate();

        private void TaskProgress(int fileIndex, int filesCount)
        {
            progressBar.Value = (fileIndex / (double)filesCount) * 100.0;
        }
    }
}
