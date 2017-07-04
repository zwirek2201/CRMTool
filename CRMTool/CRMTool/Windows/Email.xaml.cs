using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Awesomium.Windows.Controls;
using ImapX;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;
using Licencjat_new.Server;
using Licencjat_new.Windows.HelperWindows;
using TheArtOfDev.HtmlRenderer.WPF;
using WpfAnimatedGif;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Email.xaml
    /// </summary>
    public partial class Email : UserControl
    {
        private Client _client;
        private List<EmailModel> _emailClients;
        private List<ConversationModel> _conversations;

        private BackgroundWorker _messageWorker = new BackgroundWorker();
        private BackgroundWorker _processingWorker = new BackgroundWorker();
        
        private List<Tuple<string, Message>> _unprocessedMessages = new List<Tuple<string, Message>>();
        private List<ConversationEmailMessageModel> _awaitingMessages = new List<ConversationEmailMessageModel>();

        private DisplayedMessage _queuedMessage;
        private MainWindow _parent;
        private DisplayedMessage _selectedMessage;

        private DragIndicator _dragIndicator;
        private Cursor _dragAddCursor;

        public void AddToProcess(string conversationId, Message message)
        {
            _unprocessedMessages.Add(Tuple.Create(conversationId, message));
            if (!_processingWorker.IsBusy)
                _processingWorker.RunWorkerAsync();
        }

        public bool WindowInitialized { get; private set; }

        public Email()
        {
            InitializeComponent();
        }

        public void Init()
        {
            EmailList.AddNewEmailAddress += EmailList_AddNewEmailAddress;
            MessagesGrid.SelectionMode = DataGridSelectionMode.Single;
            SmallToolBarWideButton removeButton = new SmallToolBarWideButton("Usuń");
            removeButton.Click += (s, ea) =>
            {
                EmailModel email = (EmailModel)EmailTreeList.SelectedNode.ChildObject;

                DisplayedMessage message = (DisplayedMessage)MessagesGrid.SelectedItem;

                if (message != null)
                {
                    int index = MessagesGrid.SelectedIndex;

                    email.UnhandledMessages.Remove(message.Message);
                    ObservableCollection<DisplayedMessage> messages =
                        (ObservableCollection<DisplayedMessage>) MessagesGrid.ItemsSource;

                    messages.Remove(message);

                    MessagesGrid.ItemsSource = messages;
                    MessagesGrid.Items.Refresh();

                    _client.HandleMessage(email.Id, message.Message.UId.ToString());

                    MessagesGrid.SelectedIndex = index == 0 ? index : index - 1;

                    if (MessagesGrid.Items.Count == 0)
                    {
                        MessagesGrid.Visibility = Visibility.Collapsed;
                        NoMessagesLabel.Visibility = Visibility.Visible;

                        MessageContainer.Visibility = Visibility.Collapsed;
                        MessageDetailsContainer.Visibility = Visibility.Collapsed;
                    }
                }
            };

            SmallToolBarWideButton removeFromServerButton = new SmallToolBarWideButton("Usuń całkowicie");
            removeFromServerButton.Click += (s, ea) =>
            {
                EmailModel email = (EmailModel) EmailTreeList.SelectedNode.ChildObject;

                DisplayedMessage message = (DisplayedMessage) MessagesGrid.SelectedItem;

                if (message != null)
                {
                    message.Message.Remove();

                    int index = MessagesGrid.SelectedIndex;

                    email.UnhandledMessages.Remove(message.Message);
                    ObservableCollection<DisplayedMessage> messages =
                        (ObservableCollection<DisplayedMessage>) MessagesGrid.ItemsSource;

                    messages.Remove(message);

                    MessagesGrid.ItemsSource = messages;
                    MessagesGrid.Items.Refresh();

                    _client.HandleMessage(email.Id, message.Message.UId.ToString());

                    MessagesGrid.SelectedIndex = index == 0 ? index : index - 1;
                }

                if (MessagesGrid.Items.Count == 0)
                {
                    MessagesGrid.Visibility = Visibility.Collapsed;
                    NoMessagesLabel.Visibility = Visibility.Visible;

                    MessageContainer.Visibility = Visibility.Collapsed;
                    MessageDetailsContainer.Visibility = Visibility.Collapsed;
                }
            };

            MessageDetailsMenuStrip.Children.Add(removeButton);

            MessageDetailsContainer.Visibility = Visibility.Collapsed;
            MessageDetailsMenuStrip.Children.Add(removeFromServerButton);

            ConversationList.DisplayItemContextMenus = false;

            _dragIndicator = new DragIndicator();
            _dragAddCursor = new Cursor(new FileStream("../../resources/addMessage_cursor.cur", FileMode.Open));


            _messageWorker.WorkerSupportsCancellation = true;
            _messageWorker.DoWork += MessageWorker_DoWork;
            _messageWorker.RunWorkerCompleted += MessageWorker_RunWorkerCompleted;

            Unloaded += Email_Unloaded;

            _parent = (MainWindow) Window.GetWindow(this);
            _client = _parent.Client;

            if (_parent.EmailWorker.IsBusy)
            {
                _parent.EmailWorker.RunWorkerCompleted += _emailWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent.EmailClients != null)
                {
                    _emailClients = _parent.EmailClients;
                    LoadFolders(_emailClients);
                }
            }

            if (_parent.ConversationWorker.IsBusy)
            {
                _parent.ConversationWorker.RunWorkerCompleted += ConversationWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent.Conversations != null)
                {
                    _conversations = _parent.Conversations;
                    LoadConversations();
                }
            }

            MessagesGrid.Items.IsLiveSorting = true;
           // MessagesGrid.SelectionMode = DataGridSelectionMode.Extended;


            MessagesGrid.PreviewMouseLeftButtonDown += MessagesGrid_PreviewMouseLeftButtonDown;
            MessagesGrid.GiveFeedback += MessagesGrid_GiveFeedback;

            _processingWorker.DoWork += _processingWorker_DoWork;

            ConversationList.AllowDrop = true;

            WindowInitialized = true;   
        }

        private void EmailList_AddNewEmailAddress(object sender, EventArgs e)
        {
            NewEmailAddress newEmail = new NewEmailAddress();

            newEmail.ReadyButtonClicked += (s, ea) =>
            {
                ea.Login = CryptographyHelper.EncodeString(ea.Login);
                ea.Password = CryptographyHelper.EncodeString(ea.Password);

                RegistryHelper.AddRegistryValue(ea.Login, ea.Password);
                _parent.Client.AddNewEmailAddress(ea);
            };

            newEmail.CancelButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(newEmail);
                _parent.Darkened = false;
            };

            _parent.Darkened = true;
            _parent.mainCanvas.Children.Add(newEmail);
        }

        private void MessagesGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < MessagesGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow) MessagesGrid.ItemContainerGenerator.ContainerFromIndex(i);

                if (row != null)
                {
                    Rect rowRect = VisualTreeHelper.GetDescendantBounds(row);
                    Point mousePosition = e.GetPosition(row);

                    if (rowRect.Contains(mousePosition))
                    {
                        DisplayedMessage selectedMessage = (DisplayedMessage) MessagesGrid.Items[i];

                        DragDropEffects dragdropeffects = DragDropEffects.Copy;
                        DragDrop.DoDragDrop(MessagesGrid, selectedMessage, dragdropeffects);
                        MessagesGrid.SelectedItem = selectedMessage;
                    }
                }
            }
        }

        private void _processingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_unprocessedMessages.Any())
            {
                Tuple<string, Message> messageTuple = _unprocessedMessages.First();

                string conversationId = messageTuple.Item1;
                Message message = messageTuple.Item2;

                PersonModel messageSender =
                    _parent.Persons.Find(
                        obj => obj.EmailAddresses.Any(obj2 => obj2.Address == message.From.Address));

                EmailAddressModel authorEmailAddress =
                    messageSender.EmailAddresses.Single(obj => obj.Address == message.From.Address);

                ConversationMessageModel receivedMessage = new ConversationMessageModel(messageSender,
                    message.InternalDate)
                {ConversationId = conversationId};

                ConversationEmailMessageModel emailMessage =
                    new ConversationEmailMessageModel(receivedMessage, authorEmailAddress,
                        message.Subject,
                        message.Body.Html == "" ? message.Body.Text : message.Body.Html);
                this.Dispatcher.Invoke(() =>
                {

                    BitmapSource previewImage =
                        ImageHelper.GetHtmlImagePreview(
                            message.Body.Html == "" ? message.Body.Text : message.Body.Html,
                            new Size(600, 60), new Size(600, 250));

                    previewImage.Freeze();

                    emailMessage.PreviewImage = previewImage;
                });
                if (message.Attachments.Any())
                {
                    _parent.AwaitingMessages.Add(emailMessage);
                    foreach (Attachment attachment in message.Attachments)
                    {
                        if (!attachment.Downloaded)
                            attachment.Download();

                        FileModel file = new FileModel(attachment, DateTime.Today);

                        emailMessage.Attachments.Add(file);
                    }

                    _parent.UploadClient.UploadFiles(emailMessage, emailMessage.Attachments);

                }
                else
                {
                    _parent.Client.AddNewMessage(conversationId, emailMessage);
                }
                _unprocessedMessages.Remove(messageTuple);
            }
        }

        private void MessagesGrid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(_dragAddCursor);
            }
            else
            {
                e.UseDefaultCursors = true;
            }
            e.Handled = true;
        }

        private void ConversationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_parent.Conversations != null)
            {
                _conversations = _parent.Conversations;
                LoadConversations();
            }
        }

        private void LoadConversations()
        {
            ConversationList conversationList = (ConversationList)LogicalTreeHelper.FindLogicalNode(this, "ConversationList");

            if (conversationList == null) return;

            conversationList.Clear();
            foreach (ConversationModel conversation in _conversations)
            {
                ConversationListItem conversationItem = new ConversationListItem(conversation);
                conversationItem.Drop += ConversationItem_Drop;
                conversationItem.DragOver += ConversationItem_DragOver;
                conversationItem.DragLeave += ConversationItem_DragLeave;

                conversationList.AddConversation(conversationItem);
            }
        }

        private void ConversationItem_DragLeave(object sender, DragEventArgs e)
        {
            _dragIndicator.IsOpen = false;
            _dragIndicator.StaysOpen = false;

            ConversationListItem listItem = (ConversationListItem)sender;
            listItem.SetBackgroundColor(new SolidColorBrush(ColorScheme.MenuLight));
        }

        private void ConversationItem_DragOver(object sender, DragEventArgs e)
        {
            ConversationListItem listItem = (ConversationListItem)sender;
            listItem.SetBackgroundColor(new SolidColorBrush(ColorScheme.MenuDarker));

            _dragIndicator.IsOpen = true;
            _dragIndicator.Text = listItem.Conversation.Name;
            _dragIndicator.Placement = PlacementMode.Absolute;
            _dragIndicator.VerticalOffset = e.GetPosition(this).Y - 5;
            _dragIndicator.HorizontalOffset = e.GetPosition(this).X;
        }

        private void ConversationItem_Drop(object sender, DragEventArgs e)
        {
            try
            {
                ConversationListItem item = (ConversationListItem)sender;

                if (e.Data.GetDataPresent(typeof(DisplayedMessage)))
                {
                    string conversationId = item.Conversation.Id;
                    DisplayedMessage message = (DisplayedMessage)e.Data.GetData(typeof(DisplayedMessage));
                    PersonModel messageSender =
                        _parent.Persons.Find(
                            obj => obj.EmailAddresses.Any(obj2 => obj2.Address == message.Message.From.Address));

                    if (conversationId != null)
                    {
                        if (messageSender != null)
                        {

                            AddToProcess(conversationId, message.Message);

                            //EmailAddressModel authorEmailAddress =
                            //    messageSender.EmailAddresses.Single(obj => obj.Address == message.Message.From.Address);

                            //ConversationMessageModel receivedMessage = new ConversationMessageModel(messageSender,
                            //    message.Message.InternalDate);
                            //ConversationEmailMessageModel emailMessage =
                            //    new ConversationEmailMessageModel(receivedMessage, authorEmailAddress, message.Subject,
                            //        message.Message.Body.Html == "" ? message.Message.Body.Text : message.Message.Body.Html)
                            //    { ConversationId = conversationId };

                            //if (_processingWorker.IsBusy)
                            //{
                            //    _unprocessedMessages.Add(emailMessage);
                            //}
                            //else
                            //{
                            //    _processingWorker.RunWorkerAsync(emailMessage);
                            //}

                            EmailModel email = (EmailModel)EmailTreeList.SelectedNode.ChildObject;
                            email.UnhandledMessages.Remove(message.Message);
                            ObservableCollection<DisplayedMessage> messages = (ObservableCollection<DisplayedMessage>)MessagesGrid.ItemsSource;

                            messages.Remove(message);

                            MessagesGrid.ItemsSource = messages;
                            MessagesGrid.Items.Refresh();

                            _client.HandleMessage(email.Id, message.Message.UId.ToString());
                        }
                        else
                        {
                        }
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Email_Unloaded(object sender, RoutedEventArgs e)
        {
            _parent.EmailWorker.RunWorkerCompleted -= _emailWorker_RunWorkerCompleted;
            _messageWorker.RunWorkerCompleted -= MessageWorker_RunWorkerCompleted;
        }

        private void _emailWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            List<EmailModel> clients = (List<EmailModel>)e.Result;
            LoadFolders(clients);
            Label statusLabel = (Label)LogicalTreeHelper.FindLogicalNode(_parent, "leftLabel");
            statusLabel.Content = "Zakończono pobieranie";

            Image loadingImage = (Image)LogicalTreeHelper.FindLogicalNode(_parent, "leftImage");
            ImageBehavior.SetAnimatedSource(loadingImage, null);
        }

        private void LoadFolders(List<EmailModel> clients)
        {
            CustomTreeListControl tree = (CustomTreeListControl)LogicalTreeHelper.FindLogicalNode(this, "EmailTreeList");
            tree.SelectedNodeChanged += EmailTree_SelectedNodeChanged;
            foreach (EmailModel client in clients)
            {
                if (client.ImapClient != null)
                {
                    long unseenCount = 0;

                    foreach (Message message in client.UnhandledMessages)
                    {
                        unseenCount += message.Seen ? 0 : 1;
                    }

                    CustomTreeListNode rootNode = new CustomTreeListNode(client.Address,
                        ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/mail.png")));

                    rootNode.ChildObject = client;
                    tree.AddNode(rootNode);

                    client.UnseenCountChanged += Client_UnseenCountChanged;
                    client.UnseenCount = (Int32)unseenCount;
                }
                else
                {
                    CustomTreeListNode rootNode = new CustomTreeListNode(client.Address, ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/mail_locked.png")));
                    rootNode.ChildObject = client;
                    tree.AddNode(rootNode);
                }
            }
            MessagesGrid.SelectedCellsChanged += MessagesGrid_SelectedCellsChanged;
        }

        private void Client_UnseenCountChanged(object sender, EventArgs e)
        {
            EmailModel client = (EmailModel) sender;
            CustomTreeListNode node = (CustomTreeListNode)EmailTreeList.Nodes.Single(obj => obj.ChildObject == client);

            node.Text = client.UnseenCount > 0
                ? client.Address + " (" + client.UnseenCount + ")"
                : client.Address;
            if (client.UnseenCount > 0)
            {
                node.Bold = true;
            }
        }

        private void EmailTree_SelectedNodeChanged(object sender, CustomTreeListSelectedNodeChangedEventArgs e)
        {
            CustomTreeListNode selectedNode = e.SelectedNode;
            if (selectedNode.ChildObject.GetType().Name == "EmailModel")
            {
                EmailModel email = (EmailModel)selectedNode.ChildObject;
                if (email.ImapClient == null)
                {
                    MessageBox.Show("Zablokowany");
                    return;
                }

                FillMessages(email.UnhandledMessages);
            }
            _parent.NewUnhandledMessageArrived += _parent_NewUnhandledMessageArrived;
        }

        private void _parent_NewUnhandledMessageArrived(object sender, NewUnhandledMessageArrivedEventArgs e)
        {
            FillMessages(e.Email.UnhandledMessages);
        }

        private void MessagesGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DisplayedMessage message = (DisplayedMessage) MessagesGrid.SelectedItem;

            if (message != null)
            {
                Label fromLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "fromLabel");
                Label subjectLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "subjectLabel");
                Label toLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "toLabel");

                AttachmentList.Children.Clear();

                foreach (Attachment attachment in message.Message.Attachments)
                {
                    FileListItem listItem = new FileListItem(new FileModel(attachment, DateTime.Today));
                    AttachmentList.Children.Add(listItem);
                }

                MessageContainer.Visibility = Visibility.Collapsed;

                fromLabel.Content = message.Message.From;
                subjectLabel.Content = message.Subject;
                toLabel.Content = message.Message.To[0];

                if (_messageWorker.IsBusy)
                    _queuedMessage = message;
                else
                    _messageWorker.RunWorkerAsync(message);
            }
        }

        private void MessageWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                DisplayedMessage message = (DisplayedMessage)e.Result;

                if (!message.Message.Seen)
                {
                    message.Message.Seen = true;
                    message.Seen = true;
                    EmailModel client = (EmailModel)EmailTreeList.SelectedNode.ChildObject;
                    client.UnseenCount--;
                }

                HtmlPanel messageContainer = (HtmlPanel)LogicalTreeHelper.FindLogicalNode(this, "MessageContainer");

                if (_queuedMessage == null)
                {
                    _queuedMessage = null;

                    messageContainer.Text = !message.Message.Body.HasHtml
                        ? message.Message.Body.Text
                        : " <meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>" +
                          message.Message.Body.Html;
                }
                else
                {
                    _messageWorker.RunWorkerAsync(_queuedMessage);
                    _queuedMessage = null;
                }
                messageContainer.Visibility = Visibility.Visible;
                MessageDetailsContainer.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                
            }
        }

        private void MessageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
                DisplayedMessage message = (DisplayedMessage) e.Argument;
                message.Message.Download(ImapX.Enums.MessageFetchMode.Body);
                
                e.Result = message;
        }

        private void FillMessages(List<Message> messages)
        {
            ObservableCollection<DisplayedMessage> displayedMessages = new ObservableCollection<DisplayedMessage>();

            if (messages.Count > 0)
            {
                foreach (Message message in messages)
                {
                    DisplayedMessage displayedMessage = new DisplayedMessage(message,
                        message.Subject.Replace("\n", "").Replace("\r", "").Trim(' '),
                        message.From.DisplayName == "" ? message.From.Address : message.From.DisplayName,
                        message.Date.Value,
                        message.Seen);

                    displayedMessages.Add(displayedMessage);
                }
                BindingOperations.EnableCollectionSynchronization(displayedMessages, new object());
                MessagesGrid.ItemsSource = displayedMessages;
                MessagesGrid.Items.Refresh();

                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Message").ToList<DataGridColumn>()[0].Visibility = Visibility.Collapsed;
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Seen").ToList<DataGridColumn>()[0].Visibility = Visibility.Collapsed;
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Subject").ToList<DataGridColumn>()[0].Width = new DataGridLength(3, DataGridLengthUnitType.Star);
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "From").ToList<DataGridColumn>()[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Date").ToList<DataGridColumn>()[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Subject").ToList<DataGridColumn>()[0].Header = "Temat";
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "From").ToList<DataGridColumn>()[0].Header = "Od";
                MessagesGrid.Columns.Where(column => column.Header.ToString() == "Date").ToList<DataGridColumn>()[0].Header = "Data";

                MessagesGrid.Visibility = Visibility.Visible;
                NoMessagesLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessagesGrid.Visibility = Visibility.Collapsed;
                NoMessagesLabel.Visibility = Visibility.Visible;
            }
        }
    }

    internal class DisplayedMessage :INotifyPropertyChanged
    {
        public Boolean _seen;

        public string Subject { get; private set; }
        public string From { get; private set; }
        public DateTime Date { get; private set; }
        public Message Message { get; private set; }

        public Boolean Seen
        {
            get { return _seen; }
            set
            {
                _seen = value;
                OnPropertyChanged();
            }
        }

        public DisplayedMessage(Message message, string subject, string from, DateTime date, Boolean seen)
        {
            Message = message;
            Subject = subject;
            From = from;
            Date = date;
            Seen = seen;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
