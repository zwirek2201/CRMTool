using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImapX;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for NewEmailAddress.xaml
    /// </summary>
    public partial class NewEmailAddress : UserControl
    {
        private MainWindow _parent;

        public event EventHandler<NewEmailAddressEventArgs> ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public EmailAddressModel Email
        {
            get;
            private set;
        }

        public NewEmailAddress(MainWindow parent)
        {
            _parent = parent;
            InitializeComponent();

            ReadyButton.Clicked += ReadyButton_Clicked;
            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            NoPasswordSaveCheckBox.SelectedChanged += (s, ea) =>
            {
                LoginTextBlock.Visibility = NoPasswordSaveCheckBox.Selected ? Visibility.Collapsed : Visibility.Visible;
                PasswordTextBlock.Visibility = NoPasswordSaveCheckBox.Selected ? Visibility.Collapsed : Visibility.Visible;
            };
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            BackgroundWorker checkWorker = new BackgroundWorker();

            DoWorkEventHandler doWorkEvent = null;
            doWorkEvent = (s, ea) =>
            {
                try
                {
                    string address = "";
                    string name = "";
                    string login = "";
                    string password = "";
                    bool noPassword = false;
                    string imapHost = "";
                    string imapPort = "";
                    bool imapUseSsl = false;
                    string smtpHost = "";
                    string smtpPort = "";
                    bool smtpUseSsl = false;

                    Dispatcher.Invoke(() =>
                    {
                        loadingOverlay.Visibility = Visibility.Visible;
                        address = AddressTextBox.Text;
                        name = NameTextBox.Text;
                        login = LoginTextBlock.Text;
                        password = PasswordTextBlock.Text;
                        noPassword = NoPasswordSaveCheckBox.Selected;
                        imapHost = ImapHostTextBlock.Text;
                        imapPort = ImapPortTextBlock.Text;
                        imapUseSsl = ImapSslCheckBox.Selected;
                        smtpHost = SmtpHostTextBlock.Text;
                        smtpPort = SmtpPortTextBlock.Text;
                        smtpUseSsl = SmtpSslCheckBox.Selected;
                    });

                    if (
                        String.IsNullOrWhiteSpace(address) ||
                        (!noPassword &&
                         (String.IsNullOrWhiteSpace(login) ||
                          String.IsNullOrWhiteSpace(password))) ||
                        String.IsNullOrWhiteSpace(imapHost) ||
                        String.IsNullOrWhiteSpace(imapPort) ||
                        String.IsNullOrWhiteSpace(smtpHost) ||
                        String.IsNullOrWhiteSpace(smtpPort)
                        )
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingOverlay.Visibility = Visibility.Collapsed;
                            ErrorTextBlock.Text = "Uzupełnij dane";
                        });
                        ea.Result = false;
                        return;
                    }

                    Regex portRegex = new Regex("^[0-9]+$");

                    if (!portRegex.IsMatch(imapPort) ||
                        !portRegex.IsMatch(smtpPort) ||
                        !StringHelper.IsValidEmail(address))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingOverlay.Visibility = Visibility.Collapsed;
                            ErrorTextBlock.Text = "Dane nie mają poprawnego formatu";
                        });
                        ea.Result = false;
                        return;
                    }

                    ImapClient client;
                    client = EmailHelper.ConnectToServer(imapHost, Convert.ToInt32(imapPort),
                        imapUseSsl);

                    if (client == null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingOverlay.Visibility = Visibility.Collapsed;
                            ErrorTextBlock.Text = "Nie można połączyć się z serwerem poczty przychodzącej";
                        });
                        ea.Result = false;
                        return;
                    }

                    if (!SmtpHelper.TestConnection(smtpHost, Convert.ToInt32(smtpPort)))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingOverlay.Visibility = Visibility.Collapsed;
                            ErrorTextBlock.Text = "Nie można połączyć się z serwerem poczty wychodzącej";
                        });
                        ea.Result = false;
                        return;
                    }

                    if (!NoPasswordSaveCheckBox.Selected)
                    {
                        if (!EmailHelper.AuthenticateClient(client, login, password))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                loadingOverlay.Visibility = Visibility.Collapsed;
                                ErrorTextBlock.Text = "Login lub hasło są niepoprawne";
                            });
                            ea.Result = false;
                            return;
                        }
                        ea.Result = true;
                    }
                    else
                        ea.Result = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };

            RunWorkerCompletedEventHandler completed = null;
            completed = (s, ea) =>
            {
                loadingOverlay.Visibility = Visibility.Collapsed;

                if (Convert.ToBoolean(ea.Result))
                {
                    NewEmailAddressEventArgs eventArgs = new NewEmailAddressEventArgs()
                    {
                        Address = AddressTextBox.Text,
                        Login = LoginTextBlock.Text,
                        UseLoginPassword = !NoPasswordSaveCheckBox.Selected,
                        Password = PasswordTextBlock.Text,
                        ImapHost = ImapHostTextBlock.Text,
                        ImapPort = Convert.ToInt32(ImapPortTextBlock.Text),
                        ImapUseSsl = ImapSslCheckBox.Selected,
                        SmtpHost = SmtpHostTextBlock.Text,
                        SmtpPort = Convert.ToInt32(SmtpPortTextBlock.Text),
                        SmtpUseSsl = SmtpSslCheckBox.Selected,
                        Name = NameTextBox.Text
                    };

                    ReadyButtonClicked?.Invoke(this, eventArgs);
                }

                checkWorker.DoWork -= doWorkEvent;
                checkWorker.RunWorkerCompleted -= completed;
            };

            ErrorTextBlock.Text = "";

            if (_parent.EmailClients != null)
            {
                if (_parent.EmailClients.Any(obj => obj.Address == AddressTextBox.Text))
                {
                    ErrorTextBlock.Text = "Adres e-mail już istnieje";
                }
                else
                {
                    checkWorker.DoWork += doWorkEvent;
                    checkWorker.RunWorkerCompleted += completed;
                    checkWorker.RunWorkerAsync();
                }
            }
        }
    }

    public class NewEmailAddressEventArgs : EventArgs
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool UseLoginPassword { get; set; }
        public string ImapHost { get; set; }
        public int ImapPort { get; set; }
        public bool ImapUseSsl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string Name { get; set; }
    }
}
