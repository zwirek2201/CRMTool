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

namespace Licencjat_new.Windows.ScanningWindows
{
    /// <summary>
    /// Interaction logic for ScanningAutomatic.xaml
    /// </summary>
    public partial class ScanningAutomatic : UserControl
    {
        public ScanningAutomatic()
        {
            InitializeComponent();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            txtDelay.Text = (Convert.ToInt32(txtDelay.Text) + 1).ToString();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if(Convert.ToInt32(txtDelay.Text) != 1)
                txtDelay.Text = (Convert.ToInt32(txtDelay.Text) - 1).ToString();

        }

        private void txtDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;

            if (!(int.TryParse(e.Text, out result)))
            {
                e.Handled = true;
            }
        }
    }
}
