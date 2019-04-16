using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Media;
using System.IO.Compression;

namespace RESHDP_PackConv
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public string CurrentPackPath { get; set; } = "Enter a pack location first...";
        public FolderStructure Fs { get; set; }

        private const string RESPACKASSETS_PATH = "./ResourcePackAssets/";
        private const string TEXCONV_PATH = "./dependencies/texconv.exe";
        private FileManager fm = new FileManager();
        private List<Tuple<FileInfo, string, DxgiFormat>> conversionTasks = new List<Tuple<FileInfo, string, DxgiFormat>>();

        //private StringBuilder argSb = new StringBuilder();
        private StringBuilder convSb = new StringBuilder();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string arg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
        }

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();

            Fs = fm.GetObjectFromPath<FolderStructure>("./config.json");

            OnPropertyChanged(nameof(Fs));

            fm.CreateDirectory("./dependencies");
        }

        private void Button_Convert(object sender, RoutedEventArgs e)
        {
            ConvertPack();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            fm.SaveToJson(Fs, "./", "config", true);
        }

        private void ConvertPack()
        {
            //No texconv.exe
            FileInfo texConvFi = new FileInfo(TEXCONV_PATH);
            if (texConvFi.Exists == false)
            {
                MessageBox.Show("TexConv.exe not found.\nPlease add it to the dependencies folder.", "Missing TexConv.exe", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //No directory picked
            DirectoryInfo di = new DirectoryInfo(CurrentPackPath);
            if (di.Exists == false)
            {
                MessageBox.Show("Please enter the root directory of the pack first...", "No directory", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Duplicate the folder structure with a different root folder name
            string newPackDirectoryPath = di.FullName + "_Converted";
            fm.CreateDirectory(newPackDirectoryPath);
            DirectoryInfo newPackDi = new DirectoryInfo(newPackDirectoryPath);

            //List all the files which need to be converted (with their target format and path?)

            //Source file info, target path, target format
            conversionTasks.Clear();

            for (int i = 0; i < Fs.TexFolders.Length; i++)
            {
                TextureFolder tf = Fs.TexFolders[i];
                string targetPath = Path.Combine(newPackDi.FullName, tf.FolderPath);
                fm.CreateDirectory(targetPath);

                string path = Path.Combine(di.FullName, tf.FolderPath);

                int fileCount = fm.LoadFiles(path, "png", SearchOption.TopDirectoryOnly);
                for (int j = 0; j < fileCount; j++)
                {
                    //string newFilePath = Path.Combine(targetPath, fm.RemoveExtensionFromFileInfo(fm.fileInfos[j]) + ".dds");
                    conversionTasks.Add(new Tuple<FileInfo, string, DxgiFormat>(fm.fileInfos[j], targetPath, tf.Format));
                }
            }

            if (conversionTasks.Count <= 0)
            {
                MessageBox.Show("The selected directory doesn't contain any image...", "No image found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Multithreading conversion (1 thread by file)
            ConversionStart();

            convSb.Clear();

            //Multithreading is useless on the GPU AND my implementation of a index tracking doesn't work properly. 
            //Some Files will be processed multiple time. No big deal but I have to learn how to deal with that properly.
            activeThreadCount = 1;
            conversionThreads = new ConvertImageDelegate[activeThreadCount];
            for (int i = 0; i < activeThreadCount; i++)
            {
                //Start a new thread for the image processing and ocr stuff
                conversionThreads[i] = new ConvertImageDelegate(ConvertImages);
                conversionThreads[i].BeginInvoke(i, null, null);
            }
        }

        private ConvertImageDelegate[] conversionThreads;

        private int GetPackIndex(int threadIndex)
        {
            int index = processingFileCount;
            processingFileCount++;
            return index;
        }

        //Todo - handle missing dependencies
        private void ConvertImages(int threadIndex)
        {

            //Check if this thread is done and when they are all done tell the UI thread to reacitvate the UI elements.
            if (processingFileCount >= conversionTasks.Count)
            {
                activeThreadCount--;
                if (activeThreadCount <= 0)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConversionDoneDelegate(ConversionDone));
                }
                return;
            }

            var task = conversionTasks[GetPackIndex(threadIndex)];

            //Convert the image into the desired format
            string args = GetArguments(task);
            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = TEXCONV_PATH,
                    Arguments = args
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            //Write the report
            if (process.ExitCode == 0)
            {
                convSb.AppendLine(string.Concat("Conversion successful! (", DateTime.Now.Subtract(process.StartTime).TotalSeconds, ")", "/n", process.StartInfo.Arguments, "/n", output));
            }
            else
            {
                convSb.AppendLine(string.Concat("Conversion failed... (", DateTime.Now.Subtract(process.StartTime).TotalSeconds, ")", "/n", process.StartInfo.Arguments, "/n", output));
            }

            process.Dispose();

            //I can avoid method not closing pretty easily with delegate in Unity but I'm not sure how to do it in wpf / .net
            //Maybe it's not a big deal though... 🤔
            //ConvertImages();
            conversionThreads[threadIndex].BeginInvoke(threadIndex, null, null);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConversionProgressDelegate(ReportProgress));
        }

        private void ReportProgress()
        {
            progressBar.Value = (processingFileCount / (double)conversionTasks.Count) * 100.0;
        }

        //https://github.com/Microsoft/DirectXTex/wiki/Texconv
        private string GetArguments(Tuple<FileInfo, string, DxgiFormat> task)
        {
            StringBuilder argSb = new StringBuilder();

            argSb.Append(" -nologo");
            //argSb.Append(" -wrap");
            //argSb.Append(" -mirror");
            //argSb.Append(" -pmalpha"); //convert to premul alpha
            //argSb.Append(" -alpha"); //convert to straight alpha
            //argSb.Append(" -pow2"); //Fit texture to a pow of 2 for w and h
            argSb.Append(string.Format(" -ft dds -f {0}", task.Item3));
            argSb.Append(" -m 1"); //Mipmap level: 0 - all ; 1 - none ; 2 - 2 levels ; ... 
            argSb.Append(string.Format(" -o \"{0}\"", task.Item2));
            argSb.Append(" -bcmax"); //Max compression (use mode 0 & 2)
            //argSb.Append(" -bcquick"); //Min compression (just mode 6)
            argSb.Append(" -y"); //Overwrite existing file
            argSb.Append(" -gpu 1"); //0 is the IGP, 1 is the dedicated GPU but what if you have no IGP?
            argSb.Append(string.Concat(" \"", task.Item1.FullName, "\""));

            return argSb.ToString(); ;
        }

        private delegate void ConvertImageDelegate(int threadIndex);
        private delegate void ConversionDoneDelegate();
        private delegate void ConversionProgressDelegate();

        private int processingFileCount = 0;
        private int activeThreadCount = 0;

        private DateTime conversionStartTime;

        private void ConversionStart()
        {
            conversionStartTime = DateTime.Now;

            processingFileCount = 0;

            SwitchInterface(false);
        }

        private void ConversionDone()
        {
            SwitchInterface(true);

            convSb.AppendLine();
            convSb.AppendLine("Conversion time: " + DateTime.Now.Subtract(conversionStartTime).TotalSeconds.ToString("#") + " seconds");

            fm.SaveReportToFile(convSb, "./", "Conversion");

            SystemSounds.Exclamation.Play();
        }

        private void SwitchInterface(bool isEnable)
        {
            configList.IsEnabled = isEnable;
            buttonBrowse.IsEnabled = isEnable;
            buttonConvert.IsEnabled = isEnable;
            packFolderPath.IsEnabled = isEnable;
            buttonCreateResPack.IsEnabled = isEnable;
        }

        private void CreateResPack()
        {
            //No directory picked
            DirectoryInfo packDi = new DirectoryInfo(CurrentPackPath);
            if (packDi.Exists == false)
            {
                MessageBox.Show("Please enter the root directory of the pack first...", "No directory", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateResPackDoneDelegate(CreateResPackDone));
                return;
            }

            DirectoryInfo resPackAssetsDi = new DirectoryInfo(RESPACKASSETS_PATH);
            if (resPackAssetsDi.Exists == false)
            {
                MessageBox.Show("The resource pack assets folder is missing...\nPlace at least a manifest file in there.", "No Resource Pack assets", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateResPackDoneDelegate(CreateResPackDone));
                return;
            }

            string archivePath = "";
            using (TextReader tr = File.OpenText(resPackAssetsDi.FullName + "ArchiveName.txt"))
            {
                archivePath = "./" + tr.ReadLine();
            }

            string gameId = "";
            using (TextReader tr = File.OpenText(resPackAssetsDi.FullName + "GameId.txt"))
            {
                gameId = tr.ReadLine();
            }

            FileInfo manifestFi = new FileInfo(resPackAssetsDi.FullName + "manifest.json");
            if (manifestFi.Exists == false)
            {
                MessageBox.Show("The manifest file is missing...", "No manifest", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateResPackDoneDelegate(CreateResPackDone));
                return;
            }

            FileInfo logoFi = new FileInfo(resPackAssetsDi.FullName + "logo.png");


            //Create the textures folder and move the pack in there
            //https://gist.github.com/spycrab/9d05056755d8d7908bdb871a99d050bf
            DirectoryInfo texturesDi = fm.CreateDirectory("./Textures/");

            packDi.MoveTo(Path.Combine(texturesDi.FullName, gameId));

            //Create a zip archive as a Resource Pack

            //Brutal approach - no progress bar
            DirectoryInfo tempDi = fm.CreateDirectory("./Temp/");

            if (logoFi.Exists)
                logoFi = logoFi.CopyTo(tempDi.FullName + logoFi.Name);

            manifestFi = manifestFi.CopyTo(tempDi.FullName + manifestFi.Name);

            texturesDi.MoveTo(Path.Combine(tempDi.FullName, texturesDi.Name));

            ZipFile.CreateFromDirectory(tempDi.FullName, archivePath, CompressionLevel.NoCompression, false);

            //TODO - File by file approach - Allows displaying a Progress bar
            //using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Open))
            //{
            //    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            //    {
            //        archive.CreateEntryFromFile(manifestFi.FullName, manifestFi.Name, CompressionLevel.NoCompression);
            //        if (logoFi.Exists)
            //            archive.CreateEntryFromFile(logoFi.FullName, "test/" + logoFi.Name, CompressionLevel.NoCompression);
            //    }
            //}

            packDi = new DirectoryInfo(Path.Combine(tempDi.FullName, texturesDi.Name, packDi.Name));
            packDi.MoveTo(CurrentPackPath);

            tempDi.Delete(true);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateResPackDoneDelegate(CreateResPackDone));
        }

        private void CreateResPackDone()
        {
            SwitchInterface(true);
        }

        private delegate void CreateResPackDelegate();
        private delegate void CreateResPackDoneDelegate();

        private void Button_CreateResPack(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateResPackDelegate(CreateResPack));

            SwitchInterface(false);

            new CreateResPackDelegate(CreateResPack).BeginInvoke(null, null);
        }
    }
}
