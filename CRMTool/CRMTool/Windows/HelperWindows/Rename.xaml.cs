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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for Rename.xaml
    /// </summary>
    public partial class Rename : UserControl
    {
        private bool _loadingOn = false;

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public string NewName { get; set; }

        public bool LoadingOn
        {
            get { return _loadingOn; }
            set
            {
                _loadingOn = value;
                if (LoadingOn)
                {
                    loadingOverlay.Visibility = Visibility.Visible;

                    DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    loadingOverlay.BeginAnimation(OpacityProperty, fadeInAnimation);
                }
                else
                {
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    fadeOutAnimation.Completed += (s, e) => { loadingOverlay.Visibility = Visibility.Collapsed; };
                    loadingOverlay.BeginAnimation(OpacityProperty, fadeOutAnimation);
                }
            }
        }

        public Rename()
        {
            InitializeComponent();

            ReadyButton.Clicked += ReadyButton_Clicked;
            CancelButton.Clicked += CancelButton_Clicked;

            Loaded += Rename_Loaded;
            KeyUp += Rename_KeyUp;
        }

        private void Rename_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ReadyButton.PerformClick();
        }

        private void Rename_Loaded(object sender, RoutedEventArgs e)
        {
            NameText.FocusOnMe();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            CancelButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameText.Text))
            {
                NewName = NameText.Text;
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorLabel.Content = "Nazwa nie może być pusta";
            }
        }
    }
}
