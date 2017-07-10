using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ImapX;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;
using Licencjat_new.Server;
using Licencjat_new.Windows.HelperWindows;
using WpfAnimatedGif;
using Attachment = ImapX.Attachment;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

        public List<ConversationMessageModel> AwaitingMessages { get; private set; } =
            new List<ConversationMessageModel>();

        #region Events

        public event EventHandler<LoginStatusChangedEventArgs> LoginStatusChanged;
        public event EventHandler<NewUnhandledMessageArrivedEventArgs> NewUnhandledMessageArrived;
        public event EventHandler<NewConversationMessageArrivedEventArgs> NewMessageArrived;
        public event EventHandler<NewFilesArrivedEventArgs> NewFileArrived;
        public event EventHandler<NewConversationArrivedEventArgs> NewConversationArrived;
        public event EventHandler<ConversationMembersAddedEventArgs> NewconversationMembers;
        public event EventHandler<ConversationMemberRemovedEventArgs> ConversationMemberRemoved;
        public event EventHandler<ConversationRemovedEventArgs> ConversationRemoved;
        public event EventHandler<ConversationSettingsChangedEventArgs> ConversationSettingsChanged;
        public event EventHandler<NewCompanyEventArgs> NewCompanyArrived;
        public event EventHandler<CompanyRenamedEventArgs> CompanyRenamed;
        public event EventHandler<NewEmailAddressEventArgs> NewEmailAddress;
        public event EventHandler<CompanyRemovedEventArgs> CompanyRemoved;

        #endregion

        #region NotificationVariables

        private NotificationsPanel _notificationPanel;
        private JustInTimeNotificationPanel _justInTimeNotificationPanel;
        private NotificationModel _unhandledNotification;

        #endregion

        #region Windows

        private Email _emailWindow;
        private Contact _contactWindow;
        private Conversation _conversationWindow;
        private HomePage _homePageWindow;
        private Document _documentWindow;

        #endregion

        private bool _darkened;
        private Grid _darkener = new Grid() { Background = new SolidColorBrush(Colors.Black) { Opacity = 0.7 } , VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch};
        
        #endregion

        #region Properties

        public bool Darkened
        {
            get { return _darkened; }
            set
            {
                Panel.SetZIndex(_darkener, 300);
                _darkener.Height = mainCanvas.ActualHeight;
                _darkener.Width = mainCanvas.ActualWidth;

                _darkened = value;
                if (Darkened)
                {
                    if (!mainCanvas.Children.Contains(_darkener))
                    {
                        _darkener.Opacity = 0;
                        mainCanvas.Children.Add(_darkener);

                        DoubleAnimation fadeInAnimation = new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(100)
                        };
                        _darkener.BeginAnimation(OpacityProperty, fadeInAnimation);
                    }
                }
                else
                {
                    if (mainCanvas.Children.Contains(_darkener))
                    {
                        DoubleAnimation fadeOutAnimation = new DoubleAnimation()
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(100)
                        };

                        fadeOutAnimation.Completed += FadeOutAnimation_Completed;
                        _darkener.BeginAnimation(OpacityProperty, fadeOutAnimation);
                    }
                }
            }
        }

        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            mainCanvas.Children.Remove(_darkener);
        }

        #region User

        public bool LoggedIn { get; private set; }
        public string UserId { get; private set; }

        #endregion

        #region Clients

        public Client Client { get; set; }
        public NotificationClient NotificationClient { get; set; }
        public UploadClient UploadClient { get; set; }
        public DownloadClient DownloadClient { get; set; }

        #endregion

        #region Workers

        public BackgroundWorker EmailWorker { get; private set; }
        public BackgroundWorker ConversationWorker { get; private set; }
        public BackgroundWorker ContactWorker { get; private set; }
        public BackgroundWorker NotificationWorker { get; private set; }
        public BackgroundWorker ProcessingWorker { get; private set; }
        public BackgroundWorker FileWorker { get; private set; }

        #endregion

        #region Lists

        public List<EmailModel> EmailClients { get; set; } = new List<EmailModel>();
        public List<ConversationModel> Conversations { get; set; }
        public List<CompanyModel> Companies { get; set; }
        public List<PersonModel> Persons { get; set; }
        public List<FileModel> Files { get; set; }
        #endregion

        #region TitleBarButtons

        private TitleBarButton _closeButton;
        private TitleBarButton _maximizeButton;
        private TitleBarButton _minimizeButton;
        #endregion

        #region Notifications
        public NotificationModel UnhandledNotificationsPendingNotification { get; set; }
        #endregion
        #endregion

        #region Constructors

        public MainWindow()
        {
            ErrorHelper.CreateErrorLogFile();

            try
            {
                InitializeComponent();
                UpperMenu.Parent = this;

                Login loginPage = new Login();

                ContentArea.Content = loginPage;

                UpperMenu.UpperMenuModeChanged += UpperMenuModeChanged;
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 2;

                _closeButton =
                    new TitleBarButton(
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/close_light.png")),
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/close_blue.png")));
                _closeButton.Clicked += CloseButton_Clicked;

                _maximizeButton =
                    new TitleBarButton(
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_light.png")),
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_blue.png")));
                _maximizeButton.Clicked += MaximizeButton_Clicked;

                _minimizeButton =
                    new TitleBarButton(
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/minimize_light.png")),
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/minimize_blue.png")));
                _minimizeButton.Clicked += MinimizeButton_Clicked;

                Grid dragPanel = new Grid()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(ColorScheme.GlobalBlue)
                };

                dragPanel.PreviewMouseLeftButtonDown += DragPanel_MouseDown;

                DockPanel.SetDock(_closeButton, Dock.Right);
                DockPanel.SetDock(_maximizeButton, Dock.Right);
                DockPanel.SetDock(_minimizeButton, Dock.Right);

                TitleBar.Children.Add(_closeButton);
                TitleBar.Children.Add(_maximizeButton);
                TitleBar.Children.Add(_minimizeButton);
                TitleBar.Children.Add(dragPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }


        private void DragPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (e.ClickCount == 2)
                    {
                        AdjustWindowSize();
                    }
                    else
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            MouseMove += MainWindow_MouseMove;
                        }
                        Application.Current.MainWindow.DragMove();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    AdjustWindowSize();
                    this.Top = 0;
                    Application.Current.MainWindow.DragMove();
                }
                MouseMove -= MainWindow_MouseMove;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void MinimizeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void MaximizeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                AdjustWindowSize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void AdjustWindowSize()
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            if (WindowState == WindowState.Maximized)
            {
                _maximizeButton.HoverImage =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_blue.png"));
                _maximizeButton.Image =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_light.png"));
            }
            else
            {
                _maximizeButton.HoverImage =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_2_blue.png"));
                _maximizeButton.Image =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/maximize_2_light.png"));
            }
        }

        #endregion

        #region BackgroundWorkers

        #region ContactWorker

        private void _contactWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");
                    statusLabel.Content = "Pobieranie kontaktów";
                });

                List<CompanyModel> companies = Client.GetAllCompanies();
                Companies = companies;

                List<PersonModel> persons = Client.GetAllContacts();
                Persons = persons;

                foreach (PersonModel person in persons)
                {
                    if (person.CompanyId != "")
                    {
                        CompanyModel company = Companies.Find(obj => obj.Id == person.CompanyId);
                        if (company != null)
                            person.Company = company;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void _contactWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FileWorker.RunWorkerAsync();
        }

        #endregion

        #region EmailWorker

        private void _emailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");
                    Image loadingImage = (Image) LogicalTreeHelper.FindLogicalNode(this, "leftImage");

                    if (statusLabel != null)
                        statusLabel.Content = $"Ładowanie adresów email";

                    ImageBehavior.SetAnimatedSource(loadingImage,
                        ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/loading-white.gif")));
                    //ImageBehavior.SetAutoStart(loadingImage, true);
                });

                List<EmailModel> emails = (List<EmailModel>) e.Argument;

                int emailTotal = emails.Count;
                int emailCount = 0;

                foreach (EmailModel email in emails)
                {
                    emailCount++;
                    this.Dispatcher.Invoke(() =>
                    {
                        Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");

                        if (statusLabel != null)
                            statusLabel.Content = $"Ładowanie adresów email ({emailCount}/{emailTotal})";

                    });
                    string searchString = "";

                    if (email.UnhandledMessagesIds.Any() || !string.IsNullOrEmpty(email.LastUid))
                    {
                        searchString += "UID ";

                        if (email.UnhandledMessagesIds.Count > 0)
                        {
                            foreach (string message in email.UnhandledMessagesIds)
                            {
                                searchString += message;
                                searchString += email.UnhandledMessagesIds.Last() == message ? "" : ",";
                            }
                        }

                        if (email.LastUid != "")
                        {
                            long uidNumber = Convert.ToInt64(email.LastUid);
                            searchString += (email.UnhandledMessagesIds.Any() ? "," : "") + (uidNumber + 1) + ":*";
                        }
                    }
                    else
                    {
                        DateTime substractDate = DateTime.Now;
                        substractDate = substractDate.AddDays(-14);

                        searchString += "SINCE " + substractDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    }

                    ImapClient client = EmailHelper.ConnectToServer(email.ImapHost, email.ImapPort, email.ImapUseSsl);

                    if (email.Login != "" && client != null)
                    {
                        SmtpClient smtpClient = new SmtpClient(email.SmtpHost, email.SmtpPort);
                        smtpClient.EnableSsl = email.SmtpUseSsl;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.SendCompleted += SmtpClient_SendCompleted;

                        string login = CryptographyHelper.DecodeString(email.Login);
                        string password =
                            RegistryHelper.GetRegistryValue(CryptographyHelper.HashString(email.Address, 0));
                        if (password != null)
                        {
                            string decodedPassword = CryptographyHelper.DecodeString(password);

                            smtpClient.Credentials = new NetworkCredential(login, decodedPassword);
                            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                            email.SmtpClient = smtpClient;

                            if (EmailHelper.AuthenticateClient(client, login, decodedPassword))
                            {
                                client.Behavior.ExamineFolders = false;
                                client.Behavior.FolderTreeBrowseMode = ImapX.Enums.FolderTreeBrowseMode.Lazy;

                                client.Folders.Inbox.Examine();
                                client.Folders.Inbox.Messages.Download(searchString,
                                    ImapX.Enums.MessageFetchMode.Body | ImapX.Enums.MessageFetchMode.Headers |
                                    ImapX.Enums.MessageFetchMode.InternalDate | ImapX.Enums.MessageFetchMode.Flags);
                                client.Folders.Inbox.OnNewMessagesArrived += Client_NewMessagesArrived;
                                client.Folders.Inbox.StartIdling();
                                email.ImapClient = client;
                            }
                        }
                        else
                        {
                            email.ImapClient = null;
                        }
                    }
                    else
                    {
                        email.ImapClient = null;
                    }
                }
                e.Result = emails;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void SmtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
        }

        private void _emailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                EmailClients.AddRange((List<EmailModel>) e.Result);
                Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");
                statusLabel.Content = "Zakończono pobieranie";

                Image loadingImage = (Image) LogicalTreeHelper.FindLogicalNode(this, "leftImage");
                ImageBehavior.SetAnimatedSource(loadingImage, null);

                List<Message> messages = new List<Message>();
                foreach (EmailModel email in EmailClients)
                {
                    if (email.ImapClient != null)
                    {
                        long maxUid = email.ImapClient.Folders.Inbox.Messages.Max(obj => obj.UId);
                        Client.SetLastDownloadedUid(email.Id, maxUid.ToString());
                    }
                }

                DoWorkEventHandler doWorkHandler =
                    delegate(object s, DoWorkEventArgs ev)
                    {
                        int unhandledMessagesCount = 0;

                        foreach (EmailModel email in EmailClients)
                        {
                            if (email.ImapClient != null)
                            {
                                if (
                                    email.ImapClient.Folders.Inbox.Messages.Any())
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        List<Message> unhandledMessages =
                                            ProcessMessages(email, email.ImapClient.Folders.Inbox.Messages.ToList());
                                        email.UnhandledMessages.AddRange(unhandledMessages);
                                    });

                                    Client.AddUnhandledMessages(email.Id, email.UnhandledMessagesIds);
                                    unhandledMessagesCount += email.UnhandledMessages.Count();
                                }
                            }
                        }
                        ev.Result = unhandledMessagesCount;
                    };

                RunWorkerCompletedEventHandler workerCompletedHandler = null;
                workerCompletedHandler =
                    delegate(object s, RunWorkerCompletedEventArgs ev)
                    {
                        int unhandledCount = (int) ev.Result;
                        if (unhandledCount > 0)
                        {
                            _unhandledNotification = new NotificationModel("", "", null,
                                DateTime.Now, false, true)
                            {Text = unhandledCount + " wiadomości wymaga Twojej uwagi"};

                            RaiseNotification(_unhandledNotification);
                        }
                        ProcessingWorker.DoWork -= doWorkHandler;
                        ProcessingWorker.RunWorkerCompleted -= workerCompletedHandler;
                    };

                ProcessingWorker.RunWorkerCompleted += workerCompletedHandler;
                ProcessingWorker.DoWork += doWorkHandler;

                ProcessingWorker.RunWorkerAsync(messages);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        #endregion

        #region NotificationWorker

        private void _notificationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<NotificationModel> notifications = Client.GetUserNotifications();

                foreach (NotificationModel notification in notifications)
                {
                    NotificationModel processedNotification = ProcessNotification(notification);

                    RaiseNotification(processedNotification, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void _notificationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<EmailModel> emails = Client.GetUserEmailAddresses();
            EmailWorker.RunWorkerAsync(emails);
        }

        #endregion

        #region ConversationWorker

        private void _conversationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");
                    statusLabel.Content = "Pobieranie konwersacji";
                });

                List<ConversationModel> conversations = Client.GetUserConversations();

                foreach (ConversationModel conversation in conversations)
                {
                    for (int i = 0; i < conversation.MemberIds.Count; i++)
                    {
                        PersonModel member = Persons.Find(obj => obj.Id == conversation.MemberIds[i]);
                        if (member != null)
                        {
                            conversation.Members.Add(member);
                            conversation.ColorDictionary.Add(member,
                                (Color) ColorConverter.ConvertFromString(conversation.MemberColors[i]));
                        }
                    }

                    List<ConversationMessageModel> messages = Client.GetConversationMessages(conversation.Id);

                    foreach (ConversationMessageModel message in messages)
                    {
                        conversation.AddMessage(HandleNewMessage(message, conversation));
                    }
                }

                e.Result = conversations;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void _conversationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
            List<ConversationModel> conversations = (List<ConversationModel>) e.Result;
            Conversations = conversations;

            NotificationWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        #endregion

        #region FileWorker

        private void FileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Label statusLabel = (Label) LogicalTreeHelper.FindLogicalNode(this, "leftLabel");
                    statusLabel.Content = "Pobieranie informacji o plikach";
                });

                Files = Client.GetFilesInfo();

                foreach (FileModel file in Files)
                {
                    if (file.ContentType != null)
                    {
                        file.Icon = FileHelper.GetFileIcon(file.ContentType);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void FileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ConversationWorker.RunWorkerAsync();
        }

        #endregion

        #endregion

        #region NotificationPanels

        private void JustInTimeNotificationPanel_NotificationClosed(object sender, EventArgs e)
        {
            try
            {
            NotificationItem notificationItem = (NotificationItem) sender;
            Client.ReportNotificationsRead(new List<NotificationModel> {notificationItem.Notification});
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationPanel_NotificationsRead(object sender, NotificationsReadEventArgs e)
        {
            try
            {
                Client.ReportNotificationsRead(e.Notifications);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private NotificationModel ProcessNotification(NotificationModel notification)
        {
            try
            {
                string ruleString = notification.RuleString;
                string processedString = ruleString;

                MatchCollection referenceMatches = Regex.Matches(ruleString, @"\{.[^{}]*\}");

                    for (int i = 0; i < referenceMatches.Count; i++)
                    {
                        Match referenceMatch = referenceMatches[i];
                        string referenceMatchString = referenceMatch.Value.Trim('{', '}');
                        string[] referenceParts = notification.ReferenceFields[i].Split('.');

                        object referenceObject = GetType().GetProperty(referenceParts[0]).GetValue(this);

                        List<object> referenceList = (referenceObject as IEnumerable<object>).Cast<object>().ToList();

                        object referenceInnerObject =
                            referenceList.Find(
                                obj =>
                                    obj.GetType().GetProperty("Id").GetValue(obj).ToString() ==
                                    referenceMatchString);
                    //
                        string referenceValue =
                            referenceInnerObject.GetType()
                                .GetProperty(referenceParts[1])
                                .GetValue(referenceInnerObject)
                                .ToString();

                        if (referenceInnerObject is PersonModel)
                        {
                            PersonModel person = (PersonModel) referenceInnerObject;
                            if (person.FirstName.Last() == 'a')
                            {
                                processedString = processedString.Replace("dodał", "dodała");
                            }
                        }

                        processedString = processedString.Replace(referenceMatchString, referenceValue, true);
                        notification.Text = processedString;
                    }

                return notification;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public void RaiseNotification(NotificationModel notification, bool silent = false)
        {
            try
            {
                this.Dispatcher.Invoke(delegate()
                {
                    _notificationPanel.AddNotification(notification);

                    if (silent && notification.Read == false)
                    {
                        _notificationPanel.BoundNotificationButton.UnreadNotificationsCount++;
                    }
                    if (!silent)
                    {
                        _justInTimeNotificationPanel.AddNotification(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        #endregion

        #region NotificationMethods

        private ConversationMessageModel HandleNewMessage(ConversationMessageModel message,
            ConversationModel conversation)
        {
            try
            {
                PersonModel author = Persons.Find(obj => obj.Id == message.AuthorId);

                if (author != null)
                {
                    if (author.IsInternalUser)
                        message.Received = false;
                    message.Color = conversation.ColorDictionary[author];

                    foreach (string attachmentId in message.AttachmentsIds)
                    {
                        message.Attachments.Add(Files.Find(obj => obj.Id == attachmentId));
                    }

                    switch (message.GetType().Name)
                    {
                        case "ConversationEmailMessageModel":
                            ConversationEmailMessageModel emailMessage = (ConversationEmailMessageModel) message;

                            EmailAddressModel emailAddress =
                                author.EmailAddresses.Find(obj => obj.Id == message.AuthorFrom);

                            if (emailAddress != null)
                            {
                                emailMessage.Author = author;
                                emailMessage.AuthorEmailaddress = emailAddress;
                            }

                            return emailMessage;
                        case "ConversationPhoneMessageModel":
                            ConversationPhoneMessageModel phoneMessage = (ConversationPhoneMessageModel) message;

                            foreach (PersonModel person in Persons)
                            {
                                PhoneNumberModel recipientPhoneNumber =
                                    person.PhoneNumbers.Find(obj => obj.Id == phoneMessage.RecipientPhoneNumberId);

                                if (recipientPhoneNumber != null)
                                {
                                    phoneMessage.RecipientPhoneNumber = recipientPhoneNumber;
                                    phoneMessage.Recipient = person;
                                    break;
                                }
                            }

                            PhoneNumberModel phoneNumber =
                                author.PhoneNumbers.Find(obj => obj.Id == message.AuthorFrom);

                            if (phoneNumber != null)
                            {
                                phoneMessage.Author = author;
                                phoneMessage.AuthorPhoneNumber = phoneNumber;
                            }

                            return phoneMessage;
                    }
                }
                return message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        #endregion

        #region Events

        private void Client_NewMessagesArrived(object sender, IdleEventArgs e)
        {
            try
            {
                EmailModel email = EmailClients.Find(obj => obj.ImapClient.Folders.Contains((Folder) sender));

                DoWorkEventHandler doWorkHandler =
                    delegate(object s, DoWorkEventArgs ev)
                    {
                        List<Message> messages = (List<Message>) ev.Argument;

                            List<Message> unhandledMessages = ProcessMessages(email, messages);
                        this.Dispatcher.Invoke(delegate ()
                        {
                            email.UnhandledMessages.AddRange(unhandledMessages);

                            NewUnhandledMessageArrived?.Invoke(this, new NewUnhandledMessageArrivedEventArgs()
                            {
                                Email = email,
                                Messages = unhandledMessages
                            });

                            Client.AddUnhandledMessages(email.Id, email.UnhandledMessagesIds);

                            if (unhandledMessages.Count > 0)
                            {
                                _notificationPanel.RemoveNotification(_unhandledNotification);
                                _unhandledNotification = new NotificationModel("", "", null,
                                    DateTime.Now, false, true)
                                {Text = email.UnhandledMessages.Count + " wiadomości wymaga Twojej uwagi"};

                                RaiseNotification(_unhandledNotification);
                            }
                        });
                    };

                RunWorkerCompletedEventHandler workerCompletedHandler = null;
                workerCompletedHandler =
                    delegate(object s, RunWorkerCompletedEventArgs ev)
                    {
                        ProcessingWorker.DoWork -= doWorkHandler;
                        ProcessingWorker.RunWorkerCompleted -= workerCompletedHandler;
                    };

                ProcessingWorker.DoWork += doWorkHandler;
                ProcessingWorker.RunWorkerCompleted += workerCompletedHandler;

                ProcessingWorker.RunWorkerAsync(e.Messages.ToList());
                long maxUid = e.Messages.Max(obj => obj.UId);
                Client.SetLastDownloadedUid(email.Id, maxUid.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void UpperMenuModeChanged(object sender, EventArgs e)
        {
            try
            {
                UpperMenuStrip upperMenu = (UpperMenuStrip) sender;

                switch (upperMenu.MenuMode)
                {
                    case UpperMenuMode.Document:
                        if (_documentWindow == null)
                            _documentWindow = new Document();
                        ContentArea.Content = _documentWindow;
                        if (!_documentWindow.WindowInitialized)
                            _documentWindow.Init();
                        break;
                    case UpperMenuMode.HomePage:
                        if (_homePageWindow == null)
                            _homePageWindow = new HomePage();

                        ContentArea.Content = _homePageWindow;
                        //if (!_homePageWindow.WindowInitialized)
                        //    _homePageWindow.Init();
                        break;
                    case UpperMenuMode.Conversation:
                        if (_conversationWindow == null)
                            _conversationWindow = new Conversation();

                        ContentArea.Content = _conversationWindow;
                        if (!_conversationWindow.WindowInitialized)
                            _conversationWindow.Init();
                        break;
                    case UpperMenuMode.Email:
                        if (_emailWindow == null)
                            _emailWindow = new Email();

                        ContentArea.Content = _emailWindow;
                        if (!_emailWindow.WindowInitialized)
                            _emailWindow.Init();
                        break;
                    case UpperMenuMode.Contact:
                        if (_contactWindow == null)
                            _contactWindow = new Contact();

                        ContentArea.Content = _contactWindow;
                        if (!_contactWindow.WindowInitialized)
                            _contactWindow.Init();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public virtual void OnLoginStatusChanged(LoginStatusChangedEventArgs e)
        {
            try
            {
                LoginStatusChanged?.Invoke(this, e);
                if (e.LoggedIn)
                {
                    //Canvas canvas = (Canvas)LogicalTreeHelper.FindLogicalNode(this, "mainCanvas");
                    //canvas.Children.Add(new Scanning(this));

                    ContactWorker = new BackgroundWorker();
                    ContactWorker.DoWork += _contactWorker_DoWork;
                    ContactWorker.RunWorkerCompleted += _contactWorker_RunWorkerCompleted;
                    ContactWorker.RunWorkerAsync();

                    EmailWorker = new BackgroundWorker();
                    EmailWorker.DoWork += _emailWorker_DoWork;
                    EmailWorker.RunWorkerCompleted += _emailWorker_RunWorkerCompleted;

                    ConversationWorker = new BackgroundWorker();
                    ConversationWorker.DoWork += _conversationWorker_DoWork;
                    ConversationWorker.RunWorkerCompleted += _conversationWorker_RunWorkerCompleted;

                    NotificationWorker = new BackgroundWorker();
                    NotificationWorker.DoWork += _notificationWorker_DoWork;
                    NotificationWorker.RunWorkerCompleted += _notificationWorker_RunWorkerCompleted;

                    ProcessingWorker = new BackgroundWorker();

                    FileWorker = new BackgroundWorker();
                    FileWorker.DoWork += FileWorker_DoWork;
                    FileWorker.RunWorkerCompleted += FileWorker_RunWorkerCompleted;

                    NotificationClient.NewConversationMessageArrived += NotificationClient_NewConversationMessageArrived;
                    NotificationClient.NewFilesArrived += NotificationClient_NewFilesArrived;
                    NotificationClient.ConversationRenamed += NotificationClient_ConversationRenamed;
                    NotificationClient.FileRenamed += NotificationClient_FileRenamed;
                    NotificationClient.NewConversationArrived += NotificationClient_NewConversationArrived;
                    NotificationClient.ConversationMembersAdded += NotificationClient_ConversationMembersAdded;
                    NotificationClient.ConversationMemberRemoved += NotificationClient_ConversationMemberRemoved;
                    NotificationClient.ConversationRemoved += NotificationClient_ConversationRemoved;
                    NotificationClient.ConversationSettingsChanged += NotificationClient_ConversationSettingsChanged;
                    NotificationClient.NewCompanyArrived += NotificationClient_NewCompanyArrived;
                    NotificationClient.CompanyRenamed += NotificationClient_CompanyRenamed;
                    NotificationClient.NewEmailAddress += NotificationClient_NewEmailAddress;
                    NotificationClient.CompanyRemoved += NotificationClient_CompanyRemoved;
                    NotificationClient.ContactDetailsUpdated += NotificationClient_ContactDetailsUpdated;

                    _notificationPanel = new NotificationsPanel(mainCanvas.ActualWidth,
                        mainCanvas.ActualHeight - 60);
                    _notificationPanel.NotificationsRead += NotificationPanel_NotificationsRead;
                    _notificationPanel.BoundNotificationButton = UpperMenu.NotificationButton;

                    Canvas.SetTop(_notificationPanel, 40);
                    mainCanvas.Children.Add(_notificationPanel);

                    _justInTimeNotificationPanel = new JustInTimeNotificationPanel();
                    _justInTimeNotificationPanel.NotificationClosed += JustInTimeNotificationPanel_NotificationClosed;
                    _notificationPanel.BoundJustInTimeNotificationPanel = _justInTimeNotificationPanel;

                    mainCanvas.Children.Add(_justInTimeNotificationPanel);

                    _homePageWindow = new HomePage();

                    ContentArea.Content = _homePageWindow;
                    UpperMenu.MenuMode = UpperMenuMode.HomePage;

                    UploadClient.FileUploaded += UploadClient_FileUploaded;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_ContactDetailsUpdated(object sender, ContactDetailsUpdatedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PersonModel person = Persons.Find(obj => obj.Id == e.NewData.Id);

                if (person.FirstName != e.NewData.FirstName)
                    person.FirstName = e.NewData.FirstName;

                if (person.LastName != e.NewData.LastName)
                    person.LastName = e.NewData.LastName;

                if (person.Company.Id != e.NewData.CompanyId)
                {
                    person.Company = Companies.Find(obj => obj.Id == e.NewData.CompanyId);
                }

                if (person.Gender != e.NewData.Gender)
                    person.Gender = e.NewData.Gender;

                if (person.EmailAddresses != e.NewData.EmailAddresses)
                    person.EmailAddresses = e.NewData.EmailAddresses;

                if (person.PhoneNumbers != e.NewData.PhoneNumbers)
                    person.PhoneNumbers = e.NewData.PhoneNumbers;

                person.OnDataChanged();

                NotificationModel notification = ProcessNotification(e.Notification);
                RaiseNotification(notification);
            });
        }

        private void NotificationClient_CompanyRemoved(object sender, CompanyRemovedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Persons.Where(obj => obj.Company == Companies.Find(obj2 => obj2.Id == e.CompanyId))
                    .ToList()
                    .ForEach(obj => obj.Company = null);

                NotificationModel notification = ProcessNotification(e.Notification);
                RaiseNotification(notification);

                Companies.Find(obj => obj.Id == e.CompanyId).Name = "";
                Companies.Remove(Companies.Find(obj => obj.Id == e.CompanyId));

                CompanyRemoved?.Invoke(sender, e);
            });
        }

        private void NotificationClient_NewEmailAddress(object sender, NewEmailAddressEventArgs e)
        {
            EmailModel email = new EmailModel(e.Id, e.Address, e.Login, e.ImapHost, e.ImapPort, e.ImapUseSsl, e.SmtpHost,
                e.SmtpPort, e.SmtpUseSsl, new List<string>(), null);

            Dispatcher.Invoke(() =>
            {
                EmailWorker.RunWorkerAsync(new List<EmailModel>() {email});
            });

            RunWorkerCompletedEventHandler completed = null;

            completed = (s, ea) =>
            {
                NewEmailAddress?.Invoke(this, e);
                EmailWorker.RunWorkerCompleted -= completed;
            };

            EmailWorker.RunWorkerCompleted += completed;

        }

        private void NotificationClient_CompanyRenamed(object sender, CompanyRenamedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Companies.Find(obj => obj.Id == e.CompanyId).Name = e.NewName;
                CompanyRenamed?.Invoke(sender, e);

                NotificationModel notification = ProcessNotification(e.Notification);

                RaiseNotification(notification);
            });
        }

        private void NotificationClient_NewCompanyArrived(object sender, NewCompanyEventArgs e)
        {
            CompanyModel company = e.Company;
            Companies.Add(company);

            this.Dispatcher.Invoke(() => {
                                             NewCompanyArrived?.Invoke(this, e);
            });

            NotificationModel notification = e.Notification;
            notification = ProcessNotification(notification);
            RaiseNotification(notification);
        }

        private void NotificationClient_ConversationSettingsChanged(object sender, ConversationSettingsChangedEventArgs e)
        {
            try
            {
                ConversationModel conversation = Conversations.Find(obj => obj.Id == e.ConversationId);

                conversation.NotifyContactPersons = e.NotifyContactPersons;

                if (conversation != null)
                {
                    NotificationModel notification = e.Notification;
                    notification = ProcessNotification(notification);
                    RaiseNotification(notification);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_ConversationRemoved(object sender, ConversationRemovedEventArgs e)
        {
            try
            {
                ConversationModel conversation = Conversations.Find(obj => obj.Id == e.ConversationId);

                if (conversation != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ConversationRemoved?.Invoke(this, e);
                    });

                    RaiseNotification(new NotificationModel("", "", null,
                        DateTime.Now, false, true)
                    {Text = "Usunięto Cię z konwersacji " + conversation.Name});
                    Conversations.Remove(conversation);
                    Files.RemoveAll(obj => obj.ConversationId == conversation.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_ConversationMembersAdded(object sender, ConversationMembersAddedEventArgs e)
        {
            try
            {
                if (e.PersonId != Client.UserInfo.PersonId)
                {
                    ConversationModel conversation = Conversations.Find(obj => obj.Id == e.ConversationId);
                    PersonModel person = Persons.Find(obj => obj.Id == e.PersonId);
                    conversation.Members.Add(person);
                    conversation.ColorDictionary.Add(person, (Color) ColorConverter.ConvertFromString(e.PersonColor));
                }

                this.Dispatcher.Invoke(() =>
                {
                    NewconversationMembers?.Invoke(this, e);
                });

                NotificationModel notification = e.Notification;
                notification = ProcessNotification(notification);
                RaiseNotification(notification);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_ConversationMemberRemoved(object sender, ConversationMemberRemovedEventArgs e)
        {
            try
            {
            ConversationModel conversation = Conversations.Find(obj => obj.Id == e.ConversationId);
            PersonModel person = conversation.Members.Find(obj => obj.Id == e.PersonId);
            conversation.Members.Remove(person);
            conversation.MemberIds.Remove(e.PersonId);
            conversation.ColorDictionary.Remove(person);
            this.Dispatcher.Invoke(() =>
            {
                ConversationMemberRemoved?.Invoke(this, e);
            });

            NotificationModel notification = e.Notification;
                notification = ProcessNotification(notification);
                RaiseNotification(notification);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_NewConversationArrived(object sender, NewConversationArrivedEventArgs e)
        {
            try
            {
                for (int i = 0; i < e.Conversation.MemberIds.Count; i++)
                {
                    PersonModel member = Persons.Find(obj => obj.Id == e.Conversation.MemberIds[i]);
                    if (member != null)
                    {
                        e.Conversation.Members.Add(member);
                        e.Conversation.ColorDictionary.Add(member,
                            (Color) ColorConverter.ConvertFromString(e.Conversation.MemberColors[i]));
                    }
                }

                List<ConversationMessageModel> handledMessages = new List<ConversationMessageModel>();
                e.Conversation.Messages.ForEach(obj => handledMessages.Add(HandleNewMessage(obj, e.Conversation)));
                e.Conversation.Messages.Clear();
                e.Conversation.Messages = handledMessages;

                Conversations.Add(e.Conversation);

                this.Dispatcher.Invoke(() =>
                {
                    NewConversationArrived?.Invoke(this, e);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void UploadClient_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            try
            {
                if (AwaitingMessages.Contains(e.Message))
                {
                    if (AwaitingMessages.Find(obj => obj == e.Message).Attachments.All(obj => obj.Id != null))
                    {
                        Client.AddNewMessage(e.Message.ConversationId, e.Message);
                        AwaitingMessages.Remove(e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_NewConversationMessageArrived(object sender,
            NewConversationMessageArrivedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(delegate()
                {
                    ConversationModel conversation = Conversations.Find(obj => obj.Id == e.Message.ConversationId);

                    ConversationMessageModel handledMessage = HandleNewMessage(e.Message, conversation);
                    conversation.AddMessage(handledMessage);

                    NewMessageArrived?.Invoke(this,
                        new NewConversationMessageArrivedEventArgs() {Message = handledMessage});

                    NotificationModel notification = e.Notification;
                    notification = ProcessNotification(notification);
                    RaiseNotification(notification);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_NewFilesArrived(object sender, NewFilesArrivedEventArgs e)
        {
            try
            {
                List<FileModel> filesToAdd = new List<FileModel>();
                foreach (FileModel file in e.Files)
                {
                    FileModel newFile =
                        Files
                            .FirstOrDefault(obj => obj.Id == file.Id && obj.ConversationId == file.ConversationId);
                    if (newFile == null)
                    {
                        filesToAdd.Add(file);
                    }
                }
                Files.AddRange(filesToAdd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_ConversationRenamed(object sender, ConversationRenamedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Conversations.Find(obj => obj.Id == e.ConversationId).Name = e.NewName;
                    NotificationModel notification = e.Notification;
                    notification = ProcessNotification(notification);
                    RaiseNotification(notification);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void NotificationClient_FileRenamed(object sender, FileRenamedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Files.Find(obj => obj.Name == e.OldName).Name = e.NewName;
                    NotificationModel notification = e.Notification;
                    notification = ProcessNotification(notification);
                    RaiseNotification(notification);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        #endregion

        #region Methods

        private List<Message> ProcessMessages(EmailModel emailAddress, List<Message> messages)
        {
            try
            {
                List<Message> unhandledMessages = new List<Message>();
                List<string> unhandledMessageIds = new List<string>();
                foreach (Message message in messages)
                {
                    if (emailAddress.UnhandledMessagesIds.Contains(message.UId.ToString()))
                    {
                        unhandledMessages.Add(message);
                        continue;
                    }

                    if (emailAddress.LastUid == message.UId.ToString())
                        continue;

                    MatchCollection matches = Regex.Matches(message.Subject, @"\(\d{8}\)");

                    if (matches.Count == 1)
                    {
                        string receivedNumber = matches[0].Value.Replace("(", "").Replace(")", "");

                        string conversationId = Client.CheckConversationExists(receivedNumber);

                        PersonModel messageSender =
                            Persons.Find(obj => obj.EmailAddresses.Any(obj2 => obj2.Address == message.From.Address));

                        if (conversationId != null)
                        {
                            if (messageSender != null)
                            {
                                EmailAddressModel authorEmailAddress =
                                    messageSender.EmailAddresses.Single(obj => obj.Address == message.From.Address);

                                ConversationMessageModel receivedMessage =
                                    new ConversationMessageModel(messageSender,
                                        message.InternalDate)
                                    {
                                        ConversationId = conversationId
                                    };

                                ConversationEmailMessageModel emailMessage =
                                    new ConversationEmailMessageModel(receivedMessage, authorEmailAddress,
                                        message.Subject,
                                        message.Body.Html == "" ? message.Body.Text.Replace("\r\n","<br>") : message.Body.Html);                            

                                BitmapSource previewImage = null;

                                Dispatcher.Invoke(() =>
                                {
                                    previewImage =
                                        ImageHelper.GetHtmlImagePreview(
                                            message.Body.Html == "" ? message.Body.Text.Replace("\r\n", "<br>") : message.Body.Html,
                                            new Size(600, 60), new Size(600, 250));

                                    previewImage.Freeze();
                                });

                                emailMessage.PreviewImage = previewImage;

                                if (message.Attachments.Any())
                                {
                                    AwaitingMessages.Add(emailMessage);
                                    foreach (Attachment attachment in message.Attachments)
                                    {
                                        if (!attachment.Downloaded)
                                            attachment.Download();

                                        FileModel file = new FileModel(attachment, DateTime.Today);

                                        emailMessage.Attachments.Add(file);
                                    }
                                    UploadClient.UploadFiles(emailMessage, emailMessage.Attachments);
                                }
                                else
                                {
                                    Client.AddNewMessage(conversationId, emailMessage);
                                }
                            }
                        }
                        else
                        {
                            unhandledMessages.Add(message);
                            unhandledMessageIds.Add(message.UId.ToString());
                        }
                    }
                    else
                    {
                        unhandledMessages.Add(message);
                        unhandledMessageIds.Add(message.UId.ToString());
                    }
                }
                emailAddress.UnhandledMessagesIds = unhandledMessageIds;
                return unhandledMessages;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        #endregion
    }

    #region EventArgs

    public class LoginStatusChangedEventArgs : EventArgs
    {
        public bool LoggedIn { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class NewUnhandledMessageArrivedEventArgs
    {
        public EmailModel Email { get; set; }
        public List<Message> Messages { get; set; }
    }


    #endregion

    public enum NotificationType
    {
        UnhandledMessagesPending,
    }
}