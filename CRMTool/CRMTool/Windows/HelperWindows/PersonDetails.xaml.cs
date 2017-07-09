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
        private MainWindow _parent;


        public PersonDetails(MainWindow parent, PersonModel person)
        {
            _parent = parent;

            InitializeComponent();

            FirstNameTextBox.Text = person.FirstName;
            LastNameTextBox.Text = person.LastName;

            if (person.Company != null)
                CompanyTextBox.Text = person.Company.Name;

            CompanyTextBox.IsEnabled = false;
            ChangeCompanyButton.Clicked += ChangeCompanyButton_Clicked;

            GenderComboBox.AddItem("Kobieta");
            GenderComboBox.AddItem("Mężczyzna");

            if (person.Gender == Gender.Female)
                GenderComboBox.SelectedItem = GenderComboBox.Items[0];
            else
                GenderComboBox.SelectedItem = GenderComboBox.Items[1];
        }

        private void ChangeCompanyButton_Clicked(object sender, EventArgs e)
        {
            ChooseCompany choose = new ChooseCompany(_parent, new List<CompanyModel>(), false);

            choose.ReadyButtonClicked += (s, ea) =>
            {
                if (choose.SelectedCompanies.Count > 0)
                    CompanyTextBox.Text = choose.SelectedCompanies[0].Name;

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