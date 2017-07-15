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
using Licencjat_new.Windows.HelperWindows;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for ChooseCompany.xaml
    /// </summary>
    public partial class ChooseCompany : UserControl
    {
        private MainWindow _parent;
        private List<CompanyModel> _companies;
        private List<CompanyModel> _blockedCompanies;
        private bool _multipleSelection = true;

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

        public List<CompanyModel> SelectedCompanies { get; set; } = new List<CompanyModel>();

        public ChooseCompany(MainWindow parent, List<CompanyModel> blockedCompanies, bool multipleSelection = true)
        {
            _parent = parent;

            _multipleSelection = multipleSelection;

            if (blockedCompanies != null)
                _blockedCompanies = blockedCompanies;
            else
                _blockedCompanies = new List<CompanyModel>();

            InitializeComponent();

            if (_parent.ContactWorker.IsBusy)
            {
                _parent.ContactWorker.RunWorkerCompleted += ContactWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent.Companies != null)
                {
                    _companies = _parent.Companies;
                }

                InitializeContactList();
            }

            ReadyButton.Clicked += (s, ea) =>
            {
                if (SelectedCompanies.Count > 0)
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
            if (_parent.Companies != null)
            {
                _companies = _parent.Companies;
            }

            InitializeContactList();
        }

        private void InitializeContactList()
        {
            _contactList = new ContactList(new List<PersonModel>(), _companies.Where(obj => !_blockedCompanies.Contains(obj)).ToList(), true, SelectionModeType.CompanySelect, false);
            _contactList.BoundAlphabetList = AlphabetList;
            _contactList.BoundTabControl = ContactTabControl;
            _contactList.SelectedItemsChanged += _contactList_SelectedItemsChanged;

            InnerDock.Children.Add(_contactList);
        }

        private void _contactList_SelectedItemsChanged(object sender, EventArgs e)
        {
            SelectedCompanies = _contactList.SelectedCompanies.Select(obj => obj.Company).ToList();

            int selectedCount = SelectedCompanies.Count;
            SelectedCountLabel.Content = selectedCount + " firm";
        }
    }
}