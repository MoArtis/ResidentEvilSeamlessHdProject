using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using Xceed.Wpf;

namespace BgInfoEditor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<EditableMask> EditableMasks { get; set; } = new ObservableCollection<EditableMask>();

        public ObservableCollection<BgFileInfo> BgFileInfos { get; set; } = new ObservableCollection<BgFileInfo>();

        private double imagesScaleRatio = 1.0;

        public static Random random = new Random();

        private static string bgInfosPath = "./BgInfos";
        private static string dumpsPath = "./Dumps";
        private static string amsPath = "./AltMaskSources";

        private FileManager fm = new FileManager();

        public CurrentBgInfo CurrentBgInfo { get; set; } = new CurrentBgInfo();

        public MainWindow()
        {
            DataContext = this;

            //Prepare Command bindings
            CommandBinding binding = new CommandBinding(CustomCommands.SelectBgInfo);
            binding.Executed += SelectBgInfo_Executed;
            binding.CanExecute += SelectBgInfo_CanExecute;
            CommandBindings.Add(binding);

            ListCompatibleBgInfos();

            if(BgFileInfos.Count <= 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No displayable BgInfo files found...", "", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            SelectBgInfo(0);

            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(RMask, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(OMask, BitmapScalingMode.NearestNeighbor);
        }

        private void SelectBgInfo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SelectBgInfo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BgInfoDropDown.IsOpen = false;
            SelectBgInfo((int)e.Parameter);
        }


        public void SelectBgInfo(int index)
        {
            BgInfo bgInfo = fm.GetObjectFromFileIndex<BgInfo>(index);
            CurrentBgInfo.BgInfo = bgInfo;

            //Load all the needed bitmaps
            CurrentBgInfo.OMaskImage = fm.GetBitmapFromPath(Path.Combine(dumpsPath, bgInfo.namePrefix + "_Mask"));

            CurrentBgInfo.BgFileInfo = BgFileInfos[index];
        }

        public void ListCompatibleBgInfos()
        {
            fm.CreateDirectory(bgInfosPath);
            fm.CreateDirectory(dumpsPath);
            fm.CreateDirectory(amsPath);

            int bgFileInfosCount = fm.LoadFiles(bgInfosPath, "json");

            BgFileInfos.Clear();

            //List only the Bg info which has a mask AND using an alt mask source.
            BgInfo[] bgFileInfoCandidates = fm.GetObjectsFromFiles<BgInfo>();
            for (int i = 0; i < bgFileInfosCount; i++)
            {
                BgInfo bgInfo = bgFileInfoCandidates[i];

                //It needs to be a BG with a mask AND using an alt mask source
                if (bgInfo.hasMask == false || bgInfo.useProcessedMaskTex == false)
                    continue;

                //It needs to have its BG image and BG AMS image present in dump/AMS folders
                if (fm.GetBitmapFromPath(Path.Combine(dumpsPath, bgInfo.namePrefix)) == null)
                    continue;

                if (fm.GetBitmapFromPath(Path.Combine(amsPath, bgInfo.namePrefix + "_altMaskSource")) == null)
                    continue;

                //I need to add an underscore due to some nmemonic thingy with wpf. If I don't the next _ will get escaped.
                BgFileInfos.Add(new BgFileInfo() { DisplayName = "_" + bgInfo.namePrefix, Fi = fm.fileInfos[i], Index = i });
            }
        }
    }
}
