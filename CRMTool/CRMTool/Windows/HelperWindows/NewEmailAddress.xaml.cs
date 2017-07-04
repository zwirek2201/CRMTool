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
using System.Windows.Shapes;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for NewEmailAddress.xaml
    /// </summary>
    public partial class NewEmailAddress : UserControl
    {
        public event EventHandler<NewEmailAddressEventArgs> ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public EmailAddressModel Email
        {
            get;
            private set;
        }

        public NewEmailAddress()
        {
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
            if (
                !String.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                !String.IsNullOrWhiteSpace(NameTextBox.Text) &&
                !String.IsNullOrWhiteSpace(LoginTextBlock.Text) &&
                (
                NoPasswordSaveCheckBox.Selected ||
                (!NoPasswordSaveCheckBox.Selected && !String.IsNullOrWhiteSpace(PasswordTextBlock.Text))
                ) &&
                !String.IsNullOrWhiteSpace(ImapHostTextBlock.Text) &&
                !String.IsNullOrWhiteSpace(ImapPortTextBlock.Text) &&
                !String.IsNullOrWhiteSpace(SmtpHostTextBlock.Text) &&
                !String.IsNullOrWhiteSpace(SmtpPortTextBlock.Text)
                )
            {
                ErrorTextBlock.Text = "";

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
            else
            {
                ErrorTextBlock.Text = "Uzupełnij dane";
            }
        }
    }

    public class NewEmailAddressEventArgs : EventArgs
    {
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
