using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Licencjat_new.Controls;
using Licencjat_new.Server;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        #region Variables
        private Client _client;
        #endregion

        #region Properties
        public string UserId { get; set; }
        #endregion

        #region Constructors
        public Login()
        {
            InitializeComponent();
            PreviewKeyUp += Login_PreviewKeyUp;
            btnLoginButton.Clicked += BtnLoginButton_Clicked;
            Loaded += Login_Loaded;
            versionLabel.Content = "v: " + Assembly.GetExecutingAssembly().GetName().Version;
        }

        #endregion

        #region Events
        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            txtLoginBox.FocusOnMe();
        }

        private void BtnLoginButton_Clicked(object sender, EventArgs e)
        {
            MainWindow parent = (MainWindow)Window.GetWindow(this);

            txtLoginBox.Enabled = false;
            txtPasswordBox.Enabled = false;

            if (parent != null)
            {
                parent.Client = new Client(parent);
                _client = parent.Client;
                _client.loginSucceeded += Client_loginSucceeded;
                _client.loginFailed += Client_loginFailed;
                _client.loginFinished += Client_loginFinished;
                _client.connectionFailed += Client_connectionFailed;
            }

            
            loadingOverlay.Visibility = Visibility.Visible;

            if (loadingOverlay != null)
            {
                DoubleAnimation anim = new DoubleAnimation(0, 0.8, TimeSpan.FromMilliseconds(100));

                loadingOverlay.BeginAnimation(OpacityProperty, anim);   
            }

            ErrorLabel.Content = "";

            _client.Connect(txtLoginBox.Text, txtPasswordBox.Text);
        }

        private void Login_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnLoginButton.PerformClick();
        }

        private void Client_connectionFailed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ErrorLabel.Content = "Nie można połączyć się z serwerem. Spróbuj później.";

                txtLoginBox.Enabled = true;
                txtPasswordBox.Enabled = true;

                if (loadingOverlay != null)
                {
                    DoubleAnimation anim = new DoubleAnimation(0.8, 0, TimeSpan.FromSeconds(0.2));

                    anim.Completed += (s, ev) => { loadingOverlay.Visibility = Visibility.Collapsed; };
                    loadingOverlay.BeginAnimation(OpacityProperty, anim);
                }

            });
        }

        private void Client_loginFinished(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                txtLoginBox.Enabled = true;
                txtPasswordBox.Enabled = true;

                if (loadingOverlay != null)
                {
                    DoubleAnimation anim = new DoubleAnimation(0.8, 0, TimeSpan.FromSeconds(0.2));

                    anim.Completed += (s, ev) => { loadingOverlay.Visibility = Visibility.Collapsed; };
                    loadingOverlay.BeginAnimation(OpacityProperty, anim);
                }

                _client.loginSucceeded -= Client_loginSucceeded;
                _client.loginFailed -= Client_loginFailed;
                _client.loginFinished -= Client_loginFinished;
                _client.connectionFailed -= Client_connectionFailed;
            });
        }

        private void Client_loginFailed(object sender, LoginFailedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {               
                ErrorLabel.Content = e.ErrorMessage;
            });
        }

        private void Client_loginSucceeded(object sender, LoginSuccedeedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                MainWindow parent = (MainWindow) Window.GetWindow(this);

                parent.NotificationClient = new NotificationClient(e.UserId, parent);
                parent.UploadClient = new UploadClient(e.UserId, parent);
                parent.DownloadClient = new DownloadClient(e.UserId, parent);

                LoginStatusChangedEventArgs loginStatus = new LoginStatusChangedEventArgs()
                {
                    LoggedIn = true,
                    UserId = e.UserId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                };

                parent?.OnLoginStatusChanged(loginStatus);
            });
        }
        #endregion

    }
}
