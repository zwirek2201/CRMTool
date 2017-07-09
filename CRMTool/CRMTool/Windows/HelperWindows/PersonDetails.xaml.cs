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
    /// Interaction logic for PersonDetails.xaml
    /// </summary>
    public partial class PersonDetails : UserControl
    {
        public PersonDetails(PersonModel person)
        {
            InitializeComponent();

            FirstNameTextBox.Text = person.FirstName;
            LastNameTextBox.Text = person.LastName;

            if (person.Company != null)
                CompanyTextBox.Text = person.Company.Name;

            GenderComboBox.AddItem("Kobieta");
            GenderComboBox.AddItem("Mężczyzna");

            if (person.Gender == Gender.Female)
                GenderComboBox.SelectedItem = GenderComboBox.Items[0];
            else
                GenderComboBox.SelectedItem = GenderComboBox.Items[1];
        }
    }
}
