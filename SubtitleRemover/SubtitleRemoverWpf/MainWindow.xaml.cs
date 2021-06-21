using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageMagick;
using Tesseract;

namespace SubtitleRemoverWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public BitmapImage ImagePreview { get; set; } =
            new BitmapImage(new Uri("pack://application:,,,/Resources/UI/AddImage.png"));

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public double MaskOversize { get; set; } = 2.0;
        public float ProcessingScale { get; set; } = 2.0f;
        public int FirstThreshold { get; set; } = 65;
        public int BlackThreshold { get; set; } = 55;
        public float GaussianBlur { get; set; } = 0.85f;

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(object prop)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(prop)));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SwitchInterface(bool isEnabled)
        {
            IsBusy = !isEnabled;
            FolderDropArea.AllowDrop = isEnabled;
            
            TextBoxMaskOversize.IsEnabled = isEnabled;
            TextBoxProcessingScale.IsEnabled = isEnabled;
            TextBoxFirstThreshold.IsEnabled = isEnabled;
            TextBoxBlackThreshold.IsEnabled = isEnabled;
            TextBoxGaussianBlur.IsEnabled = isEnabled;
        }

        private void FolderDropArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void UpdateInterfaceValues()
        {
            if (double.TryParse(TextBoxMaskOversize.Text, out var maskOversize))
            {
                MaskOversize = maskOversize;
            }
            else
            {
                TextBoxMaskOversize.Text = "2.00";
                MaskOversize = 2.0;
            }
            
            if (float.TryParse(TextBoxProcessingScale.Text, out var processingScale))
            {
                ProcessingScale = processingScale;
            }
            else
            {
                TextBoxProcessingScale.Text = "2.00";
                ProcessingScale = 2.0f;
            }
            
            if (int.TryParse(TextBoxFirstThreshold.Text, out var firstThreshold))
            {
                FirstThreshold = firstThreshold;
            }
            else
            {
                TextBoxFirstThreshold.Text = "65";
                FirstThreshold = 65;
            }
            
            if (int.TryParse(TextBoxBlackThreshold.Text, out var blackThreshold))
            {
                BlackThreshold = blackThreshold;
            }
            else
            {
                TextBoxBlackThreshold.Text = "55";
                BlackThreshold = 55;
            }
            
            if (float.TryParse(TextBoxGaussianBlur.Text, out var gaussianBlur))
            {
                GaussianBlur = gaussianBlur;
            }
            else
            {
                TextBoxGaussianBlur.Text = "0.85";
                GaussianBlur = 0.85f;
            }
        }
        
        private async void FolderDropArea_Drop(object sender, DragEventArgs e)
        {
            // Debug.WriteLine($"MASKOVERSIZE: {MaskOversize}");
            UpdateInterfaceValues();
            
            var fPaths = (string[]) e.Data.GetData(DataFormats.FileDrop);

            if (fPaths == null || fPaths.Length != 1)
            {
                MessageBox.Show(this, "Please drag exactly one folder.", "Not a folder", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            var inputPath = fPaths[0];
            var inputDi = new DirectoryInfo(inputPath);
            if (inputDi.Exists == false)
            {
                //TODO - error notification / msg with binding
                MessageBox.Show(this, "This is not a folder.", "Not a folder", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            var inputFiles = inputDi.GetFiles("*.png", SearchOption.TopDirectoryOnly);

            if (inputFiles.Length <= 0)
            {
                MessageBox.Show(this,
                    @"No png files in the input folder.", "No png files", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            TaskProgress(0f);

            SwitchInterface(false);

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(OperationCancelled);
            
            await Task.Run(()=>Run(inputDi, inputFiles), _cancellationTokenSource.Token);

            SwitchInterface(true);
        }

        private void OperationCancelled()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        private void TaskProgress(float percent)
        {
            Dispatcher.Invoke(() => { ProgressBar.Value = percent * 100f; });
        }

        private void TextBoxDouble_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void TextBoxInt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // private void TextBoxMaskOversize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        // {
        //     // Debug.WriteLine($"Preview: {e.Text}");
        //     
        //     var regex = new Regex("[^0-9.]+");
        //     e.Handled = regex.IsMatch(e.Text);
        // }

        // private void TextBoxMaskOversize_TextChanged(object sender, TextChangedEventArgs e)
        // {
        //     // Debug.WriteLine($"TextChanged: {MaskOversize}");
        //
        //     // MaskOversize = double.Parse(TextBoxMaskOversize.Text);
        // }
        //
        // private void TextBoxMaskOversize_OnLostFocus(object sender, RoutedEventArgs e)
        // {
        //     
        // }

        private void Cancel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsBusy;
        }

        private void Cancel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task Run(DirectoryInfo inputDi, FileInfo[] inputFiles)
        {
            //Create the output folder
            var outputDi = new DirectoryInfo(Path.Combine(inputDi.FullName, "Result"));
            if (!outputDi.Exists)
                outputDi.Create();

            var subtitleRegion = new Rectangle(140, 375, 360, 80);

            using var subtitleProcessor =
                new SubtitleProcessor(300, ProcessingScale, FirstThreshold, BlackThreshold, GaussianBlur);

            var maskColor = new MagickColor("#0F0F");

            var taskCountTotal = inputFiles.Length * 2;
            
            for (var i = 0; i < inputFiles.Length; i++)
            {
                var inputFile = inputFiles[i];

                if (!inputFile.Exists)
                    continue;

                using var mImage = new MagickImage(inputFile);

                var imageName = Path.GetFileNameWithoutExtension(inputFile.FullName);

                if (_cancellationTokenSource.IsCancellationRequested)
                    return;
                
                var boundingBoxes = await Task.Run(() =>
                    subtitleProcessor.GetSubtitleBoundingBoxes(mImage, subtitleRegion,
                        PageIteratorLevel.Block));

                var taskCount = i * 2 + 1;
                
                TaskProgress(taskCount / (float)taskCountTotal);

                var resultImagePath = Path.Combine(outputDi.FullName, $"{imageName}.png");

                if (boundingBoxes == null || boundingBoxes.Count <= 0)
                {
                    //Just copy
                    inputFile.CopyTo(resultImagePath, true);
                    mImage.Dispose();
                    continue;
                }

                if (_cancellationTokenSource.IsCancellationRequested)
                    return;

                await Task.Run(() => subtitleProcessor.DrawInPaintingMasks(mImage, boundingBoxes, maskColor, MaskOversize));

                TaskProgress((taskCount + 1) / (float)taskCountTotal);

                mImage.Write(resultImagePath);
                mImage.Dispose();
            }
            
            System.Media.SystemSounds.Exclamation.Play();
        }


    }
}