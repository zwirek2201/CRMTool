using System;
using System.Collections.Generic;
using System.Linq;
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
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for NewInternalUser.xaml
    /// </summary>
    public partial class NewInternalUser : UserControl
    {
        private MainWindow _parent;

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;


        public NewInternalUser(MainWindow parent)
        {
            _parent = parent;

            InitializeComponent();

            GenderComboBox.AddItem("Kobieta");
            GenderComboBox.AddItem("Mężczyzna");

            ReadyButton.Clicked += ReadyButton_Clicked;
            CancelButton.Clicked += CancelButton_Clicked;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            CancelButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                String.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                String.IsNullOrWhiteSpace(PasswordTextBox.Text) ||
                String.IsNullOrWhiteSpace(Password2TextBox.Text) ||
                GenderComboBox.SelectedItem == null)
            {
                ErrorLabel.Text = "Uzupełnij wszystkie dane!";
                return;
            }

            if (PasswordTextBox.Text != Password2TextBox.Text)
            {
                ErrorLabel.Text = "Podane hasła nie są takie same";
                return;
            }

            if (_parent.Client.CheckLoginExists(CryptographyHelper.HashString(LoginTextBox.Text,0)))
            {
                ErrorLabel.Text = "Podany login już istnieje";
                return;
            }

            ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}