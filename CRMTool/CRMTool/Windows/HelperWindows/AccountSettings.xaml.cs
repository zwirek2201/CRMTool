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
    /// Interaction logic for AccountSettings.xaml
    /// </summary>
    public partial class AccountSettings : UserControl
    {
        private MainWindow _parent;
        public PersonModel Person { get; private set; }
        public CompanyModel Company { get; private set; }

        public List<PersonDetailListItem> EmailItems = new List<PersonDetailListItem>();
        public List<PersonDetailListItem> PhoneItems = new List<PersonDetailListItem>();

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;


        public AccountSettings(MainWindow parent, PersonModel person)
        {
            Person = person;
            _parent = parent;

            InitializeComponent();

            GenderComboBox.AddItem("Kobieta");
            GenderComboBox.AddItem("Mężczyzna");

            if (Person != null)
            {
                Company = person.Company;

                FirstNameTextBox.Text = person.FirstName;
                LastNameTextBox.Text = person.LastName;

                if (person.Gender == Gender.Female)
                    GenderComboBox.SelectedItem = GenderComboBox.Items[0];
                else
                    GenderComboBox.SelectedItem = GenderComboBox.Items[1];

                foreach (PhoneNumberModel phoneNumber in person.PhoneNumbers)
                {
                    PersonDetailListItem detail = new PersonDetailListItem(phoneNumber);
                    detail.RemoveDetail += Detail_RemoveDetail;
                    PhoneItems.Add(detail);
                    PhoneList.Children.Add(detail);
                }
            }

            if (PhoneItems.Count == 0)
                NoPhoneLabel.Visibility = Visibility.Visible;

            addPhoneNumberButton.Clicked += AddPhoneNumberButton_Clicked;
            ReadyButton.Clicked += ReadyButton_Clicked;
            CancelButton.Clicked += CancelButton_Clicked;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            CancelButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Detail_RemoveDetail(object sender, EventArgs e)
        {
            PersonDetailListItem detail = (PersonDetailListItem)sender;

            if (detail.Name != "" || detail.DetailValue != "")
            {
                string messageString = detail.ChildObject is EmailAddressModel
                    ? "Czy na pewno chcesz usunąć ten adres e-mail?"
                    : "Czy na pewno chcesz usunąć ten numer telefonu?";
                CustomMessageBox message = new CustomMessageBox(messageString, MessageBoxButton.YesNo);

                message.YesButtonClicked += (s, ea) =>
                {
                    if (detail.ChildObject is PhoneNumberModel)
                    {
                        PhoneNumberModel phoneNumber = (PhoneNumberModel) detail.ChildObject;
                        bool stop = false;
                        foreach (ConversationModel conversation in _parent.Conversations)
                        {
                            foreach (ConversationMessageModel conversationMessage in conversation.Messages)
                            {
                                if (conversationMessage is ConversationPhoneMessageModel)
                                {
                                    ConversationPhoneMessageModel phoneMessage = (ConversationPhoneMessageModel)conversationMessage;
                                    if (phoneMessage.AuthorPhoneNumber.Id == phoneNumber.Id ||
                                        phoneMessage.RecipientPhoneNumber.Id == phoneNumber.Id)
                                    {
                                        stop = true;
                                    }
                                } 
                            }
                        }

                            if(stop)
                        {
                            CustomMessageBox message2 =
                                new CustomMessageBox(
                                    "Nie można usunąć tego numeru telefonu ponieważ jest użyty w wiadomości",
                                    MessageBoxButton.OK);

                            message2.OKButtonClicked += (s2, ea2) =>
                            {
                                DarkenerPanel.Visibility = Visibility.Collapsed;
                                _parent.mainCanvas.Children.Remove(message2);
                            };

                            DarkenerPanel.Visibility = Visibility.Visible;
                            _parent.mainCanvas.Children.Add(message2);
                        }
                        else
                        {
                            PhoneItems.Remove(detail);
                            PhoneList.Children.Remove(detail);

                            if (PhoneItems.Count == 0)
                                NoPhoneLabel.Visibility = Visibility.Visible;
                            else
                                NoPhoneLabel.Visibility = Visibility.Collapsed;
                        }
                    }
                    DarkenerPanel.Visibility = Visibility.Collapsed;
                    _parent.mainCanvas.Children.Remove(message);
                };

                message.NoButtonClicked += (s, ea) =>
                {
                    DarkenerPanel.Visibility = Visibility.Collapsed;
                    _parent.mainCanvas.Children.Remove(message);
                };

                DarkenerPanel.Visibility = Visibility.Visible;
                _parent.mainCanvas.Children.Add(message);
            }
            else
            {
                if (detail.ChildObject is PhoneNumberModel)
                {
                    PhoneItems.Remove(detail);
                    PhoneList.Children.Remove(detail);

                    if (PhoneItems.Count == 0)
                        NoPhoneLabel.Visibility = Visibility.Visible;
                    else
                        NoPhoneLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddPhoneNumberButton_Clicked(object sender, EventArgs e)
        {
            PersonDetailListItem detail = new PersonDetailListItem(new PhoneNumberModel("", "", ""));
            detail.RemoveDetail += Detail_RemoveDetail;
            PhoneItems.Add(detail);
            PhoneList.Children.Add(detail);

            NoPhoneLabel.Visibility = Visibility.Collapsed;
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                String.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                EmailItems.Any(obj => obj.Name == "" || obj.DetailValue == "") ||
                PhoneItems.Any(obj => String.IsNullOrWhiteSpace(obj.Name) ||
                String.IsNullOrWhiteSpace(obj.DetailValue)))
            {
                ErrorLabel.Text = "Uzupełnij wszystkie dane!";
                return;
            }

            if (EmailItems.Any(obj => !StringHelper.IsValidEmail(obj.DetailValue)))
            {
                ErrorLabel.Text = "Adres e-mail ma niepoprawny format";
                return;
            }

            Regex phoneRegex = new Regex(@"^[0-9\s+()]+$");

            if (PhoneItems.Any(obj => !phoneRegex.IsMatch(obj.DetailValue)))
            {
                ErrorLabel.Text = "Numer telefonu ma niepoprawny format";
                return;
            }

            ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
