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
        public event EventHandler YesButtonClicked;
        public event EventHandler NoButtonClicked;

        public CustomMessageBox(string text, MessageBoxButton buttons)
        {
            InitializeComponent();

            Text = text;
            MessageText.Text = text;
            MessageText.TextWrapping = TextWrapping.Wrap;
            MessageText.Padding = new Thickness(10);

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
                case MessageBoxButton.YesNo:
                    RoundedButton yesbutton = new RoundedButton("Tak", new SolidColorBrush(ColorScheme.GlobalBlue),
                        new SolidColorBrush(ColorScheme.MenuLight));
                    yesbutton.Clicked += (s, ea) =>
                    {
                        YesButtonClicked?.Invoke(this, EventArgs.Empty);
                    };

                    DockPanel.SetDock(yesbutton, Dock.Right);
                    ButtonsContainer.Children.Add(yesbutton);

                    RoundedButton nobutton = new RoundedButton("Nie", new SolidColorBrush(ColorScheme.GlobalBlue),
                        new SolidColorBrush(ColorScheme.MenuLight));
                    nobutton.Clicked += (s, ea) =>
                    {
                        NoButtonClicked?.Invoke(this, EventArgs.Empty);
                    };

                    DockPanel.SetDock(nobutton, Dock.Right);
                    ButtonsContainer.Children.Add(nobutton);
                    break;
            }
        }
    }
}
