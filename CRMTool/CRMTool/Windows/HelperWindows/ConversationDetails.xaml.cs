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
    /// Interaction logic for ConversationDetails.xaml
    /// </summary>
    public partial class ConversationDetails : UserControl
    {
        public event EventHandler CloseButtonClicked;
        public ConversationDetails(ConversationModel conversation)
        {
            InitializeComponent();

            NameLabel.Content = conversation.Name;

            conversation.Members.ForEach(obj => MemberList.AddMemberToList(obj, conversation.ColorDictionary[obj],false));

            VisibleIdLabel.Content = conversation.VisibleId;
            DateCreatedLabel.Content = conversation.DateCreated.ToString("dd.MM.yyyy HH:mm");
            MessageCountLabel.Content = conversation.Messages.Count;

            CloseButton.PreviewMouseLeftButtonDown += CloseButton_PreviewMouseLeftButtonDown;
            CloseButton.MouseEnter += CloseButton_MouseEnter;
            CloseButton.MouseLeave += CloseButton_MouseLeave;
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            CloseImage.Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/Resources/close_light.png"));
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Background = new SolidColorBrush(ColorScheme.MenuLight);
            CloseImage.Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/Resources/close_blue.png"));
        }

        private void CloseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseButton.PreviewMouseLeftButtonUp += CloseButton_PreviewMouseLeftButtonUp;
        }

        private void CloseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseButtonClicked?.Invoke(this, EventArgs.Empty);
            CloseButton.PreviewMouseLeftButtonUp -= CloseButton_PreviewMouseLeftButtonUp;
        }
    }
}
