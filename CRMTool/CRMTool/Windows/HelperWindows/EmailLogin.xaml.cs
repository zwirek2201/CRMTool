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
using ImapX;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for EmailLogin.xaml
    /// </summary>
    public partial class EmailLogin : UserControl
    {
        private EmailModel _email;

        private bool _loadingOn = false;

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public string Login { get; set; }
        public string Password { get; set; }

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

        public EmailLogin(EmailModel email)
        {
            _email = email;
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
            LoginText.FocusOnMe();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            CancelButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginText.Text) || string.IsNullOrWhiteSpace(PasswordText.Text))
            {
                ErrorLabel.Content = "Uzupełnij dane";
                return;
            }

            ImapClient client = new ImapClient(_email.ImapHost, _email.ImapPort, _email.ImapUseSsl);

            if (!EmailHelper.ConnectToServer(client))
            {
                ErrorLabel.Content = "Nie można nawiązać połączenia z serwerem";
                return;
            }

            if (!EmailHelper.AuthenticateClient(client, LoginText.Text, PasswordText.Text))
            {
                ErrorLabel.Content = "Błędny login lub hasło";
                return;
            }

            Login = LoginText.Text;
            Password = PasswordText.Text;
            ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
