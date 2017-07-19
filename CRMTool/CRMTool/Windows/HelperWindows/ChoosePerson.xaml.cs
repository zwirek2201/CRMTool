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
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for ChoosePerson.xaml
    /// </summary>
    public partial class ChoosePerson : UserControl
    {
        private MainWindow _parent;
        private List<PersonModel> _persons;
        private List<CompanyModel> _companies;
        private List<PersonModel> _blockedPersons;
        private bool _multipleSelection = true;

        private ChoosePersonMode _chooseMode;

        private bool _darkened = false;

        private ContactList _contactList;

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public bool Darkened
        {
            get { return _darkened; }
            set
            {
                _darkened = value;
                if (Darkened)
                {
                    DarkenerPanel.Visibility = Visibility.Visible;

                    DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    DarkenerPanel.BeginAnimation(OpacityProperty, fadeInAnimation);
                }
                else
                {
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    fadeOutAnimation.Completed += (s, e) => { DarkenerPanel.Visibility = Visibility.Collapsed; };
                    DarkenerPanel.BeginAnimation(OpacityProperty, fadeOutAnimation);
                }
            }
        }

        public List<PersonModel> SelectedPersons { get; set; } = new List<PersonModel>();

        public ChoosePerson(MainWindow parent, List<PersonModel> blockedPersons, bool multipleSelection = true, ChoosePersonMode chooseMode = ChoosePersonMode.ChoosePerson)
        {
            _parent = parent;

            _multipleSelection = multipleSelection;

            if (blockedPersons != null)
                _blockedPersons = blockedPersons;
            else
                _blockedPersons = new List<PersonModel>();

            InitializeComponent();

            if (_parent.ContactWorker.IsBusy)
            {
                _parent.ContactWorker.RunWorkerCompleted += ContactWorker_RunWorkerCompleted; ;
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

            ReadyButton.Clicked += (s, ea) =>
            {
                if (SelectedPersons.Count > 0)
                {
                    ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            };

            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            _parent.NewCompanyArrived += _parent_NewCompanyArrived;
        }

        private void _parent_NewCompanyArrived(object sender, Server.NewCompanyEventArgs e)
        {
            if (!_contactList.Companies.Contains(e.Company))
                _contactList.AddCompany(e.Company);
        }

        private void ContactWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
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

        private void InitializeContactList()
        {
            _contactList = new ContactList(_parent.Persons.Where(obj => !_blockedPersons.Contains(obj)).ToList(), _parent.Companies, true, SelectionModeType.PersonSelect, _multipleSelection);
            _contactList.BoundAlphabetList = AlphabetList;
            _contactList.BoundTabControl = ContactTabControl;
            _contactList.SelectedItemsChanged += _contactList_SelectedItemsChanged;

            InnerDock.Children.Add(_contactList);
        }

        private void _contactList_SelectedItemsChanged(object sender, EventArgs e)
        {
            SelectedPersons = _contactList.SelectedPersons.Select(obj => obj.Person).ToList();
        }
    }

    public enum ChoosePersonMode
    {
        ChoosePerson,
        ChooseEmailAddress,
        ChoosePhoneNumber
    }
}