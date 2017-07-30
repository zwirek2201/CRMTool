using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public class Client
    {
        public Program Program
        {
            get;
            set;
        }

        private TcpClient _client;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private Stream _stream;
        private UserInfo _userInfo = new UserInfo();

        public NotificationClient NotificationClient { get; set; }
        public UploadClient UploadClient { get; set; }
        public DownloadClient DownloadClient { get; set;}

        public event UserLogStateChangedEventHandler UserLoggedIn;
        public event UserLogStateChangedEventHandler UserLoggedOut;

        public delegate void UserLogStateChangedEventHandler(object sender);

        public UserInfo UserInfo
        {
            get { return _userInfo; }
            private set { _userInfo = value; }
        }

        public Client(TcpClient client, Program program)
        {
            _client = client;
            Program = program;
            Logger.Log("New connection");

            Thread thread = new Thread(ClientSetup);
            thread.Start();
        }

        private void ClientSetup()
        {
            try
            {
                _stream = _client.GetStream();

                SslStream sslStream = new SslStream(_stream, false);


                _reader = new BinaryReader(_stream, Encoding.UTF8);
                _writer = new BinaryWriter(_stream, Encoding.UTF8);

                _writer.Write(MessageDictionary.Hello);

                byte response = _reader.ReadByte();
                if (response == MessageDictionary.Hello)
                {
                    response = _reader.ReadByte();
                    switch (response)
                    {
                        case MessageDictionary.Login:
                            string login = _reader.ReadString();
                            string password = _reader.ReadString();

                            LoginResultInfo loginInfo = DBApi.CheckUserCredentials(login, password);
                            if (loginInfo.Status == MessageDictionary.OK)
                            {
                                _writer.Write(MessageDictionary.OK);
                                _writer.Write(loginInfo.UserId);
                                _writer.Write(loginInfo.PersonId);
                                _writer.Write(loginInfo.FirstName);
                                _writer.Write(loginInfo.LastName);
                                _writer.Write(loginInfo.LastLoggedOut);

                                _writer.Flush();

                                _userInfo = new UserInfo()
                                {
                                    UserId = loginInfo.UserId,
                                    PersonId = loginInfo.PersonId,
                                    Login = login,
                                    FirstName = loginInfo.FirstName,
                                    LastName = loginInfo.LastName,
                                    IsConnected = true
                                };

                                UserLoggedIn?.Invoke(this);
                                Logger.Log("Login succedeed");

                                Receiver();
                            }
                            else if (loginInfo.Status == MessageDictionary.Error)
                            {
                                switch (loginInfo.Error)
                                {
                                    case MessageDictionary.WrongPassword:
                                        Logger.Log($"Login failed ({MessageDictionary.WrongPassword})");
                                        _writer.Write(MessageDictionary.Error);
                                        _writer.Write(MessageDictionary.WrongPassword);
                                        _writer.Write(
                                            ErrorMessageDictionary.GetErrorMessageByCode(MessageDictionary.WrongPassword));
                                        _writer.Flush();
                                        break;
                                    case MessageDictionary.UserNotFound:
                                        Logger.Log($"Login failed ({MessageDictionary.UserNotFound})");
                                        _writer.Write(MessageDictionary.Error);
                                        _writer.Write(MessageDictionary.UserNotFound);
                                        _writer.Write(
                                            ErrorMessageDictionary.GetErrorMessageByCode(MessageDictionary.UserNotFound));
                                        _writer.Flush();
                                        break;
                                }
                            }
                            break;
                        case MessageDictionary.ImNotificationClient:
                            string userId = _reader.ReadString();
                            NotificationClient notificationClient = new NotificationClient(_client, userId);
                            notificationClient.program = Program;

                            Client client = Program.GetClientById(userId);

                            if (client != null)
                            {
                                client.NotificationClient = notificationClient;
                                _writer.Write(MessageDictionary.OK);
                                Program.DestroyClient(this);
                            }
                            break;
                        case MessageDictionary.ImUploadClient:
                            userId = _reader.ReadString();
                            UploadClient uploadClient = new UploadClient(_client, userId);

                            client = Program.GetClientById(userId);

                            if (client != null)
                            {
                                client.UploadClient = uploadClient;
                                _writer.Write(MessageDictionary.OK);
                                Program.DestroyClient(this);
                            }
                            break;
                        case MessageDictionary.ImDownloadClient:
                            userId = _reader.ReadString();
                            DownloadClient downloadClient = new DownloadClient(_client, userId);

                            client = Program.GetClientById(userId);

                            if (client != null)
                            {
                                client.DownloadClient = downloadClient;
                                _writer.Write(MessageDictionary.OK);
                                Program.DestroyClient(this);
                            }
                            break;
                        default:
                            CloseConnection();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CloseConnection();
            }
        }

        public void Receiver()
        {
            try
            {
                while (_client.Client.Connected)
                {
                    byte response = _reader.ReadByte();
                    string conversationId = "";
                    string authorId = "";
                    string emailAddressId = "";
                    string newEmailAddress = "";
                    switch (response)
                    {
                            #region GetEmailAddresses

                        case MessageDictionary.GetEmailAddresses:
                            Logger.Log("Got \"GetEmailAddresses\" request");
                            List<EmailResultInfo> emailAddresses = DBApi.GetUserEmailAddresses(_userInfo.UserId);
                            _writer.Write(MessageDictionary.OK);
                            _writer.Write(emailAddresses.Count);
                            foreach (EmailResultInfo email in emailAddresses)
                            {
                                _writer.Write(email.Id);
                                _writer.Write(email.Address);
                                _writer.Write(email.Login);
                                _writer.Write(email.ImapHost);
                                _writer.Write(email.ImapPort);
                                _writer.Write(email.ImapUseSsl);
                                _writer.Write(email.SmtpHost);
                                _writer.Write(email.SmtpPort);
                                _writer.Write(email.SmtpUseSsl);
                                _writer.Write(email.LastUid);

                                _writer.Write(email.UnhandledMessages.Count);

                                foreach (string message in email.UnhandledMessages)
                                {
                                    _writer.Write(message);
                                }
                            }
                            _writer.Write(MessageDictionary.EndOfMessage);
                            _writer.Flush();
                            Logger.Log($"Sent {emailAddresses.Count} addresses");
                            break;

                            #endregion

                            #region ReceiveDocument

                        case MessageDictionary.DownloadFile:
                            try
                            {
                                Logger.Log("Got \"ReceiveDocument\" request");

                                byte[] buffer = new byte[1024*8];
                                Int64 length = _reader.ReadInt64();
                                Int64 receivedBytes = 0;
                                int count;

                                string fileName = _reader.ReadString();

                                FileStream file = File.Create(@"C:\Users\Marcin\Desktop\" + fileName);
                                _writer.Write(MessageDictionary.OK);
                                file.Position = 0;
                                file.Seek(0, SeekOrigin.Begin);
                                while (receivedBytes < length && (count = _reader.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    file.Write(buffer, 0, count);
                                    receivedBytes += count;
                                }

                                file.Close();
                                Logger.Log($"Received {receivedBytes} bytes");
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region GetUserConversations

                        case MessageDictionary.GetUserConversations:
                            try
                            {
                                Logger.Log("Got \"GetUserConversations\" request");
                                List<ConversationResultInfo> userConversations =
                                    DBApi.GetUserConversations(_userInfo.UserId);
                                _writer.Write(MessageDictionary.OK);
                                _writer.Write(userConversations.Count);
                                foreach (ConversationResultInfo conversation in userConversations)
                                {
                                    _writer.Write(conversation.Id);
                                    _writer.Write(conversation.Name);
                                    _writer.Write(conversation.VisibleId);
                                    _writer.Write(conversation.DateStarted);
                                    _writer.Write(conversation.NotifyContactPersons);

                                    _writer.Write(conversation.MemberIds.Count);

                                    for (int i = 0; i < conversation.MemberIds.Count; i++)
                                    {
                                        _writer.Write(conversation.MemberIds[i]);
                                        _writer.Write(conversation.MemberColors[i]);
                                    }
                                }
                                _writer.Write(MessageDictionary.EndOfMessage);
                                _writer.Flush();
                                Logger.Log($"Sent {userConversations.Count} conversations");
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region GetConversationMessages

                        case MessageDictionary.GetConversationMessages:
                            try
                            {
                                Logger.Log("Got \"GetConversationMessages\" request");
                                _writer.Write(MessageDictionary.OK);
                                conversationId = _reader.ReadString();
                                List<ConversationMessageResultInfo> conversationMessages =
                                    DBApi.GetConversationMessages(conversationId);
                                _writer.Write(MessageDictionary.OK);
                                _writer.Write(conversationMessages.Count);
                                foreach (ConversationMessageResultInfo message in conversationMessages)
                                {
                                    _writer.Write(message.Id);
                                    _writer.Write(message.AuthorId);
                                    _writer.Write(message.AuthorFromId);
                                    _writer.Write(message.InitialDate);
                                    _writer.Write(message.Received);

                                    SendFile(message.PreviewImage);

                                    _writer.Write(message.AttachmentIds.Count);
                                    foreach (string attachmentId in message.AttachmentIds)
                                        _writer.Write(attachmentId);

                                    _writer.Write(message.MessageType);

                                    switch (message.MessageType)
                                    {
                                        case MessageDictionary.MessageTypeEmail:
                                            _writer.Write(message.Subject);
                                            _writer.Write(message.MessageContent);
                                            break;
                                        case MessageDictionary.MessageTypePhoneCall:
                                            _writer.Write(message.RecipientToId);
                                            _writer.Write(message.CallDescription);
                                            _writer.Write(message.CallAnswered);
                                            break;
                                    }
                                }
                                _writer.Write(MessageDictionary.EndOfMessage);
                                _writer.Flush();
                                Logger.Log($"Sent {conversationMessages.Count} messages");
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region CheckConversationExists

                        case MessageDictionary.CheckConversationExists:
                            try
                            {
                                _writer.Write(MessageDictionary.OK);
                                string conversationVisibleId = _reader.ReadString();
                                CheckExistsResultInfo resultInfo =
                                    DBApi.CheckConversationExistsByVisibleId(conversationVisibleId);

                                if (resultInfo.Exists)
                                {
                                    _writer.Write(MessageDictionary.Exists);
                                    _writer.Write(resultInfo.Id);
                                    _writer.Flush();
                                }
                                else
                                {
                                    _writer.Write(MessageDictionary.DoesNotExist);
                                    _writer.Flush();
                                }
                                Logger.Log("Sent response (" + resultInfo.Exists + ")");
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region NewMessage

                        case MessageDictionary.NewMessage:
                            try
                            {
                                string messageId = "";
                                ConversationMessageResultInfo message = null;
                                _writer.Write(MessageDictionary.OK);

                                conversationId = _reader.ReadString();
                                authorId = _reader.ReadString();
                                List<string> attachmentIds = new List<string>();
                                int attachmentCount = _reader.ReadInt32();
                                for (int i = 0; i < attachmentCount; i++)
                                {
                                    attachmentIds.Add(_reader.ReadString());
                                }

                                switch (_reader.Read())
                                {
                                    case MessageDictionary.MessageTypeEmail:
                                        _writer.Write(MessageDictionary.OK);

                                        string authorEmailAddressId = _reader.ReadString();
                                        string messageDate = _reader.ReadString();
                                        string messageSubject = _reader.ReadString();
                                        string messageContent = _reader.ReadString();

                                        byte[] messagePreview = ReceiveFile();

                                        messageId = DBApi.AddNewEmailMessage(conversationId, authorId,
                                            authorEmailAddressId,
                                            messageSubject,
                                            messageContent, messageDate, messagePreview, attachmentIds);
                                        message =
                                            new ConversationMessageResultInfo(messageId, authorId, authorEmailAddressId,
                                                messageDate, true, MessageDictionary.MessageTypeEmail, messagePreview,
                                                attachmentIds)
                                            {
                                                Subject = messageSubject,
                                                MessageContent = messageContent,
                                                ConversationId = conversationId
                                            };
                                        _writer.Write(MessageDictionary.OK);
                                        _writer.Flush();
                                        break;
                                    case MessageDictionary.MessageTypePhoneCall:
                                        _writer.Write(MessageDictionary.OK);

                                        string authorPhoneNumberId = _reader.ReadString();
                                        string recipientPhoneNumberId = _reader.ReadString();
                                        messageDate = _reader.ReadString();
                                        string callDescription = _reader.ReadString();
                                        bool callAnswered = _reader.ReadBoolean();

                                        messagePreview = ReceiveFile();

                                        messageId = DBApi.AddNewPhoneMessage(conversationId, authorId,
                                            authorPhoneNumberId, recipientPhoneNumberId,
                                            callDescription,
                                            callAnswered, messageDate, messagePreview, attachmentIds);
                                        message =
                                            new ConversationMessageResultInfo(messageId, authorId, authorPhoneNumberId,
                                                messageDate, true, MessageDictionary.MessageTypePhoneCall, messagePreview,
                                                attachmentIds)
                                            {
                                                RecipientToId = recipientPhoneNumberId,
                                                CallDescription = callDescription,
                                                CallAnswered = callAnswered,
                                                ConversationId = conversationId
                                            };
                                        _writer.Write(MessageDictionary.OK);
                                        _writer.Flush();
                                        break;
                                }
                                NotifySubscribedUsersAboutNewMessage(message);
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region GetAllCompanies

                        case MessageDictionary.GetAllCompanies:
                            try
                            {
                                Logger.Log("Got \"GetAllCompanies\" request");
                                List<CompanyResultInfo> companies = DBApi.GetAllCompanies();

                                _writer.Write(MessageDictionary.OK);
                                _writer.Write(companies.Count);

                                foreach (CompanyResultInfo company in companies)
                                {
                                    _writer.Write(company.Id);
                                    _writer.Write(company.Name);
                                }

                                _writer.Write(MessageDictionary.EndOfMessage);
                                _writer.Flush();
                                Logger.Log("Sent " + companies.Count + " companies");

                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region GetAllContacts

                        case MessageDictionary.GetAllContacts:
                            try
                            {
                                Logger.Log("Got \"GetAllContacts\" request");
                                List<PersonResultInfo> contacts = DBApi.GetAllContacts();

                                _writer.Write(MessageDictionary.OK);
                                _writer.Write(contacts.Count);

                                foreach (PersonResultInfo contact in contacts)
                                {
                                    _writer.Write(contact.Id);
                                    _writer.Write(contact.FirstName);
                                    _writer.Write(contact.LastName);
                                    _writer.Write(contact.Gender);
                                    _writer.Write(contact.CompanyId);
                                    _writer.Write(contact.IsInternalUser);

                                    _writer.Write(contact.EmailAddresses.Count);
                                    foreach (EmailAddressResultInfo emailAddress in contact.EmailAddresses)
                                    {
                                        _writer.Write(emailAddress.Id);
                                        _writer.Write(emailAddress.Name);
                                        _writer.Write(emailAddress.Address);
                                    }
                                    _writer.Write(MessageDictionary.EndOfMessage);

                                    _writer.Write(contact.PhoneNumbers.Count);
                                    foreach (PhoneNumberResultInfo phoneNumber in contact.PhoneNumbers)
                                    {
                                        _writer.Write(phoneNumber.Id);
                                        _writer.Write(phoneNumber.Name);
                                        _writer.Write(phoneNumber.Number);
                                    }

                                    _writer.Write(MessageDictionary.EndOfMessage);
                                }

                                _writer.Write(MessageDictionary.EndOfMessage);
                                _writer.Flush();
                                Logger.Log("Sent " + contacts.Count + " contacts");
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region GetUserNotifications

                        case MessageDictionary.GetUserNotifications:
                            int notificationFrom = _reader.ReadInt32();
                            int notificationCount = _reader.ReadInt32();

                            List<NotificationResultInfo> notifications = DBApi.GetUserNotifications(UserInfo.UserId,
                                notificationFrom, notificationCount);

                            _writer.Write(MessageDictionary.OK);
                            _writer.Write(notifications.Count);

                            foreach (NotificationResultInfo notification in notifications)
                            {
                                NotificationModel notificationModel =
                                    NotificationHandler.ProcessNotification(notification);

                                try
                                {
                                    _writer.Write(notificationModel.NotificationId);
                                    _writer.Write(notificationModel.NotificationText);
                                    _writer.Write(notificationModel.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                                    _writer.Write(notificationModel.NotificationRead);
                                }
                                catch (Exception ex)
                                {
                                    
                                }
                                _writer.Write(notificationModel.NotificationReferenceFields.Count);

                                foreach (string referenceField in notificationModel.NotificationReferenceFields)
                                {
                                    _writer.Write(referenceField);
                                }
                            }

                            _writer.Write(MessageDictionary.EndOfMessage);
                            break;

                            #endregion

                            #region NotificationsRead

                        case MessageDictionary.NotificationsRead:
                            List<string> readNotifications = new List<string>();
                            notificationCount = _reader.ReadInt32();

                            for (int i = 0; i < notificationCount; i++)
                            {
                                readNotifications.Add(_reader.ReadString());
                            }

                            if (_reader.Read() == MessageDictionary.EndOfMessage)
                            {
                                DBApi.ReportNotificationsRead(readNotifications);
                            }
                            break;

                            #endregion

                            #region SetLastDownloadedUid

                        case MessageDictionary.SetLastDownloadedUid:
                            string downloadedEmailAddress = _reader.ReadString();
                            string uid = _reader.ReadString();

                            DBApi.SetLastDownloadedUid(downloadedEmailAddress, uid);
                            break;

                            #endregion

                            #region AddUnhandledMessages

                        case MessageDictionary.AddUnhandledMessages:
                            try
                            {
                                emailAddressId = _reader.ReadString();

                                int messageCount = _reader.ReadInt32();

                                List<string> unhandledMessages = new List<string>();

                                for (int i = 0; i < messageCount; i++)
                                {
                                    unhandledMessages.Add(_reader.ReadString());
                                }

                                DBApi.AddUnhandledMessages(emailAddressId, unhandledMessages);
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region HandleMessage

                        case MessageDictionary.HandleMessage:
                            emailAddressId = _reader.ReadString();
                            string messageUId = _reader.ReadString();
                            DBApi.HandleMessage(emailAddressId, messageUId);
                            break;

                            #endregion

                            #region GetFilesInfo

                        case MessageDictionary.GetFilesInfo:
                            List<FileResultInfo> files = DBApi.GetUserConversationFilesInfo(UserInfo.UserId);

                            _writer.Write(MessageDictionary.OK);
                            _writer.Write(files.Count);

                            foreach (FileResultInfo file in files)
                            {
                                _writer.Write(file.ConversationId);
                                _writer.Write(file.Id);
                                _writer.Write(file.Name);
                                _writer.Write(file.ContentType);
                                _writer.Write(file.Size);
                                _writer.Write(file.DateAdded.ToString("dd-MM-yyyy"));
                            }

                            _writer.Write(MessageDictionary.EndOfMessage);
                            break;
                        case MessageDictionary.RenameConversation:
                            _writer.Write(MessageDictionary.OK);
                            conversationId = _reader.ReadString();
                            string oldName = _reader.ReadString();
                            string newName = _reader.ReadString();

                            DBApi.RenameConversation(conversationId, newName);
                            NotifySubscribedUsersAboutConversationNameChange(conversationId, oldName, newName);
                            break;
                        case MessageDictionary.RenameFile:
                            _writer.Write(MessageDictionary.OK);
                            string fileId = _reader.ReadString();
                            oldName = _reader.ReadString();
                            newName = _reader.ReadString();

                            DBApi.RenameFile(fileId, newName);
                            NotifyAllUsersAboutFileNameChange(conversationId, oldName, newName);
                            break;

                            #endregion

                            #region NewConversation

                        case MessageDictionary.NewConversation:
                            try
                            {
                                _writer.Write(MessageDictionary.OK);
                                string conversationName = _reader.ReadString();
                                ConversationResultInfo conversationResult = DBApi.AddNewConversation(conversationName);
                                List<string> userColors =
                                    DBApi.AddConversationMembers(new List<string>() {UserInfo.PersonId},
                                        conversationResult.Id);

                                conversationResult.MemberIds.Add(UserInfo.PersonId);
                                conversationResult.MemberColors.Add(userColors[0]);

                                NotifyUserAboutNewConversation(UserInfo.UserId, conversationResult);
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region AddConversationMembers

                        case MessageDictionary.AddConversationMembers:
                            try
                            {
                                List<string> personIds = new List<string>();
                                _writer.Write(MessageDictionary.OK);
                                conversationId = _reader.ReadString();

                                int memberCount = _reader.ReadInt32();

                                for (int i = 0; i < memberCount; i++)
                                {
                                    personIds.Add(_reader.ReadString());
                                }

                                List<string> userColors = DBApi.AddConversationMembers(personIds, conversationId);

                                NotifyAllSubscribedUsersAboutNewConversationMembers(conversationId, personIds,
                                    userColors);
                            }
                            catch (Exception ex)
                            {

                            }
                            break;

                            #endregion

                            #region RemoveConversationMember

                        case MessageDictionary.RemoveConversationMember:
                            _writer.Write(MessageDictionary.OK);
                            conversationId = _reader.ReadString();
                            string memberId = _reader.ReadString();

                            NotifySubscribedUsersAboutConversationMemberRemoved(conversationId, memberId);
                            DBApi.RemoveConversationMember(conversationId, memberId);
                            break;

                        #endregion

                        #region ChangeConversationSettings
                        case MessageDictionary.ChangeConversationSettings:
                            _writer.Write(MessageDictionary.OK);

                            conversationId = _reader.ReadString();

                            bool notifyContactPersons = _reader.ReadBoolean();

                            DBApi.ChangeConversationSettings(conversationId, notifyContactPersons);
                            NotifySubscribedUsersAboutConversationSettingsChange(conversationId, notifyContactPersons);
                            break;
                        #endregion

                        #region AddCompany
                        case MessageDictionary.NewCompany:
                            _writer.Write(MessageDictionary.OK);
                            string companyName = _reader.ReadString();
                            string companyId = DBApi.AddCompany(companyName);

                            NotifyAllUsersAboutNewCompany(companyId, companyName);
                            break;
                        #endregion

                        #region RenameCompany
                        case MessageDictionary.RenameCompany:
                            _writer.Write(MessageDictionary.OK);
                            companyId = _reader.ReadString();
                            oldName = _reader.ReadString();
                            newName = _reader.ReadString();

                            DBApi.RenameCompany(companyId, newName);


                            NotifyAllUsersAboutCompanyRenamed(companyId, oldName, newName);
                            break;
                        #endregion
                        #region AddEmailAddress
                        case MessageDictionary.AddEmailAddress:
                            _writer.Write(MessageDictionary.OK);
                            newEmailAddress = _reader.ReadString();
                            string login = _reader.ReadString();
                            bool useLoginInfo = _reader.ReadBoolean();
                            string imapHost = _reader.ReadString();
                            int imapPort = _reader.ReadInt32();
                            bool imapUseSsl = _reader.ReadBoolean();
                            string smtpHost = _reader.ReadString();
                            int smtpPort = _reader.ReadInt32();
                            bool smtpUseSsl = _reader.ReadBoolean();
                            string name = _reader.ReadString();

                            string emailId = DBApi.AddEmailAddress(UserInfo.PersonId,newEmailAddress, login, imapHost, imapPort, imapUseSsl, smtpHost, smtpPort, smtpUseSsl, name);
                            NotifyUserAboutNewEmailAddress(UserInfo.UserId, emailId, newEmailAddress, login, imapHost, imapPort,
                                imapUseSsl, smtpHost, smtpPort, smtpUseSsl, name);

                            break;
                        #endregion
                        #region RemoveCompany
                        case MessageDictionary.RemoveCompany:
                            _writer.Write(MessageDictionary.OK);
                            companyId = _reader.ReadString();

                            companyName = DBApi.RemoveCompany(companyId);


                            NotifyAllUsersAboutCompanyRemoved(companyId, companyName);
                            break;
                        #endregion

                        #region UpdatePersonDetails
                        case MessageDictionary.UpdatePersonDetails:
                            _writer.Write(MessageDictionary.OK);
                            string id = _reader.ReadString();
                            string firstName = _reader.ReadString();
                            string lastName = _reader.ReadString();
                            int gender = _reader.ReadInt32();
                            companyId = _reader.ReadString();

                            List<EmailAddressResultInfo> emailAddressesList = new List<EmailAddressResultInfo>();

                            int emailCount = _reader.ReadInt32();

                            if (emailCount != -1)
                            {
                                for (int i = 0; i < emailCount; i++)
                                {
                                    emailId = _reader.ReadString();
                                    string emailName = _reader.ReadString();
                                    string emailAddress = _reader.ReadString();

                                    emailAddressesList.Add(new EmailAddressResultInfo(emailId, emailName, emailAddress));
                                }
                            }
                            else
                            {
                                emailAddressesList = null;
                            }

                            List<PhoneNumberResultInfo> phoneNumbersList = new List<PhoneNumberResultInfo>();

                            int phoneCount = _reader.ReadInt32();
                            for (int i = 0; i < phoneCount; i++)
                            {
                                string phoneId = _reader.ReadString();
                                string phoneName = _reader.ReadString();
                                string phoneNumber = _reader.ReadString();

                                phoneNumbersList.Add(new PhoneNumberResultInfo(phoneId, phoneName, phoneNumber));
                            }

                            DBApi.UpdatePersonDetails(id, firstName, lastName, gender, companyId, emailAddressesList,
                                phoneNumbersList);

                            NotifyAllUsersAboutPersonDetailsChanged(id, firstName, lastName, gender, companyId,
                                emailAddressesList, phoneNumbersList);

                            break;
                        #endregion

                        #region NewExternalContact
                        case MessageDictionary.NewExternalContact:
                            _writer.Write(MessageDictionary.OK);
                            firstName = _reader.ReadString();
                            lastName = _reader.ReadString();
                            gender = _reader.ReadInt32();
                            companyId = _reader.ReadString();

                            emailAddressesList = new List<EmailAddressResultInfo>();

                            emailCount = _reader.ReadInt32();
                            for (int i = 0; i < emailCount; i++)
                            {
                                emailId = _reader.ReadString();
                                string emailName = _reader.ReadString();
                                string emailAddress = _reader.ReadString();

                                emailAddressesList.Add(new EmailAddressResultInfo(emailId, emailName, emailAddress));
                            }

                            phoneNumbersList = new List<PhoneNumberResultInfo>();

                            phoneCount = _reader.ReadInt32();
                            for (int i = 0; i < phoneCount; i++)
                            {
                                string phoneId = _reader.ReadString();
                                string phoneName = _reader.ReadString();
                                string phoneNumber = _reader.ReadString();

                                phoneNumbersList.Add(new PhoneNumberResultInfo(phoneId, phoneName, phoneNumber));
                            }

                            string personId = DBApi.NewExternalContact(firstName, lastName, gender, companyId, emailAddressesList,
                                phoneNumbersList);

                            NotifyAllUsersAboutNewExternalContact(personId, firstName, lastName, gender, companyId,
                                emailAddressesList, phoneNumbersList);

                            break;
                        #endregion

                        #region RemoveExternalContact
                        case MessageDictionary.RemoveExternalContact:
                            _writer.Write(MessageDictionary.OK);
                            personId = _reader.ReadString();

                            string personName = DBApi.RemoveExternalContact(personId);

                            NotifyAllUsersAboutExternalContactRemoved(personId, personName);
                            break;
                        #endregion;

                        #region RemoveConverastion
                        case MessageDictionary.RemoveConversation:
                            _writer.Write(MessageDictionary.OK);
                            conversationId = _reader.ReadString();

                            string conversationName2 = DBApi.GetConversationName(conversationId);

                            NotifySubscribedUsersAboutConversationRemoved(conversationId, conversationName2);

                            DBApi.RemoveConversation(conversationId);

                            break;
                            #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is IOException))
                {
                    ErrorHelper.LogError(ex);
                }
                else
                {
                    _userInfo.IsConnected = false;
                    UserLoggedOut?.Invoke(this);
                }
            }
        }

        private byte[] ReceiveFile()
        {
            byte[] buffer = new byte[1024 * 8];
            Int64 length = _reader.ReadInt64();
            Int64 receivedBytes = 0;
            int count;

            List<byte> file = new List<byte>();

            _writer.Write(MessageDictionary.OK);
            while (receivedBytes < length && (count = _reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.AddRange(buffer);
                receivedBytes += count;
            }

            return file.ToArray();
        }

        private void SendFile(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);

            _writer.Write((Int64)data.Length);
            if (_reader.Read() == MessageDictionary.OK)
            {
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[1024 * 8];
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, count);
            }
        }

        private void NotifySubscribedUsersAboutConversationRemoved(string conversationId, string conversationName)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.RemovedConversation,
                    SenderId = UserInfo.PersonId,
                    NotificationDate = notificationDate,
                    OldName = conversationName,
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(conversationId);

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    userClient?.NotificationClient.RemoveConversation(conversationId, notification);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyAllUsersAboutExternalContactRemoved(string personId, string personName)
        {
            DateTime notificationDate = DateTime.Now;

            NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
            {
                Type = NotificationType.ExternalContactRemoved,
                SenderId = UserInfo.PersonId,
                OldName = personName,
                NotificationDate = notificationDate,
            };

            NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

            List<string> subscribedUsersId = DBApi.GetAllUsers();

            foreach (string userId in subscribedUsersId)
            {
                NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                recipientNotificationResultInfo.RecipientId = userId;
                string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                notification.NotificationId = notificationId;

                Client userClient = Program.GetClientById(userId);
                if (userClient == null) return;

                userClient.NotificationClient.ExternalContactRemoved(personId, notification);
            }
        }

        private void NotifyAllUsersAboutNewExternalContact(string id, string firstName, string lastName, int gender, string companyId, List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList)
        {
            DateTime notificationDate = DateTime.Now;

            NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
            {
                Type = NotificationType.NewExternalContact,
                SenderId = UserInfo.PersonId,
                PersonId = id,
                NotificationDate = notificationDate,
            };

            NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

            List<string> subscribedUsersId = DBApi.GetAllUsers();

            foreach (string userId in subscribedUsersId)
            {
                NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                recipientNotificationResultInfo.RecipientId = userId;
                string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                notification.NotificationId = notificationId;

                Client userClient = Program.GetClientById(userId);
                if (userClient == null) return;

                userClient.NotificationClient.NewExternalContact(id, firstName, lastName, gender, companyId, emailAddressesList, phoneNumbersList, notification);
            }
        }

        private void NotifyAllUsersAboutPersonDetailsChanged(string id, string firstName, string lastName, int gender, string companyId, List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.UpdatePersonDetails,
                    SenderId = UserInfo.PersonId,
                    PersonId = id,
                    NotificationDate = notificationDate,
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetAllUsers();

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    if(id != UserInfo.PersonId)
                        userClient?.NotificationClient.PersonDetailsChanged(id, firstName, lastName, gender, companyId, emailAddressesList, phoneNumbersList, notification);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void NotifyAllUsersAboutCompanyRemoved(string companyId, string companyName)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.RemovedCompany,
                    SenderId = UserInfo.PersonId,
                    OldName = companyName,
                    NotificationDate = notificationDate,
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetAllUsers();

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);


                    userClient?.NotificationClient.CompanyRemoved(companyId, notification);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyUserAboutNewEmailAddress(string userId, string emailId, string newEmailAddress, string login,
            string imapHost, int imapPort, bool imapUseSsl, string smtpHost, int smtpPort, bool smtpUseSsl, string name)
        {
            try
            {
                Client userClient = Program.GetClientById(userId);

                if (userClient != null)
                {
                    userClient.NotificationClient.NewEmailAddress(emailId, newEmailAddress, login, imapHost, imapPort, imapUseSsl,
                        smtpHost, smtpPort, smtpUseSsl, name);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyAllUsersAboutCompanyRenamed(string companyId, string oldName, string newName)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.RenamedCompany,
                    SenderId = UserInfo.PersonId,
                    CompanyId = companyId,
                    NotificationDate = notificationDate,
                    OldName = oldName,
                    NewName = newName
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetAllUsers();

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);


                    userClient?.NotificationClient.CompanyRenamed(companyId, newName, notification);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyAllUsersAboutNewCompany(string companyId, string companyName)
        {
            DateTime notificationDate = DateTime.Now;

            NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
            {
                Type = NotificationType.NewCompany,
                SenderId = UserInfo.PersonId,
                NotificationDate = notificationDate,
                CompanyId = companyId,
                NewName = companyName
            };

            NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

            List<string> subscribedUsersId = DBApi.GetAllUsers();

            foreach (string userId in subscribedUsersId)
            {
                NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                recipientNotificationResultInfo.RecipientId = userId;
                string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                notification.NotificationId = notificationId;

                Client userClient = Program.GetClientById(userId);
                if (userClient == null) return;

                userClient.NotificationClient.NewCompany(companyId, companyName, notification);
            }
        }

        private void NotifySubscribedUsersAboutConversationSettingsChange(string conversationId, bool notifyContactPersons)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.ConversationSettingsChanged,
                    SenderId = UserInfo.PersonId,
                    ConversationId = conversationId,
                    NotificationDate = notificationDate
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(conversationId);

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    if (userClient.UserInfo.UserId != UserInfo.UserId)
                    {
                        userClient?.NotificationClient.ConversationSettingsChanged(conversationId, notifyContactPersons, notification);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifySubscribedUsersAboutNewMessage(ConversationMessageResultInfo message)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.MessageAdded,
                    SenderId = message.AuthorId,
                    ConversationId = message.ConversationId,
                    NotificationDate = notificationDate
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(message.ConversationId);


                List<FileResultInfo> files = new List<FileResultInfo>();

                foreach (string fileId in message.AttachmentIds)
                {
                    files.AddRange(DBApi.GetFilesInfo(new List<String>() {fileId}));
                }

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    files.ForEach(obj => obj.ConversationId = message.ConversationId);

                    userClient?.NotificationClient.NewFiles(files);

                    userClient?.NotificationClient.NewMessage(message, notification);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void NotifySubscribedUsersAboutConversationNameChange(string conversationId, string oldName, string newName)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.RenamedConversation,
                    SenderId = UserInfo.PersonId,
                    ConversationId = conversationId,
                    NotificationDate = notificationDate,
                    OldName = oldName,
                    NewName = newName
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(conversationId);

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    userClient?.NotificationClient.ConversationRenamed(conversationId, oldName, newName, notification);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyAllUsersAboutFileNameChange(string fileId, string oldName, string newName)
        {
            try
            {
                DateTime notificationDate = DateTime.Now;

                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.RenamedFile,
                    SenderId = UserInfo.PersonId,
                    FileId = fileId,
                    NotificationDate = notificationDate,
                    OldName = oldName,
                    NewName = newName
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetAllUsers();

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    Client userClient = Program.GetClientById(userId);

                    userClient?.NotificationClient.FileRenamed(fileId, oldName, newName, notification);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyUserAboutNewConversation(string userId, ConversationResultInfo conversation)
        {
                Client userClient = Program.GetClientById(userId);

            userClient?.NotificationClient.NewConversation(conversation);
        }

        private void NotifyAllSubscribedUsersAboutNewConversationMembers(string conversation,
            List<string> persons, List<string> personColors)
        {
            DateTime notificationDate = DateTime.Now;

            for (int i = 0; i < persons.Count; i++)
            {
                string person = persons[i];
                string personColor = personColors[i];
                NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
                {
                    Type = NotificationType.ConversationMemberAdded,
                    SenderId = UserInfo.PersonId,
                    ConversationId = conversation,
                    NotificationDate = notificationDate,
                    PersonId = person
                };

                NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

                List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(conversation);

                foreach (string userId in subscribedUsersId)
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = "";
                    notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;
                    Client userClient = Program.GetClientById(userId);

                    if (userClient != null)
                    {
                        if (userClient.UserInfo.PersonId == person)
                        {
                            ConversationResultInfo conversationResult = DBApi.GetUserConversation(conversation, userClient.UserInfo.UserId);
                            userClient.NotifyUserAboutNewConversation(userId, conversationResult);
                        }

                        userClient.NotificationClient.NewConversationMember(conversation, person, personColor,
                            notification);
                    }

                }
            }
        }

        private void NotifySubscribedUsersAboutConversationMemberRemoved(string conversationId, string memberId)
        {
            DateTime notificationDate = DateTime.Now;

            NotificationResultInfo notificationResultInfo = new NotificationResultInfo()
            {
                Type = NotificationType.ConversationMemberRemoved,
                SenderId = UserInfo.PersonId,
                ConversationId = conversationId,
                NotificationDate = notificationDate,
                PersonId = memberId
            };

            NotificationModel notification = NotificationHandler.ProcessNotification(notificationResultInfo);

            List<string> subscribedUsersId = DBApi.GetUsersSubscribedToConversation(conversationId);

            foreach (string userId in subscribedUsersId)
            {
                Client userClient = Program.GetClientById(userId);

                if (userClient != null && notificationResultInfo.PersonId == userClient.UserInfo.PersonId)
                {
                        userClient.NotificationClient.RemoveConversation(conversationId);
                }
                else
                {
                    NotificationResultInfo recipientNotificationResultInfo = notificationResultInfo;
                    recipientNotificationResultInfo.RecipientId = userId;
                    string notificationId = "";
                    notificationId = DBApi.AddNewNotification(recipientNotificationResultInfo);

                    notification.NotificationId = notificationId;

                    if (userClient != null)
                    {
                        userClient.NotificationClient.ConversationMemberRemoved(conversationId, memberId,
                            notification);
                    }
                }
            }
        }


        private void CloseConnection()
        {
            
        }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string PersonId { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsConnected { get; set; } = false;
    }
}
