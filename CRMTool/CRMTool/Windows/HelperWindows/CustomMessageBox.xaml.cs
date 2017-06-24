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
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : UserControl
    {
        public string Text { get; set; }

        public event EventHandler OKButtonClicked;
        public CustomMessageBox(string text, MessageBoxButton buttons)
        {
            InitializeComponent();

            Text = text;
            MessageText.Text = text;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    RoundedButton button = new RoundedButton("OK", new SolidColorBrush(ColorScheme.GlobalBlue),
                        new SolidColorBrush(ColorScheme.MenuLight));
                    button.Clicked += (s, ea) =>
                    {
                        OKButtonClicked?.Invoke(this, EventArgs.Empty);
                    };

                    DockPanel.SetDock(button, Dock.Right);
                    ButtonsContainer.Children.Add(button);
                    break;
            }
        }
    }
}
