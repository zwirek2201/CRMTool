using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;
using Licencjat_new.Server;
using Licencjat_new.Windows.HelperWindows;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class Conversation
    {
        #region Variables
        private MainWindow _parent;
        private List<ConversationModel> _conversations;
        private List<PersonModel> _filteredPersons = new List<PersonModel>();
        private NewConversationMessagePanelButton _newEmailButton;
        private NewConversationMessagePanelButton _newPhoneButton;

        BackgroundWorker _sendingWorker;
        #endregion

        #region Properties
        public ConversationModel SelectedConversation { get; set; }
        public bool WindowInitialized { get; private set; }
        #endregion

        #region Constructors
        public Conversation()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization

        public void Init()
        {
            _parent = (MainWindow) Window.GetWindow(this);
            _sendingWorker = new BackgroundWorker();

            ConversationList.SelectedConversationChanged += ConversationList_SelectedConversationChanged;

            if (_parent != null && (_parent.Conversations == null || _parent.Conversations.Count == 0))
            {
                _parent.ConversationWorker.RunWorkerCompleted += _conversationWorker_RunWorkerCompleted;
            }
            else
            {
                Console.Write(_parent?.Conversations);
                Console.Write(_parent?.Conversations.Count);

                if (_parent?.Conversations != null)
                {
                    _conversations = _parent.Conversations;
                    LoadConversations();
                }
            }

            _parent.NewMessageArrived += _parent_NewMessageArrived;
            _parent.NewConversationArrived += _parent_NewConversationArrived;
            _parent.NewconversationMembers += _parent_NewconversationMembers;
            _parent.ConversationMemberRemoved += _parent_ConversationMemberRemoved;
            _parent.ConversationRemoved += _parent_ConversationRemoved;

            MessageList.BoundConversationList = ConversationList;
            //MemberList.Visibility = Visibility.Collapsed;

            ConversationListContainer.AddConversation += ConversationListContainer_AddConversation;

            ConversationList.RemoveConversation += ConversationList_RemoveConversation;
            ConversationList.RenameConversation += ConversationList_RenameConversation;
            ConversationList.ShowConversationDetails += ConversationList_ShowConversationDetails;

            MemberList.AddFilter += MemberList_AddFilter;
            MemberList.RemoveFilter += MemberList_RemoveFilter;
            MemberList.ClearFilter += MemberList_ClearFilter;
            MemberList.AddMember += MemberList_AddMember;
            MemberList.RemoveMember += MemberList_RemoveMember;

            MessageList.DownloadAllAttachments += MessageList_DownloadAllAttachments;
            MessageList.RenameFile += MessageList_RenameFile;
            MessageList.DownloadFile += MessageList_DownloadFile;
            MessageList.ShowMessageDetails += MessageList_ShowMessageDetails;

            _newEmailButton =
                new NewConversationMessagePanelButton(
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/addEmailMessage.png")),
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/addEmailMessage_hover.png")),
                    "Nowa wiadomość e-mail");


            _newEmailButton.Click += NewEmailButton_Click;

            _newPhoneButton =
                new NewConversationMessagePanelButton(
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/addPhoneMessage.png")),
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/addPhoneMessage_hover.png")),
                    "Nowa rozmowa telefoniczna");

            _newPhoneButton.Click += _newPhoneButton_Click;

            NewMessagePanel.AddButton(_newEmailButton);
            NewMessagePanel.AddButton(_newPhoneButton);

            _newPhoneButton.Visibility = Visibility.Collapsed;

            if (_parent.Conversations == null)
            {
                _parent.ConversationWorker.RunWorkerCompleted += (s, ea) =>
                {
                    if (_parent.Conversations.Count > 0)
                        _newPhoneButton.Visibility = Visibility.Visible;
                };
            }
            else
            {
                _newPhoneButton.Visibility = Visibility.Visible;
            }

            _newEmailButton.Visibility = Visibility.Collapsed;

            if (_parent.EmailClients == null)
            {
                _parent.EmailWorker.RunWorkerCompleted += (s, ea) =>
                {
                    if (_parent.EmailClients.Count > 0)
                        _newEmailButton.Visibility = Visibility.Visible;
                };
            }
            else
            {
                if (_parent.EmailClients.Count > 0)
                    _newEmailButton.Visibility = Visibility.Visible;
            }

            WindowInitialized = true;
        }

        private void _newPhoneButton_Click(object sender, EventArgs e)
        {
            ConversationModel conversation = ConversationList.SelectedConversation;

            if (conversation.Members.Where(obj => !obj.IsInternalUser).Any(obj => obj.PhoneNumbers.Count > 0))
            {
            _parent.Darkened = true;

            NewPhoneConversationMessage newMessage = new NewPhoneConversationMessage(_parent, conversation);
            _parent.mainCanvas.Children.Add(newMessage);

            newMessage.ReadyButtonClicked += (s, ea) =>
            {
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(newMessage);

                DoWorkEventHandler doWorkHandler =
                    delegate(object se, DoWorkEventArgs ev)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                _parent.StatusBar.StatusText = "Wysyłanie wiadomości";
                            });

                            ConversationMessageModel message =
                                new ConversationMessageModel(newMessage.MessageType == PhoneMessageType.ReceivedCall ? newMessage.SelectedPerson : _parent.Persons.Find(obj => obj.Id == _parent.Client.UserInfo.PersonId), DateTime.Now);

                            message.PreviewImage = ImageHelper.GetHtmlImagePreview(
                                newMessage.Message.Replace("\r\n", "<br>"),
                                new Size(600, 60), new Size(600, 250));

                            message.PreviewImage.Freeze();

                            ConversationPhoneMessageModel phoneMessage = new ConversationPhoneMessageModel(message,
                                newMessage.MessageType == PhoneMessageType.ReceivedCall
                                    ? newMessage.SelectedPhoneNumber
                                    : _parent.Persons.Find(obj => obj.Id == _parent.Client.UserInfo.PersonId)
                                        .PhoneNumbers.First(),
                                newMessage.MessageType != PhoneMessageType.ReceivedCall
                                    ? newMessage.SelectedPhoneNumber
                                    : _parent.Persons.Find(obj => obj.Id == _parent.Client.UserInfo.PersonId)
                                        .PhoneNumbers.First(), newMessage.Message,
                                newMessage.CallAnswered);

                            foreach (FileModel file in newMessage.Attachments)
                            {
                                FileModel newFile = new FileModel(file.Id, file.Name, file.ContentType, file.Size,
                                    file.DateAdded)
                                {ConversationId = conversation.Id, Data = file.Data};

                                phoneMessage.Attachments.Add(newFile);
                            }

                            EventHandler<FileUploadedEventArgs> fileUploadedEventHandler = null;
                            fileUploadedEventHandler = delegate(object s2, FileUploadedEventArgs ea2)
                            {
                                if (phoneMessage.Attachments.All(obj => obj.Id != null))
                                {
                                    _parent.Client.AddNewMessage(conversation.Id, phoneMessage);
                                    _parent.UploadClient.FileUploaded -= fileUploadedEventHandler;
                                }
                            };

                            if (!phoneMessage.Attachments.Any(obj => obj.Id == null))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    _parent.Client.AddNewMessage(conversation.Id, phoneMessage);
                                });
                            }
                            else
                            {
                                _parent.UploadClient.FileUploaded += fileUploadedEventHandler;
                                _parent.UploadClient.UploadFiles(phoneMessage,
                                    phoneMessage.Attachments.Where(obj => obj.Id == null).ToList());
                            }


                            this.Dispatcher.Invoke(() =>
                            {
                                _parent.StatusBar.StatusText = "Wysyłano wiadomość";
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                            ErrorHelper.LogError(ex);
                        }
                    };

                _sendingWorker.DoWork += doWorkHandler;

                RunWorkerCompletedEventHandler runWorkerCompleted = null;

                runWorkerCompleted = delegate(object s2, RunWorkerCompletedEventArgs ev)
                {
                    _sendingWorker.DoWork -= doWorkHandler;
                    _sendingWorker.RunWorkerCompleted -= runWorkerCompleted;
                };

                _sendingWorker.RunWorkerCompleted += runWorkerCompleted;

                _sendingWorker.RunWorkerAsync();

                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(newMessage);
            };

            newMessage.CancelButtonClicked += (s, ea) =>
            {
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(newMessage);

            };
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    _parent.Darkened = true;
                    CustomMessageBox messageBox =
                        new CustomMessageBox(
                            "Nie można utworzyć wiadomości. Żaden z członków konwersacji nie posiada numeru telefonu.",
                            MessageBoxButton.OK);

                    messageBox.OKButtonClicked += (s2, ea2) =>
                    {
                        _parent.mainCanvas.Children.Remove(messageBox);
                        _parent.Darkened = false;

                    };

                    _parent.mainCanvas.Children.Add(messageBox);
                });
            }
        }

        private void NewEmailButton_Click(object sender, EventArgs e)
        {
            ConversationModel conversation = ConversationList.SelectedConversation;

            if (conversation.Members.Where(obj => !obj.IsInternalUser).Any(obj => obj.EmailAddresses.Count > 0))
            {
                _parent.Darkened = true;

            NewEmailMessage newMessage = new NewEmailMessage(conversation, _parent);

            newMessage.CancelButtonClicked += (s, ea) =>
            {
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(newMessage);
            };

            newMessage.ReadyButtonClicked += (s, ea) =>
            {
                    DoWorkEventHandler doWorkHandler =
                        delegate(object se, DoWorkEventArgs ev)
                        {
                            try
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    _parent.StatusBar.StatusText = "Wysyłanie wiadomości";
                                });
                                SmtpClient client =
                                    _parent.EmailClients.Find(obj => obj.Address == newMessage.SendingAddress)
                                        .SmtpClient;

                                MailMessage mailMessage = new MailMessage();

                                mailMessage.From = new MailAddress(newMessage.SendingAddress);
                                mailMessage.Subject = newMessage.OutputSubject;
                                mailMessage.Body = newMessage.Message;

                                foreach (PersonModel recipient in conversation.Members)
                                {
                                    if (!recipient.IsInternalUser)
                                    {
                                        recipient.EmailAddresses.ForEach(obj => mailMessage.To.Add(new MailAddress(obj.Address)));
                                    }
                                }

                                foreach (FileModel file in newMessage.Attachments)
                                {
                                    mailMessage.Attachments.Add(new Attachment(new MemoryStream(file.Data), file.Name));
                                }

                                client.Send(mailMessage);

                                ConversationMessageModel message =
                                    new ConversationMessageModel(
                                        _parent.Persons.Find(obj => obj.Id == _parent.Client.UserInfo.PersonId),
                                        DateTime.Now);

                                message.PreviewImage = ImageHelper.GetHtmlImagePreview(
                                    newMessage.Message.Replace("\r\n", "<br>"),
                                    new Size(600, 60), new Size(600, 250));

                                message.PreviewImage.Freeze();

                                ConversationEmailMessageModel emailMessage = new ConversationEmailMessageModel(message,
                                    _parent.Persons.Find(obj => obj.Id == _parent.Client.UserInfo.PersonId)
                                        .EmailAddresses.Find(obj => obj.Address == newMessage.SendingAddress),
                                    newMessage.OutputSubject, newMessage.Message);

                                foreach (FileModel file in newMessage.Attachments)
                                {
                                    FileModel newFile = new FileModel(file.Id, file.Name, file.ContentType, file.Size,
                                        file.DateAdded) {ConversationId = conversation.Id, Data = file.Data};

                                    emailMessage.Attachments.Add(newFile);
                                }

                                EventHandler<FileUploadedEventArgs> fileUploadedEventHandler = null;
                                fileUploadedEventHandler = delegate(object s2, FileUploadedEventArgs ea2)
                                {
                                    if (emailMessage.Attachments.All(obj => obj.Id != null))
                                    {
                                        _parent.Client.AddNewMessage(conversation.Id, emailMessage);
                                        _parent.UploadClient.FileUploaded -= fileUploadedEventHandler;
                                    }
                                };

                                if (!emailMessage.Attachments.Any(obj => obj.Id == null))
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        _parent.Client.AddNewMessage(conversation.Id, emailMessage);
                                    });
                                }
                                else
                                {
                                    _parent.UploadClient.FileUploaded += fileUploadedEventHandler;
                                    _parent.UploadClient.UploadFiles(emailMessage,
                                        emailMessage.Attachments.Where(obj => obj.Id == null).ToList());
                                }


                                this.Dispatcher.Invoke(() =>
                                {
                                    _parent.StatusBar.StatusText = "Wysyłano wiadomość";
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                                ErrorHelper.LogError(ex);
                            }
                        };

                    _sendingWorker.DoWork += doWorkHandler;

                    RunWorkerCompletedEventHandler runWorkerCompleted = null;

                    runWorkerCompleted = delegate(object s2, RunWorkerCompletedEventArgs ev)
                    {
                        _sendingWorker.DoWork -= doWorkHandler;
                        _sendingWorker.RunWorkerCompleted -= runWorkerCompleted;
                    };

                    _sendingWorker.RunWorkerCompleted += runWorkerCompleted;

                    _sendingWorker.RunWorkerAsync();

                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(newMessage);
            };

            _parent.mainCanvas.Children.Add(newMessage);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    _parent.Darkened = true;
                    CustomMessageBox messageBox =
                        new CustomMessageBox(
                            "Nie można wysłać wiadomości. Żaden z członków konwersacji nie posiada adresu e-mail.",
                            MessageBoxButton.OK);

                    messageBox.OKButtonClicked += (s2, ea2) =>
                    {
                        _parent.mainCanvas.Children.Remove(messageBox);
                        _parent.Darkened = false;

                    };

                    _parent.mainCanvas.Children.Add(messageBox);
                });
            }
        }

        private void MessageList_ShowMessageDetails(object sender, EventArgs e)
        {
            _parent.Darkened = true;

            ConversationMessageListItem message = (ConversationMessageListItem)sender;

            ConversationMessageDetails details = new ConversationMessageDetails(message.Message);
            details.CloseButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(details);
                _parent.Darkened = false;
            };
            _parent.mainCanvas.Children.Add(details);
        }

        private void ConversationList_ShowConversationDetails(object sender, EventArgs e)
        {
            _parent.Darkened = true;
            ConversationDetails details = new ConversationDetails(ConversationList.SelectedConversation);
            details.CloseButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(details);
                _parent.Darkened = false;
            };
            _parent.mainCanvas.Children.Add(details);
        }

        private void ConversationListContainer_AddConversation(object sender, EventArgs e)
        {
            _parent.Darkened = true;
            Rename addConversation = new Rename();
            addConversation.CancelButtonClicked += (s, ea) =>
            {
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(addConversation); 
            };

            addConversation.ReadyButtonClicked += (s, ea) =>
            {
                _parent.Client.AddNewConversation(addConversation.NewName);
                _parent.Darkened = false;
                _parent.mainCanvas.Children.Remove(addConversation);
            };
            _parent.mainCanvas.Children.Add(addConversation);
        }

        private void MessageList_DownloadFile(object sender, EventArgs e)
        {
            FileListItem fileItem = (FileListItem)sender;

            if (fileItem.File.Data == null)
            {
                EventHandler<FileDownloadedEventArgs> eventDelegate = null;

                eventDelegate = (s, args) =>
                {
                    if (fileItem.File.Downloaded)
                    {
                        DownloadHelper.DownloadFile(fileItem.File, "C://Users/Marcin/Documents");

                        _parent.RaiseNotification(new NotificationModel("", "", null,
                            DateTime.Now, false, true)
                        {Text = "Plik został pobrany"});

                        _parent.DownloadClient.FileDownloaded -= eventDelegate;
                    }
                };
                _parent.DownloadClient.FileDownloaded += eventDelegate;
                _parent.DownloadClient.DownloadQueue.Add(fileItem.File);
            }
            else
            {
                DownloadHelper.DownloadFile(fileItem.File, "C://Users/Marcin/Documents");

                _parent.RaiseNotification(new NotificationModel("", "", null,
                    DateTime.Now, false, true)
                { Text = "Plik został pobrany" });
            }
        }

        private void MessageList_RenameFile(object sender, System.EventArgs e)
        {
            FileListItem item = (FileListItem)sender;

            _parent.Darkened = true;

            Rename renameWindow = new Rename();

            renameWindow.CancelButtonClicked += (o, args) =>
            {
                _parent.mainCanvas.Children.Remove(renameWindow);
                _parent.Darkened = false;
            };

            renameWindow.ReadyButtonClicked += (s, ev) =>
            {
                try
                {
                    renameWindow.LoadingOn = true;

                    _parent.Client.RenameFile(item.File.Id, item.File.Name,
                        renameWindow.NewName + item.File.Name.Substring(item.File.Name.LastIndexOf('.')));

                    renameWindow.LoadingOn = false;

                    _parent.mainCanvas.Children.Remove(renameWindow);
                    _parent.Darkened = false;
                }
                catch (Exception ex)
                {
                    
                }
            };

            _parent.mainCanvas.Children.Add(renameWindow);
        }

        private void MessageList_DownloadAllAttachments(object sender, System.EventArgs e)
        {
            ConversationMessageListItem item = (ConversationMessageListItem)sender;

            EventHandler<FileDownloadedEventArgs> eventDelegate = null;

            eventDelegate = (s, args) =>
            {
                if (item.Message.Attachments.All(obj => obj.Downloaded))
                {
                    MemoryStream archiveStream = DownloadHelper.ZipFiles(item.Message.Attachments);

                    using (FileStream fileStream = File.Create("C://Users/Marcin/Desktop/Zalaczniki.zip"))
                    {
                        archiveStream.Seek(0, SeekOrigin.Begin);
                        archiveStream.CopyTo(fileStream);
                    }

                    archiveStream.Position = 0;
                    archiveStream.Close();
                    archiveStream.Dispose();

                    _parent.RaiseNotification(new NotificationModel("", "", null,
                                DateTime.Now, false, true)
                    { Text = item.Message.Attachments.Count + " załączników zostało pobranych"});

                    _parent.DownloadClient.FileDownloaded -= eventDelegate;
                }
            };
            _parent.DownloadClient.FileDownloaded += eventDelegate;
            _parent.DownloadClient.DownloadQueue.AddRange(item.Message.Attachments);
        }
        #endregion

        #region Events
        private void _parent_NewconversationMembers(object sender, ConversationMembersAddedEventArgs e)
        {
            ConversationList.SelectedConversation.OnDataChanged();
            if (ConversationList.SelectedConversation.Id == e.ConversationId)
            {
                PersonModel person = _parent.Persons.Find(obj => obj.Id == e.PersonId);
                MemberList.AddMemberToList(person,
                    ConversationList.SelectedConversation.ColorDictionary[person]);

                if (MemberList.Members.Count > 1)
                {
                    _newPhoneButton.Visibility = Visibility.Visible;

                    if(_parent.EmailClients != null && _parent.EmailClients.Any(obj => obj.ImapClient != null))
                        _newEmailButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void _parent_ConversationMemberRemoved(object sender, ConversationMemberRemovedEventArgs e)
        {
            ConversationList.SelectedConversation.OnDataChanged();
            if (ConversationList.SelectedConversation.Id == e.ConversationId)
            {
                PersonModel person = _parent.Persons.Find(obj => obj.Id == e.PersonId);
                MemberList.RemoveMemberFromList(person);

                if (MemberList.Members.Count == 1)
                {
                    if (_newEmailButton != null)
                    {
                        _newEmailButton.Visibility = Visibility.Collapsed;
                        _newPhoneButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void _parent_ConversationRemoved(object sender, ConversationRemovedEventArgs e)
        {
            ConversationModel conversation = _conversations.Find(obj => obj.Id == e.ConversationId);
            if (ConversationList.SelectedConversation.Id == e.ConversationId)
                ConversationList.SelectedConversation = ConversationList.Conversations.First().Conversation;

            ConversationList.RemoveConversationFromList(
                ConversationList.Conversations.Find(obj => obj.Conversation == conversation));
        }

        private void _parent_NewMessageArrived(object sender, Server.NewConversationMessageArrivedEventArgs e)
        {
            if (SelectedConversation != null && e.Message.ConversationId == SelectedConversation.Id)
            {
                MessageList.ClearMessages();
                MessageList.AddMessages(SelectedConversation.Messages);
            }
        }

        private void _conversationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _conversations = _parent?.Conversations;
            LoadConversations();
            ConversationList.SelectedConversation = ConversationList.Conversations[0].Conversation;
        }

        private void ConversationList_SelectedConversationChanged(object sender, SelectedConversationChangedEventArgs e)
        {
              SelectedConversation = e.Conversation;

            if (MessageList == null) return;

            MessageList.ClearMessages();
            MessageList.AddMessages(SelectedConversation.Messages);

            MemberList.ClearMembers();

            foreach (PersonModel member in SelectedConversation.Members)
            {
                MemberList.AddMemberToList(member, SelectedConversation.ColorDictionary[member]);
            }

            MessageListContainer.ScrollToBottom();
            MemberList.Visibility = Visibility.Visible;

            if (_newEmailButton != null)
            {
                if (MemberList.Members.Count == 1)
                {
                    _newEmailButton.Visibility = Visibility.Collapsed;
                    _newPhoneButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if(_parent.EmailClients != null && _parent.EmailClients.Any(obj => obj.ImapClient != null))
                    _newEmailButton.Visibility = Visibility.Visible;

                    _newPhoneButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void MemberList_AddMember(object sender, EventArgs e)
        {
            _parent.Darkened = true;
            ChoosePerson window = new ChoosePerson(_parent, ConversationList.SelectedConversation.Members);
            _parent.mainCanvas.Children.Add(window);

            window.ReadyButtonClicked += (s, ea) =>
            {
                List<PersonModel> selectedPersons = window.SelectedPersons;

                _parent.Client.AddConversationMember(ConversationList.SelectedConversation.Id, selectedPersons);
                _parent.mainCanvas.Children.Remove(window);
                _parent.Darkened = false;
            };

            window.CancelButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(window);
                _parent.Darkened = false;
            };

        }

        private void MemberList_RemoveFilter(object sender, System.EventArgs e)
        {
            MemberListItem memberItem = (MemberListItem)sender;
            memberItem.Filtered = false;

            _filteredPersons.Remove(memberItem.Person);

            MessageList.ClearMessages();
            if (_filteredPersons.Any())
                MessageList.AddMessages(
                    ConversationList.SelectedConversation.Messages.FindAll(obj => _filteredPersons.Contains(obj.Author)));
            else
            {
                MessageList.AddMessages(ConversationList.SelectedConversation.Messages);
                MemberList.Members.ForEach(obj => obj.Filtering = false);
            }
        }

        private void MemberList_AddFilter(object sender, System.EventArgs e)
        {
            MemberListItem memberItem = (MemberListItem)sender;
            memberItem.Filtered = true;

            if(!_filteredPersons.Any())
                MemberList.Members.ForEach(obj => obj.Filtering = true);

            _filteredPersons.Add(memberItem.Person);

            MessageList.ClearMessages();
            MessageList.AddMessages(ConversationList.SelectedConversation.Messages.FindAll(obj => _filteredPersons.Contains(obj.Author)));
        }

        private void MemberList_ClearFilter(object sender, System.EventArgs e)
        {
            MemberList.Members.ForEach(obj => obj.Filtered = false);

            MessageList.ClearMessages();
            MessageList.AddMessages(ConversationList.SelectedConversation.Messages);

            MemberList.Members.ForEach(obj => obj.Filtering = false);
        }

        private void MemberList_RemoveMember(object sender, EventArgs e)
        {
            MemberListItem memberItem = (MemberListItem)sender;
            ConversationModel conversation = ConversationList.SelectedConversation;
            if (memberItem.Person.Id == _parent.Client.UserInfo.PersonId)
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox("Nie możesz usunać siebie z konwersacji",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s, ea) =>
                {
                    _parent.mainCanvas.Children.Remove(messageBox);
                    _parent.Darkened = false;

                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
            else if (conversation.Members.Count == 1)
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox("Nie można usunąć tej osoby, ponieważ jest jedynym członkiem konwersacji",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s, ea) =>
                {
                    _parent.mainCanvas.Children.Remove(messageBox);
                    _parent.Darkened = false;

                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
            else if (conversation.Messages.Any(obj => obj.Author == memberItem.Person))
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox("Nie można usunąć tej osoby, ponieważ konwersacja zawiera jej wiadomości",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s, ea) =>
                {
                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
            else
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox("Czy na pewno chcesz usunąć tą osobę z konwersacji?",
                        MessageBoxButton.YesNo);

                messageBox.YesButtonClicked += (s, ea) =>
                {
                    _parent.Client.RemoveMember(conversation.Id, memberItem.Person.Id);

                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                };

                messageBox.NoButtonClicked += (s, ea) =>
                {
                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
        }

        private void ConversationList_RenameConversation(object sender, System.EventArgs e)
        {
            ConversationListItem item = (ConversationListItem)sender;

            _parent.Darkened = true;

            Rename renameWindow = new Rename();
            EventHandler readyButtonClicked = null;


            renameWindow.CancelButtonClicked += (s, ev) =>
            {
                _parent.mainCanvas.Children.Remove(renameWindow);
                _parent.Darkened = false;
                renameWindow.ReadyButtonClicked -= readyButtonClicked;
            };

            readyButtonClicked = (s, ev) =>
            {
                renameWindow.LoadingOn = true;

                _parent.Client.RenameConversation(item.Conversation.Id, item.Conversation.Name, renameWindow.NewName);

                renameWindow.LoadingOn = false;

                _parent.mainCanvas.Children.Remove(renameWindow);
                _parent.Darkened = false;
                renameWindow.ReadyButtonClicked -= readyButtonClicked;
            };
            renameWindow.ReadyButtonClicked += readyButtonClicked;

            _parent.mainCanvas.Children.Add(renameWindow);
        }

        private void ConversationList_RemoveConversation(object sender, System.EventArgs e)
        {
            ConversationListItem conversationItem = (ConversationListItem)sender;

            ConversationModel conversation = conversationItem.Conversation;

            if (conversation.Messages.Count > 0)
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox(
                        "Nie można usunąć konwersacji, ponieważ zawiera onainit wiadomości.",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s2, ea2) =>
                {
                    _parent.mainCanvas.Children.Remove(messageBox);
                    _parent.Darkened = false;

                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
            else
            {
                _parent.Darkened = true;
                CustomMessageBox messageBox =
                    new CustomMessageBox("Czy na pewno chcesz usunąć tą konwersację?",
                        MessageBoxButton.YesNo);

                messageBox.YesButtonClicked += (s, ea) =>
                {
                    _parent.Client.RemoveConversation(conversation.Id);

                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                };

                messageBox.NoButtonClicked += (s, ea) =>
                {
                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                };

                _parent.mainCanvas.Children.Add(messageBox);
            }
        }
        #endregion

        #region Methods
        private void LoadConversations()
        {
            if (ConversationList == null) return;

            ConversationList.Clear();
            ConversationList.AddConversations(_conversations);

            if (ConversationList.Conversations.Any())
                ConversationList.SelectedConversationItem = ConversationList.Conversations.First();
        }

        private void _parent_NewConversationArrived(object sender, NewConversationArrivedEventArgs e)
        {
            ConversationListItem conversationItem = new ConversationListItem(e.Conversation);
            ConversationList.AddConversation(conversationItem);
            ConversationList.SelectedConversationItem = conversationItem;
        }
        #endregion
    }
}
