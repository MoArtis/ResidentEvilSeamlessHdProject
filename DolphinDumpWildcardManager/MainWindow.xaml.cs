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

namespace RESHDP_WildcardManager
{
    public enum HashType
    {
        Texture,
        TLUT
    }

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public HashType CurrentHashType { get; set; }
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
            GenerateWildcardedPackDelegate generateWildcardedPackDelegate = new GenerateWildcardedPackDelegate(GenerateWildcardedPack);
            generateWildcardedPackDelegate.BeginInvoke(null, null);

        }

        private delegate void GenerateWildcardedPackDelegate();

        private void GenerateWildcardedPack()
        {
            int fileCount = fm.LoadFiles(CurrentPackPath, "png", SearchOption.AllDirectories);

            int uniqueHashIndex = 0;
            int wildcardedHashIndex = 0;
            switch (CurrentHashType)
            {
                case HashType.Texture:
                    uniqueHashIndex = 3;
                    wildcardedHashIndex = 2;
                    break;

                case HashType.TLUT:
                    uniqueHashIndex = 2;
                    wildcardedHashIndex = 3;
                    break;
            }

            textures.Clear();

            for (int i = 0; i < fileCount; i++)
            {
                string hash = fm.fileInfos[i].Name.Split(new char[] { '_' }, StringSplitOptions.None)[uniqueHashIndex];

                if (textures.ContainsKey(hash) == false)
                {
                    textures.Add(hash, new List<FileInfo>());
                }

                textures[hash].Add(fm.fileInfos[i]);
            }

            //Create a new directory for the wildcarded textures
            DirectoryInfo di = new DirectoryInfo(CurrentPackPath);
            di = di.Parent;
            string wildcardedPath = Path.Combine(di.FullName, "Wildcarded");

            Trace.WriteLine(wildcardedPath);

            if (Directory.Exists(wildcardedPath))
            {
                MessageBox.Show("Please delete the existing \"Wildcarded\" folder and try again.", "Existing Wildcarded Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GenerationDoneDelegate(GenerationDone));
                return;
            }

            DirectoryInfo wildcardedDirInfo = fm.CreateDirectory(wildcardedPath);

            StringBuilder nameSb = new StringBuilder();

            //Copy the files and make sure they are named with a wildcard
            List<FileInfo>[] wildCardCandidates = textures.Values.ToArray();
            for (int i = 0; i < wildCardCandidates.Length; i++)
            {
                FileInfo fi = wildCardCandidates[i][0];

                //If the IgnoreUnique option is selected, copy the file but do not change its name.
                string filePath = "";
                string subDirPath = "";

                subDirPath = fi.FullName.Remove(0, CurrentPackPath.Length);
                subDirPath = subDirPath.Remove(subDirPath.Length - fi.Name.Length, fi.Name.Length);
                subDirPath = subDirPath.Remove(0, 1);

                subDirPath = Path.Combine(wildcardedPath, subDirPath);

                if (IgnoreUniques && wildCardCandidates[i].Count == 1)
                {
                    filePath = Path.Combine(subDirPath, fi.Name);
                }
                else
                {
                    string[] nameParts = fi.Name.Split(new char[] { '_' }, StringSplitOptions.None);
                    nameParts[wildcardedHashIndex] = "$";

                    nameSb.Clear();
                    nameSb.Append(nameParts[0]);
                    for (int j = 1; j < nameParts.Length; j++)
                    {
                        nameSb.Append("_");
                        nameSb.Append(nameParts[j]);
                    }

                    filePath = Path.Combine(subDirPath, nameSb.ToString());

                    Trace.WriteLine(filePath);
                }

                //Check if the file already exists
                if (File.Exists(filePath))
                {
                    MessageBox.Show(textures.Keys.ToArray()[i] + " - " + fi.Name + " - " + nameSb.ToString(), "Duplicate detected!", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

                fm.CreateDirectory(subDirPath);

                if (MarkTexture)
                {
                    MagickImage magickImage = new MagickImage(fi);
                    //magickImage.Negate(Channels.RGB);
                    magickImage.Colorize(new MagickColor(0, 65535, 0, 0), new Percentage(25.0));

                    Bitmap bitmap = magickImage.ToBitmap();
                    bitmap.Save(filePath);

                    magickImage.Dispose();
                    bitmap.Dispose();
                }
                else
                {
                    fi.CopyTo(filePath, false);
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GenerationProgressDelegate(() => GenerationProgress(i, wildCardCandidates.Length)));
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GenerationDoneDelegate(GenerationDone));
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

        private delegate void GenerationDoneDelegate();

        private void GenerationDone()
        {
            SwitchInterface(true);

            SystemSounds.Exclamation.Play();
        }

        private void SwitchInterface(bool isEnable)
        {
            buttonBrowse.IsEnabled = isEnable;
            buttonGenerate.IsEnabled = isEnable;
            packFolderPath.IsEnabled = isEnable;
            dropDownHashType.IsEnabled = isEnable;
            checkBoxIgnoreUniques.IsEnabled = isEnable;
            checkBoxMarkTextures.IsEnabled = isEnable;
        }

        private delegate void GenerationProgressDelegate();

        private void GenerationProgress(int fileIndex, int filesCount)
        {
            progressBar.Value = (fileIndex / (double)filesCount) * 100.0;
        }
    }
}
