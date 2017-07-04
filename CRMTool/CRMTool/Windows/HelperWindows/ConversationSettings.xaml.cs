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
    /// Interaction logic for ConversationSettings.xaml
    /// </summary>
    public partial class ConversationSettings : UserControl
    {
        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public ConversationSettings(ConversationModel conversation)
        {
            InitializeComponent();

            NameLabel.Content = conversation.Name;
            if(conversation.NotifyContactPersons)
                NotificationToggleButton.Toggled = conversation.NotifyContactPersons;

            ReadyButton.Clicked += (s, ea) =>
            {
                conversation.NotifyContactPersons = NotificationToggleButton.Toggled;
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };
        }
    }
}
