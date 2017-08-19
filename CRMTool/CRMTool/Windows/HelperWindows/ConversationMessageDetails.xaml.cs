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
using ImapX;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for ConversationMessageDetails.xaml
    /// </summary>
    public partial class ConversationMessageDetails : UserControl
    {
        public event EventHandler CloseButtonClicked;

        public ConversationMessageDetails(ConversationMessageModel message)
        {
            this.Dispatcher.Invoke(() =>
            {
                InitializeComponent();

                DetailsTable.AddItem("Data: ", message.InitialDate.Value.ToString("dd.MM.yyyy HH:mm"));

                if (message.Attachments.Count == 0)
                {
                    AttachmentListContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (FileModel attachment in message.Attachments)
                    {
                        FileListItem listItem = new FileListItem(attachment);
                        listItem.AllowDelete = false;
                        listItem.AllowDownload = false;
                        listItem.AllowRename = false;
                        listItem.AllowSelect = false;
                        AttachmentList.Children.Add(listItem);
                    }
                }

                if (message is ConversationEmailMessageModel)
                {
                    ConversationEmailMessageModel emailMessage = (ConversationEmailMessageModel) message;
                    EmailAddressModel email = message.Author.EmailAddresses.Find(obj => obj.Id == message.AuthorFrom);
                    DetailsTable.AddItem("Od: ", message.Author.FullName + " <" + email.Address + ">");
                    SubjectLabel.Text = emailMessage.MessageSubject;
                    MessageContainer.Text = emailMessage.MessageContent;
                }
                else if (message is ConversationPhoneMessageModel)
                {
                    ConversationPhoneMessageModel phoneMessage = (ConversationPhoneMessageModel)message;

                    DetailsTable.AddItem("Od: ", message.Author.FullName + " (" + phoneMessage.AuthorPhoneNumber.Number + ")");
                    DetailsTable.AddItem("Do: ", phoneMessage.Recipient.FullName + " (" + phoneMessage.RecipientPhoneNumber.Number + ")");
                    
                    SubjectLabelContainer.Visibility = Visibility.Collapsed;

                    if (!phoneMessage.CallAnswered)
                        CallUnansweredContainer.Visibility = Visibility.Visible;

                    MessageContainer.Margin = new Thickness(MessageContainer.Margin.Left,10,MessageContainer.Margin.Right, MessageContainer.Margin.Bottom);
                    MessageContainer.Text = phoneMessage.CallDescription;
                }

                CloseButton.PreviewMouseLeftButtonDown += CloseButton_PreviewMouseLeftButtonDown;
                CloseButton.MouseEnter += CloseButton_MouseEnter;
                CloseButton.MouseLeave += CloseButton_MouseLeave;
            });

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
