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
    /// Interaction logic for NewCompany.xaml
    /// </summary>
    public partial class NewCompany : UserControl
    {
        public NewCompany(CompanyModel company = null)
        {
            InitializeComponent();
            List<PersonModel> companyMembers;

            if (company == null)
                companyMembers = new List<PersonModel>();
            else
                companyMembers = company.Persons;
                            
            ContactList contactList = new ContactList(companyMembers, new List<CompanyModel>(), false, false);

            MainDock.Children.Add(contactList);
        }
    }
}
