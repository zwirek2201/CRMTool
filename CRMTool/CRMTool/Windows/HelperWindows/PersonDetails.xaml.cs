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
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for PersonDetails.xaml
    /// </summary>
    public partial class PersonDetails : UserControl
    {
        private MainWindow _parent;
        public PersonModel Person { get; private set; }
        public CompanyModel Company { get; private set; }

        public List<PersonDetailListItem> EmailItems = new List<PersonDetailListItem>();
        public List<PersonDetailListItem> PhoneItems = new List<PersonDetailListItem>();

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;


        public PersonDetails(MainWindow parent, PersonModel person)
        {
            Person = person;
            _parent = parent;

            InitializeComponent();

            ChangeCompanyButton.Clicked += ChangeCompanyButton_Clicked;

            GenderComboBox.AddItem("Kobieta");
            GenderComboBox.AddItem("Mężczyzna");

            CompanyTextBox.IsEnabled = false;

            if (Person != null)
            {
                Company = person.Company;

                FirstNameTextBox.Text = person.FirstName;
                LastNameTextBox.Text = person.LastName;

                if (person.Company != null)
                    CompanyTextBox.Text = person.Company.Name;

                if (person.Gender == Gender.Female)
                    GenderComboBox.SelectedItem = GenderComboBox.Items[0];
                else
                    GenderComboBox.SelectedItem = GenderComboBox.Items[1];

                foreach (EmailAddressModel emailAddress in person.EmailAddresses)
                {
                    PersonDetailListItem detail = new PersonDetailListItem(emailAddress);
                    detail.RemoveDetail += Detail_RemoveDetail;
                    EmailItems.Add(detail);
                    EmailList.Children.Add(detail);
                }

                foreach (PhoneNumberModel phoneNumber in person.PhoneNumbers)
                {
                    PersonDetailListItem detail = new PersonDetailListItem(phoneNumber);
                    detail.RemoveDetail += Detail_RemoveDetail;
                    PhoneItems.Add(detail);
                    PhoneList.Children.Add(detail);
                }
            }

            if (EmailItems.Count == 0)
                NoEmailsLabel.Visibility = Visibility.Visible;

            if (PhoneItems.Count == 0)
                NoPhoneLabel.Visibility = Visibility.Visible;

            addEmailButton.Clicked += AddEmailButton_Clicked;
            addPhoneNumberButton.Clicked += AddPhoneNumberButton_Clicked;
            ReadyButton.Clicked += ReadyButton_Clicked;
            CancelButton.Clicked += CancelButton_Clicked;
            RemoveCompanyButton.Clicked += RemoveCompanyButton_Clicked;
        }

        private void RemoveCompanyButton_Clicked(object sender, EventArgs e)
        {
            CompanyTextBox.Text = "";
            Company = null;
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
                        PhoneItems.Remove(detail);
                        PhoneList.Children.Remove(detail);

                        if (PhoneItems.Count == 0)
                            NoPhoneLabel.Visibility = Visibility.Visible;
                        else
                            NoPhoneLabel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        EmailItems.Remove(detail);
                        EmailList.Children.Remove(detail);

                        if (EmailItems.Count == 0)
                            NoEmailsLabel.Visibility = Visibility.Visible;
                        else
                            NoEmailsLabel.Visibility = Visibility.Collapsed;
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
                else
                {
                    EmailItems.Remove(detail);
                    EmailList.Children.Remove(detail);

                    if (EmailItems.Count == 0)
                        NoEmailsLabel.Visibility = Visibility.Visible;
                    else
                        NoEmailsLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddPhoneNumberButton_Clicked(object sender, EventArgs e)
        {
            PersonDetailListItem detail = new PersonDetailListItem(new PhoneNumberModel("", "", "", true, true));
            detail.RemoveDetail += Detail_RemoveDetail;
            PhoneItems.Add(detail);
            PhoneList.Children.Add(detail);

            NoPhoneLabel.Visibility = Visibility.Collapsed;
        }

        private void AddEmailButton_Clicked(object sender, EventArgs e)
        {
            PersonDetailListItem detail = new PersonDetailListItem(new EmailAddressModel("", "", "", true, true));
            detail.RemoveDetail += Detail_RemoveDetail;
            EmailItems.Add(detail);
            EmailList.Children.Add(detail);

            NoEmailsLabel.Visibility = Visibility.Collapsed;
        }

        private void ReadyButton_Clicked(object sender, EventArgs e)
        {
            bool stop;

            stop = String.IsNullOrWhiteSpace(FirstNameTextBox.Text) || String.IsNullOrWhiteSpace(FirstNameTextBox.Text) || EmailItems.Any(obj => obj.Name == "" || obj.DetailValue == "") || PhoneItems.Any(obj => String.IsNullOrWhiteSpace(obj.Name) || String.IsNullOrWhiteSpace(obj.DetailValue));

            if (!stop)
            {
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorLabel.Text = "Uzupełnij wszystkie dane!";
            }
        }

        private void ChangeCompanyButton_Clicked(object sender, EventArgs e)
        {
            ChooseCompany choose = new ChooseCompany(_parent, new List<CompanyModel>(), false);

            choose.ReadyButtonClicked += (s, ea) =>
            {
                if (choose.SelectedCompanies.Count > 0)
                {
                    CompanyTextBox.Text = choose.SelectedCompanies.First().Name;
                    Company = choose.SelectedCompanies.First();
                }

                _parent.mainCanvas.Children.Remove(choose);
            };

            choose.CancelButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(choose);
            };

            _parent.mainCanvas.Children.Add(choose);
        }
    }
}