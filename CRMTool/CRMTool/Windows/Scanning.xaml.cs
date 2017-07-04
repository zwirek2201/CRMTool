using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageMagick;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Scanning.xaml
    /// </summary>
    public partial class Scanning : UserControl
    {
        #region Variables
        private ScanningMode _scanMode = ScanningMode.Manual;
        private int _displayedPage = 0;
        private bool _saving = false;

        #region Lists
        private List<System.Drawing.Image> _scannedImages = new List<System.Drawing.Image>();
        #endregion

            #region BackgroundWorkers
        private BackgroundWorker _scanWorker;
        private BackgroundWorker _saveWorker;
        #endregion
        #endregion

        #region Properties
        private int DisplayedPage
        {
            get { return _displayedPage; }
            set
            {
                _displayedPage = value;
                this.Dispatcher.Invoke(() =>
                {
                    if (_scannedImages.Count > 0)
                    {
                        lblPageCount.Visibility = Visibility.Visible;
                        lblPageCount.Content = "Strona " + (_displayedPage + 1) + " z " + _scannedImages.Count;
                        scanImage.Source = ConvertImageToImageSource(_scannedImages[_displayedPage]);
                    }
                    else
                    {
                        lblPageCount.Visibility = Visibility.Collapsed;
                        scanImage.Source = null;
                    }
                });
            }
        }
        #endregion

        #region Constructors
        public Scanning()
        {
            InitializeComponent();

            ScanningWindows.ScanningManual scan = new ScanningWindows.ScanningManual();
            scan.btnManualScan.Click += BtnManualScan_Click;
            ContentArea.Content = scan;

            _scanWorker = new BackgroundWorker();
            _scanWorker.WorkerSupportsCancellation = true;
            _scanWorker.WorkerReportsProgress = true;
            _scanWorker.DoWork += _scanWorker_DoWork;
            _scanWorker.RunWorkerCompleted += _scanWorker_RunWorkerCompleted;

            _saveWorker = new BackgroundWorker();
            _saveWorker.DoWork += _saveWorker_DoWork;
            _saveWorker.RunWorkerCompleted += _saveWorker_RunWorkerCompleted;

            ReadyButton.Clicked += ReadyButton_Clicked;
            //btnCancelScanning.Click += BtnCancelScanning_Click;
        }
        #endregion

        #region Events

            #region SaveWorker
        private void _saveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<System.Drawing.Image> images = (List<System.Drawing.Image>)e.Argument;
                using (MagickImageCollection collection = new MagickImageCollection())
                {
                    _saving = true;
                    foreach (System.Drawing.Image image in images)
                    {
                        MemoryStream stream = new MemoryStream();
                        image.Save(stream, ImageFormat.Jpeg);
                        stream.Position = 0;
                        collection.Add(new MagickImage(stream));
                    }
                    MemoryStream stream2 = new MemoryStream();
                    collection.Write(stream2, MagickFormat.Pdf);

                    string fileName = "";

                    Dispatcher.Invoke(() =>
                    {
                        fileName = txtFileName.Text + ".pdf";
                    });

                    _saving = false;
                    
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void _saveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStatus.Content = "Zapisano...";
        }

        #endregion

            #region ScanWorker
        private void _scanWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (_scanMode)
            {
                case ScanningMode.Manual:
                    System.Drawing.Image image = ScannerHelper.Scan();
                    _scannedImages.Add(image);
                    DisplayedPage = _scannedImages.Count - 1;
                    e.Result = image;
                    break;
                case ScanningMode.Automatic:
                    BackgroundWorker back = (BackgroundWorker)sender;
                    while (!back.CancellationPending)
                    {
                        back.ReportProgress(0);
                        this.Dispatcher.Invoke(() =>
                        {
                            ScanningWindows.ScanningAutomatic scan = (ScanningWindows.ScanningAutomatic)ContentArea.Content;
                            loadingOverlay.Visibility = Visibility.Visible;
                            scan.btnStopScanning.Visibility = Visibility.Collapsed;
                            ReadyButton.Visibility = Visibility.Collapsed;
                        });
                        System.Drawing.Image image2 = ScannerHelper.Scan();
                        _scannedImages.Add(image2);
                        DisplayedPage = _scannedImages.Count - 1;
                        this.Dispatcher.Invoke(() =>
                        {
                            ScanningWindows.ScanningAutomatic scan = (ScanningWindows.ScanningAutomatic)ContentArea.Content;
                            loadingOverlay.Visibility = Visibility.Collapsed;
                            scan.btnStopScanning.Visibility = Visibility.Visible;
                        });
                        for (int i = 1; i <= Convert.ToInt32(e.Argument) * 10; i++)
                        {
                            if (!back.CancellationPending)
                            {
                                System.Threading.Thread.Sleep(100);
                                back.ReportProgress(i);
                            }
                        }
                    }
                    break;
            }
        }
        private void _scanWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ScanningWindows.ScanningAutomatic scan = (ScanningWindows.ScanningAutomatic)ContentArea.Content;
            double progress = (Convert.ToDouble(e.ProgressPercentage) / Convert.ToDouble(scan.txtDelay.Text)) * 10;
            scan.prProgress.Value = progress;
        }
        private void _scanWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch (_scanMode)
            {
                case ScanningMode.Manual:
                    System.Drawing.Image image = (System.Drawing.Image)e.Result;
                    scanImage.Source = ConvertImageToImageSource(image);
                    loadingOverlay.Visibility = Visibility.Hidden;
                    ReadyButton.Visibility = Visibility.Visible;
                    break;
                case ScanningMode.Automatic:
                    ScanningWindows.ScanningAutomatic scan = (ScanningWindows.ScanningAutomatic)ContentArea.Content;
                    scan.prProgress.Visibility = Visibility.Collapsed;
                    scan.btnAutoScan.Visibility = Visibility.Visible;
                    scan.btnStopScanning.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        #endregion

            #region ButtonClicks
        private void btnScanMode_Click(object sender, RoutedEventArgs e)
        {
            Button send = (Button)sender;

            foreach (object modeButton in modeMenu.Children)
            {
                Button button = (Button)modeButton;
                if (button != send)
                {
                    button.Background = new SolidColorBrush(ColorScheme.MenuDarker);
                    button.Foreground = new SolidColorBrush(Colors.Black);
                    button.FontWeight = FontWeights.Normal;
                }
            }

            send.Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            send.Foreground = new SolidColorBrush(Colors.White);
            send.FontWeight = FontWeights.Medium;

            switch (send.Name)
            {
                case "btnScanModeManual":
                    _scanMode = ScanningMode.Manual;
                    ScanningWindows.ScanningManual scanManual = new ScanningWindows.ScanningManual();
                    scanManual.btnManualScan.Click += BtnManualScan_Click;
                    ContentArea.Content = scanManual;
                    break;
                case "btnScanModeAutomatic":
                    _scanMode = ScanningMode.Automatic;
                    ScanningWindows.ScanningAutomatic scanAuto = new ScanningWindows.ScanningAutomatic();
                    scanAuto.btnAutoScan.Click += BtnAutoScan_Click;
                    ContentArea.Content = scanAuto;
                    break;
            }
        }

        private void BtnAutoScan_Click(object sender, RoutedEventArgs e)
        {
            ScanningWindows.ScanningAutomatic scan = (ScanningWindows.ScanningAutomatic) ContentArea.Content;
            scan.prProgress.Minimum = 0;
            scan.prProgress.Maximum = 100;
            scan.prProgress.Visibility = Visibility.Visible;
            _scanWorker.ProgressChanged += _scanWorker_ProgressChanged;
            scan.btnAutoScan.Visibility = Visibility.Collapsed;
            scan.btnStopScanning.Click += BtnStopScanning_Click;
            _scanWorker.RunWorkerAsync(Convert.ToInt32(scan.txtDelay.Text));
        }

        private void BtnStopScanning_Click(object sender, RoutedEventArgs e)
        {
            _scanWorker.CancelAsync();
            ReadyButton.Visibility = Visibility.Visible;
        }

        private void BtnManualScan_Click(object sender, RoutedEventArgs e)
        {
            loadingOverlay.Visibility = Visibility.Visible;
            _scanWorker.RunWorkerAsync();
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            Timer time = new Timer();
            time.Interval = 2000;
            time.Elapsed += Time_Elapsed;
            time.AutoReset = true;


            if (_saving)
            {
                lblErrorMessage.Content = "Trwa zapisywanie innego pliku!";
                errorPanel.Visibility = Visibility.Visible;
                time.Start();
                return;
            }

            if (_scannedImages.Count == 0)
            {
                lblErrorMessage.Content = "Brak stron do zapisania!";
                errorPanel.Visibility = Visibility.Visible;
                time.Start();
                return;
            }

            if (txtFileName.Text == "")
            {
                txtFileName.Background = new SolidColorBrush(ColorScheme.GlobalLightRed);
                txtFileName.BorderBrush = new SolidColorBrush(ColorScheme.GlobalDarkRed);
                txtFileName.BorderThickness = new Thickness(1);
                lblErrorMessage.Content = "Dodaj nazwę dokumentu!";
                errorPanel.Visibility = Visibility.Visible;
                time.Start();
                return;
            }

            lblStatus.Content = "Zapisywanie...";
            scanImage.Source = null;
            DisplayedPage = 0;

            List<System.Drawing.Image> images = new List<System.Drawing.Image>();

            foreach (System.Drawing.Image image in _scannedImages)
                images.Add((System.Drawing.Image) image.Clone());

            _scannedImages.Clear();

            _saveWorker.RunWorkerAsync(images);
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedPage < _scannedImages.Count - 1)
            {
                DisplayedPage++;
            }
        }

        private void btnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedPage > 0)
            {
                DisplayedPage--;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_scannedImages.Count <= 0 || _displayedPage <= 0)
                return;

            System.Drawing.Image swapImage = _scannedImages[_displayedPage];
            _scannedImages.Remove(_scannedImages[_displayedPage]);
            _scannedImages.Insert(_displayedPage - 1, swapImage);
            DisplayedPage --;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_scannedImages.Count == 0)
                return;

            _scannedImages[_displayedPage].RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            scanImage.Source = ConvertImageToImageSource(_scannedImages[_displayedPage]);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (_scannedImages.Count == 0 || _displayedPage >= _scannedImages.Count - 1)
                return;

            System.Drawing.Image swapImage = _scannedImages[_displayedPage];
            _scannedImages.Remove(_scannedImages[_displayedPage]);
            _scannedImages.Insert(_displayedPage + 1, swapImage);
            DisplayedPage++;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (_scannedImages.Count == 0)
                return;

            _scannedImages.Remove(_scannedImages[_displayedPage]);
            if (_displayedPage > 0)
            {
                DisplayedPage --;
            }
            else
            {
                lblPageCount.Visibility = Visibility.Collapsed;
                scanImage.Source = null;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_scannedImages.Any())
            {
                MessageBoxResult result = MessageBox.Show("Plik nie został zapisany. Czy na pewno chcesz wyjść?",
                    "Zapisywanie pliku", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Canvas parent = (Canvas) Parent;
                    parent.Children.Remove(this);
                }
            }
            else
            {
                Canvas parent = (Canvas)Parent;
                parent.Children.Remove(this);
            }
        }
        #endregion

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = (Timer)sender;
            Dispatcher.Invoke(() =>
            {
                errorPanel.Visibility = Visibility.Collapsed;
            });
            timer.Stop();
        }

        private void txtFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtFileName.Background = new SolidColorBrush(ColorScheme.GlobalWhite);
            txtFileName.BorderThickness = new Thickness(0);
        }

        private void imageCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            lowerImageToolBar.Opacity = 0.8;
        }

        private void imageCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            lowerImageToolBar.Opacity = 0.2;
        }
        #endregion

        #region Methods
        private BitmapImage ConvertImageToImageSource(System.Drawing.Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.EndInit();

            return bmp;
        }
        #endregion
    }

    internal enum ScanningMode
    {
        Manual,
        Automatic
    }

}
