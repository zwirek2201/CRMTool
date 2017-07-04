using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Licencjat_new.Windows.HelperWindows;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Contact.xaml
    /// </summary>
    public partial class Contact : UserControl
    {
        #region Variables

            #region Other
        private MainWindow _parent;
        private List<String> _alphabetUsed;
        #endregion

            #region Lists
        private List<PersonModel> _persons;
        private List<CompanyModel> _companies;

        private ContactList _contactList;

        #endregion
        #endregion

        #region Constructors
        public Contact()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization
        public bool WindowInitialized { get; private set; }

        public void Init()
        {
            _parent = (MainWindow)Window.GetWindow(this);

            ContactSearchBox.SearchPhraseChanged += ContactSearchBox_SearchPhraseChanged;

            if (_parent.ContactWorker.IsBusy)
            {
                _parent.ContactWorker.RunWorkerCompleted += _contactWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent.Persons != null)
                {
                    _persons = _parent.Persons;
                }

                if (_parent.Companies != null)
                {
                    _companies = _parent.Companies;
                }

                InitializeContactList();
            }

            ToggleButton.ImageSource = "pack://application:,,,/resources/group.png";


            _parent.NewCompanyArrived += _parent_NewCompanyArrived;
            //ContactSearchBox searchBox = ContactSearchBox;
            //ContactList.BoundSearchBox = searchBox;
            WindowInitialized = true;
        }

        private void _parent_NewCompanyArrived(object sender, Server.NewCompanyEventArgs e)
        {
            _contactList.AddCompany(e.Company);
        }

        private void InitializeContactList()
        {
            _contactList = new ContactList(_parent.Persons, _parent.Companies);
            _contactList.BoundAlphabetList = AlphabetList;
            _contactList.BoundTabControl = ContactTabControl;
            _contactList.BoundToggleButton = ToggleButton;
            _contactList.BoundSearchBox = ContactSearchBox;
            _contactList.BoundSearchBox.SearchPhraseChanged += (s, ea) =>
            {
                string searchPhrase = _contactList.BoundSearchBox.SearchPhrase;
                if (searchPhrase != "" && searchPhrase != "Wyszukaj")
                {
                    MainMenuStrip.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainMenuStrip.Visibility = Visibility.Visible;
                }
            };

                ContactMainContainer.Children.Add(_contactList);

            _contactList.RenameCompany += _contactList_RenameCompany;

            ToolBarButton addButton = new ToolBarButton("",
                new Uri("pack://application:,,,/resources/add.png"));
            addButton.Click += AddButton_Click;

            MainMenuStrip.AddButton(addButton, Dock.Left);
        }

        private void _contactList_RenameCompany(object sender, EventArgs e)
        {
            Rename rename = new Rename();
            _parent.Darkened = true;
            _parent.mainCanvas.Children.Add(rename);

            rename.CancelButtonClicked += (s, ea) =>
            {
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(rename);
            };

            rename.ReadyButtonClicked += (s, ea) =>
            {
                ContactCompanyListItem companyItem = (ContactCompanyListItem) sender;
                CompanyModel company = companyItem.Company;
                _parent.Client.RenameCompany(company, rename.NewName);

                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(rename);
            };
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            switch (ContactTabControl.SelectedMode)
            {
                case ContactTabControlMode.Contacts:
                    break;
                case ContactTabControlMode.Companies:
                    Rename newCompany = new Rename();
                    _parent.Darkened = true;

                    newCompany.CancelButtonClicked += (s, ea) =>
                    {
                        _parent.Darkened = false;
                        _parent.mainCanvas.Children.Remove(newCompany);
                    };

                    newCompany.ReadyButtonClicked += (s, ea) =>
                    {
                        string companyName = newCompany.NewName;

                        _parent.Client.AddNewCompany(companyName);

                        _parent.Darkened = false;
                        _parent.mainCanvas.Children.Remove(newCompany);
                    };

                    _parent.mainCanvas.Children.Add(newCompany);
                    break;
                case ContactTabControlMode.InternalContacts:
                    break;
            }
        }


        #endregion

        #region Events
        private void ContactSearchBox_SearchPhraseChanged(object sender, EventArgs e)
        {
            //ContactSearchBox searchBox = (ContactSearchBox)sender;

            //ContactMainContainer.Children.Remove(_searchContactList);

            //if (searchBox.SearchPhrase == "")
            //{
            //    AlphabetList.Elements = _alphabetUsed;
            //    //ContactListContainer.Visibility = Visibility.Visible;

            //}
            //else
            //{
            //    if (AlphabetList.Elements.Count > 0)
            //        _alphabetUsed = AlphabetList.Elements;

            //    string searchPhrase = searchBox.SearchPhrase;

            //    AlphabetList.Elements = new List<String>();
            //    ContactListContainer.Visibility = Visibility.Collapsed;

            // List<PersonModel> searchedPersons = _persons.Where(obj => obj.Company != null && ContainsSearchTerm(obj, searchPhrase)).ToList();

            //    List<CompanyModel> searchedCompanies =
            //        _companies.Where(obj => ContainsSearchTerm(obj, searchPhrase)).ToList();

            //    List<PersonModel> searchedInternalContacts = _persons.Where(obj => obj.Company == null && ContainsSearchTerm(obj, searchPhrase)).ToList();

            //    _searchContactList = new ContactList(searchedPersons, searchedCompanies, searchedInternalContacts);

            //    ContactMainContainer.Children.Add(_searchContactList);
            //}
        }

        private void _contactWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_parent.Persons != null)
            {
                _persons = _parent.Persons;
            }

            if (_parent.Companies != null)
            {
                _companies = _parent.Companies;
            }

            InitializeContactList();
        }

        private void ContactTabControl_SelectedModeChanged(object sender, EventArgs e)
        {
            //ContactTabControl tabControl = (ContactTabControl)sender;
            //ContactList deletedList = (ContactList)ContactListContainer.Children[1];
            //deletedList.Toggled = false;
            //ContactListContainer.Children.RemoveAt(1);

            //switch (tabControl.SelectedMode)
            //{
            //    case ContactTabControlMode.Contacts:
            //        ContactListContainer.Children.Add(_contactList);
            //        _contactList.Toggled = true;
            //        AlphabetList.Elements = _contactList.UsedAlphabet;
            //        break;
            //    case ContactTabControlMode.Companies:
            //        ContactListContainer.Children.Add(_companyList);
            //        _companyList.Toggled = true;
            //        AlphabetList.Elements = _companyList.UsedAlphabet;
            //        break;
            //    case ContactTabControlMode.InternalContacts:
            //        ContactListContainer.Children.Add(_internalContactList);
            //        _internalContactList.Toggled = true;
            //        AlphabetList.Elements = _internalContactList.UsedAlphabet;
            //        break;
            //}
        }
        #endregion
    }
}
