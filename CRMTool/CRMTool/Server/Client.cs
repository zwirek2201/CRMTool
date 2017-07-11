using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;
using Licencjat_new.Windows.HelperWindows;
using Message = ImapX.Message;

namespace Licencjat_new.Server
{
    public class Client
    {
        #region Variables
        private Thread _connectionThread;
        private string _user;
        private string _password;
        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private readonly string _server = Properties.Settings.Default.ServerIP;
        private int _port = 2001;
        private bool _isConnected = false;

        public event EventHandler<LoginSuccedeedEventArgs> loginSucceeded;
        public event EventHandler<LoginFailedEventArgs> loginFailed;
        public event EventHandler loginFinished;
        public event EventHandler connectionSuccess;
        public event EventHandler connectionFailed;
        #endregion

        public UserInfo UserInfo { get; set; }

        #region Methods
        public void Connect(string user, string password)
        {
            try
            {
                string ip =
                    _user = user;
                _password = password;

                if (_connectionThread == null)
                {
                    _connectionThread = new Thread(Setup);
                    _connectionThread.IsBackground = true;
                    _connectionThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        private void Setup()
        {
            try
            {
                _client = new TcpClient(_server, _port);
                _stream = _client.GetStream();

                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);

                int result = _reader.ReadByte();
                if (result == MessageDictionary.Hello)
                {
                    _writer.Write(MessageDictionary.Hello);
                    _isConnected = true;

                    connectionSuccess?.Invoke(this, EventArgs.Empty);
                    Login();
                }
                else
                {
                    _isConnected = false;
                    connectionFailed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                connectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Login()
        {
            try
            {
                if (_isConnected)
                {
                    _writer.Write(MessageDictionary.Login);
                    _writer.Write(_user);
                    _writer.Write(_password);
                    _writer.Flush();

                    byte response = _reader.ReadByte();
                    if (response == MessageDictionary.OK)
                    {
                        string id = _reader.ReadString();
                        string personId = _reader.ReadString();
                        string login = _user;
                        string firstName = _reader.ReadString();
                        string lastName = _reader.ReadString();
                        string lastLoggedOut = _reader.ReadString();

                        DateTime? lastLoggedOutDate = null;

                        if (lastLoggedOut != "")
                            lastLoggedOutDate = DateTime.Parse(lastLoggedOut);

                        UserInfo = new UserInfo()
                        {
                            UserId = id,
                            PersonId = personId,
                            Login = login,
                            FirstName = firstName,
                            LastName = lastName,
                            LastLoggedOut = lastLoggedOutDate,
                        };

                        loginSucceeded?.Invoke(this,
                            new LoginSuccedeedEventArgs(id, login, firstName, lastName, lastLoggedOutDate));
                    }
                    else if (response == MessageDictionary.Error)
                    {
                        byte errorCode = _reader.ReadByte();
                        string errorMessage = _reader.ReadString();
                        loginFailed?.Invoke(this, new LoginFailedEventArgs(errorCode, errorMessage));
                    }
                }
                else
                {
                    byte errorCode = _reader.ReadByte();
                    string errorMessage = _reader.ReadString();
                    loginFailed?.Invoke(this, new LoginFailedEventArgs(errorCode, errorMessage));
                }
                loginFinished?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public List<EmailModel> GetUserEmailAddresses()
        {
            try
            {
                List<EmailModel> emailAddresses = new List<EmailModel>();

                _writer.Write(MessageDictionary.GetEmailAddresses);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    int emailCount = _reader.ReadInt32();
                    for (int i = 0; i < emailCount; i++)
                    {
                        string id = _reader.ReadString();
                        string address = _reader.ReadString();
                        string login = _reader.ReadString();
                        string imapHost = _reader.ReadString();
                        int imapPort = _reader.ReadInt32();
                        bool imapUsessl = _reader.ReadBoolean();
                        string smtpHost = _reader.ReadString();
                        int smtpPort = _reader.ReadInt32();
                        bool smtpUsessl = _reader.ReadBoolean();
                        string lastUid = _reader.ReadString();

                        int unhandledMessagesCount = _reader.ReadInt32();

                        List<string> unhandledMessages = new List<string>();

                        for (int j = 0; j < unhandledMessagesCount; j++)
                        {
                            unhandledMessages.Add(_reader.ReadString());
                        }

                        EmailModel email = new EmailModel(id, address, login, imapHost, imapPort, imapUsessl, smtpHost, smtpPort, smtpUsessl, unhandledMessages,
                            lastUid);
                        emailAddresses.Add(email);
                    }
                    return _reader.Read() == MessageDictionary.EndOfMessage ? emailAddresses : null;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public List<ConversationModel> GetUserConversations()
        {
            try
            {
                List<ConversationModel> userConversations = new List<ConversationModel>();

                _writer.Write(MessageDictionary.GetUserConversations);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    int conversationCount = _reader.ReadInt32();
                    for (int i = 0; i < conversationCount; i++)
                    {
                        string id = _reader.ReadString();
                        string name = _reader.ReadString();
                        string visibleId = _reader.ReadString();
                        string dateCreatedString = _reader.ReadString();
                        bool notifyContactPersons = _reader.ReadBoolean();

                        DateTime dateCreated = DateTime.ParseExact(dateCreatedString, "dd-MM-yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture);

                        int memberCount = _reader.ReadInt32();

                        List<string> memberIds = new List<string>();
                        List<string> memberColors = new List<string>();
                        for (int j = 0; j < memberCount; j++)
                        {
                            string memberId = _reader.ReadString();
                            string memberColor = _reader.ReadString();

                            memberIds.Add(memberId);
                            memberColors.Add(memberColor);
                        }

                        ConversationModel conversation = new ConversationModel(id, name, visibleId, dateCreated,
                            memberIds, memberColors, notifyContactPersons);
                        userConversations.Add(conversation);
                    }
                    if (_reader.Read() == MessageDictionary.EndOfMessage)
                        return userConversations;
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public List<ConversationMessageModel> GetConversationMessages(string conversationId)
        {
            try
            {
                List<ConversationMessageModel> messages = new List<ConversationMessageModel>();

                _writer.Write(MessageDictionary.GetConversationMessages);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversationId);
                    if (_reader.Read() == MessageDictionary.OK)
                    {
                        int messageCount = _reader.ReadInt32();
                        for (int i = 0; i < messageCount; i++)
                        {
                            string id = _reader.ReadString();
                            string authorId = _reader.ReadString();
                            string authorFrom = _reader.ReadString();
                            string initialDateString = _reader.ReadString();
                            Boolean received = _reader.ReadBoolean();

                            byte[] previewData = ReceiveFile();

                            List<string> attachmentIds = new List<string>();

                            int attachmentCount = _reader.ReadInt32();

                            for (int j = 0; j < attachmentCount; j++)
                            {
                                attachmentIds.Add(_reader.ReadString());
                            }

                            MemoryStream stream = new MemoryStream(previewData);
                            BitmapImage previewImage = new BitmapImage();
                            previewImage.BeginInit();
                            previewImage.StreamSource = stream;
                            previewImage.EndInit();

                            previewImage.Freeze();

                            DateTime initialDate = DateTime.ParseExact(initialDateString,"dd-MM-yyyy HH:mm:ss",CultureInfo.InvariantCulture);
                            ConversationMessageModel message = new ConversationMessageModel(id, conversationId, authorId,
                                authorFrom, initialDate, received, previewImage, false, attachmentIds);

                            byte type = _reader.ReadByte();

                            if (type == MessageDictionary.MessageTypeEmail)
                            {
                                string subject = _reader.ReadString();
                                string content = _reader.ReadString();

                                ConversationEmailMessageModel emailMessage = new ConversationEmailMessageModel(message,
                                    subject, content);
                                message = emailMessage;
                            }
                            else if (type == MessageDictionary.MessageTypePhoneCall)
                            {
                                string recipientPhoneNumberId = _reader.ReadString();
                                string callDescription = _reader.ReadString();
                                bool callAnswered = _reader.ReadBoolean();
                                ConversationPhoneMessageModel phoneMessage = new ConversationPhoneMessageModel(message, recipientPhoneNumberId,
                                    callDescription, callAnswered);
                                message = phoneMessage;
                            }

                            if (message != null)
                                messages.Add(message);
                        }
                        if (_reader.Read() == MessageDictionary.EndOfMessage)
                            return messages;
                        throw new Exception("Connection unsynced");
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public string CheckConversationExists(string conversationVisibleId)
        {
            try
            {
                _writer.Write(MessageDictionary.CheckConversationExists);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversationVisibleId);
                    switch (_reader.Read())
                    {
                        case MessageDictionary.Exists:
                            string conversationId = _reader.ReadString();
                            return conversationId;
                        case MessageDictionary.DoesNotExist:
                            throw new Exception("Conversation doesn't exist");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public int AddNewMessage(string conversationId, ConversationMessageModel message)
        {
            try
            {
                Stream previewImageData = null;

                if (message.PreviewImage != null)
                {
                    PngBitmapEncoder pngFile = new PngBitmapEncoder();

                    pngFile.Frames.Add((BitmapFrame)message.PreviewImage);

                    MemoryStream stream = new MemoryStream();
                    pngFile.Save(stream);

                    previewImageData = stream;
                }

                _writer.Write(MessageDictionary.NewMessage);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversationId);
                    _writer.Write(message.AuthorId);

                    _writer.Write(message.Attachments.Count);
                    foreach (FileModel attachment in message.Attachments)
                    {
                        _writer.Write(attachment.Id);
                    }

                    if (message is ConversationEmailMessageModel)
                    {
                        ConversationEmailMessageModel emailMessage = (ConversationEmailMessageModel) message;
                        _writer.Write(MessageDictionary.MessageTypeEmail);
                        if (_reader.Read() == MessageDictionary.OK)
                        {
                            _writer.Write(emailMessage.AuthorEmailaddress.Id);
                            _writer.Write(emailMessage.InitialDate.Value.ToString("yyyy-MM-dd HH:mm"));
                            _writer.Write(emailMessage.MessageSubject);
                            _writer.Write(emailMessage.MessageContent);

                            SendFile(previewImageData);


                            if (_reader.Read() != MessageDictionary.OK)
                            {
                                throw new Exception("Connection unsynced");
                            }
                            return 1;
                        }
                        throw new Exception("Connection unsynced");
                    }

                    if (message is ConversationPhoneMessageModel)
                    {
                        ConversationPhoneMessageModel phoneMessage = (ConversationPhoneMessageModel)message;
                        _writer.Write(MessageDictionary.MessageTypePhoneCall);

                        _writer.Write(phoneMessage.AuthorPhoneNumber.Id);
                        _writer.Write(phoneMessage.RecipientPhoneNumber.Id);
                        _writer.Write(phoneMessage.InitialDate.Value.ToString("yyyy-MM-dd HH:mm"));
                        _writer.Write(phoneMessage.CallDescription);
                        _writer.Write(phoneMessage.CallAnswered);

                        SendFile(previewImageData);

                        if (_reader.Read() != MessageDictionary.OK)
                        {
                            throw new Exception("Connection unsynced");
                        }
                        return 1;
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return 0;
            }
        }

        public List<CompanyModel> GetAllCompanies()
        {
            try
            {
                _writer.Write(MessageDictionary.GetAllCompanies);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    List<CompanyModel> companies = new List<CompanyModel>();
                    int companyCount = _reader.ReadInt32();

                    for (int i = 0; i < companyCount; i++)
                    {
                        string CompanyId = _reader.ReadString();
                        string CompanyName = _reader.ReadString();

                        CompanyModel company = new CompanyModel(CompanyId, CompanyName);
                        companies.Add(company);
                    }

                    if (_reader.Read() == MessageDictionary.EndOfMessage)
                    {
                        return companies;
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public List<PersonModel> GetAllContacts()
        {
            try
            {
                _writer.Write(MessageDictionary.GetAllContacts);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    List<PersonModel> contacts = new List<PersonModel>();
                    int personCount = _reader.ReadInt32();

                    for (int i = 0; i < personCount; i++)
                    {
                        string PersonId = _reader.ReadString();
                        string PersonFirstName = _reader.ReadString();
                        string PersonLastName = _reader.ReadString();
                        string PersonGenderCode = _reader.ReadString();
                        string PersonCompanyId = _reader.ReadString();
                        bool IsInternalUser = _reader.ReadBoolean();

                        Gender PersonGender = (Gender)Convert.ToInt32(PersonGenderCode);

                        PersonModel contactPerson = new PersonModel(PersonId, PersonFirstName, PersonLastName, PersonGender,
                            PersonCompanyId, IsInternalUser);

                        int emailAddressCount = _reader.ReadInt32();

                        for (int j = 0; j < emailAddressCount; j++)
                        {
                            string emailId = _reader.ReadString();
                            string emailName = _reader.ReadString();
                            string emailAddress = _reader.ReadString();
                            bool emailActive = _reader.ReadBoolean();
                            bool emailDefault = _reader.ReadBoolean();

                            EmailAddressModel emailAddressModel = new EmailAddressModel(emailId, emailAddress, emailName,
                                emailActive, emailDefault);
                            contactPerson.EmailAddresses.Add(emailAddressModel);
                        }

                        if (_reader.Read() == MessageDictionary.EndOfMessage)
                        {
                            int phoneNumberCount = _reader.ReadInt32();

                            for (int j = 0; j < phoneNumberCount; j++)
                            {
                                string phoneNumberId = _reader.ReadString();
                                string phoneName = _reader.ReadString();
                                string phoneNumber = _reader.ReadString();
                                bool phoneActive = _reader.ReadBoolean();
                                bool phoneDefault = _reader.ReadBoolean();

                                PhoneNumberModel phoneNumberModel = new PhoneNumberModel(phoneNumberId, phoneNumber, phoneName,
                                    phoneActive, phoneDefault);
                                contactPerson.PhoneNumbers.Add(phoneNumberModel);
                            }
                        }
                        if (_reader.Read() != MessageDictionary.EndOfMessage)
                            throw new Exception("Connection unsynced");

                        contacts.Add(contactPerson);
                    }
                    if (_reader.Read() == MessageDictionary.EndOfMessage)
                    {
                        return contacts;
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public List<NotificationModel> GetUserNotifications(int from = 0, int count = 20)
        {
            try
            {
                _writer.Write(MessageDictionary.GetUserNotifications);
                _writer.Write(from);
                _writer.Write(count);

                if (_reader.Read() == MessageDictionary.OK)
                {
                    List<NotificationModel> notifications = new List<NotificationModel>();

                    int notificationCount = _reader.ReadInt32();

                    for (int i = 0; i < notificationCount; i++)
                    {
                        List<string> referenceFields = new List<string>();

                        string notificationId = _reader.ReadString();

                        string notificationText = _reader.ReadString();

                        DateTime notificationDate = DateTime.ParseExact(_reader.ReadString(),"dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        bool notificationRead = _reader.ReadBoolean();

                        int referenceFieldCount = _reader.ReadInt32();

                        for (int j = 0; j < referenceFieldCount; j++)
                        {
                            referenceFields.Add(_reader.ReadString());
                        }

                        NotificationModel notification = new NotificationModel(notificationId, notificationText, referenceFields,
                            notificationDate, notificationRead);

                        notifications.Add(notification);
                    }

                    if (_reader.Read() == MessageDictionary.EndOfMessage)
                    {
                        return notifications;
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public void ReportNotificationsRead(List<NotificationModel> notifications)
        {
            try
            {
                _writer.Write(MessageDictionary.NotificationsRead);
                _writer.Write(notifications.Count);

                foreach (NotificationModel notification in notifications)
                {
                    _writer.Write(notification.Id);
                }

                _writer.Write(MessageDictionary.EndOfMessage);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void SetLastDownloadedUid(string emailAddress, string uid)
        {
            try
            {
                _writer.Write(MessageDictionary.SetLastDownloadedUid);
                _writer.Write(emailAddress);
                _writer.Write(uid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddUnhandledMessages(string emailAddress, List<string> messages)
        {
            try
            {
                _writer.Write(MessageDictionary.AddUnhandledMessages);
                _writer.Write(emailAddress);
                _writer.Write(messages.Count);

                foreach (string message in messages)
                {
                    _writer.Write(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void HandleMessage(string emailAddress, string messageUId)
        {
            try
            {
                _writer.Write(MessageDictionary.HandleMessage);
                _writer.Write(emailAddress);
                _writer.Write(messageUId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public List<FileModel> GetFilesInfo()
        {
            try
            {
                List<FileModel> files = new List<FileModel>();

                _writer.Write(MessageDictionary.GetFilesInfo);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    int fileCount = _reader.ReadInt32();
                    for (int i = 0; i < fileCount; i++)
                    {
                        string conversationId = _reader.ReadString();
                        string id = _reader.ReadString();
                        string name = _reader.ReadString();
                        string contentType = _reader.ReadString();
                        long size = _reader.ReadInt64();
                        string dateAdded = _reader.ReadString();

                        FileModel file = new FileModel(id, name, new ContentType(contentType), size, DateTime.ParseExact(dateAdded, "dd-MM-yyyy", CultureInfo.InvariantCulture)) {ConversationId = conversationId};
                        files.Add(file);
                    }

                    if (_reader.Read() == MessageDictionary.EndOfMessage)
                    {
                        return files;
                    }
                    throw new Exception("Connection unsynced");
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                return null;
            }
        }

        public void RenameConversation(string conversationId, string oldName, string newName)
        {
            try
            {
                _writer.Write(MessageDictionary.RenameConversation);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversationId);
                    _writer.Write(oldName);
                    _writer.Write(newName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void RenameFile(string fileId, string oldName, string newName)
        {
            try
            {
                _writer.Write(MessageDictionary.RenameFile);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(fileId);
                    _writer.Write(oldName);
                    _writer.Write(newName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddNewConversation(string name)
        {
            try
            {
                _writer.Write(MessageDictionary.NewConversation);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddConversationMember(string conversation, List<PersonModel> selectedPersons)
        {
            try
            {
                _writer.Write(MessageDictionary.AddConversationMembers);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversation);

                    _writer.Write(selectedPersons.Count);

                    foreach (PersonModel person in selectedPersons)
                        _writer.Write(person.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void RemoveMember(string conversationId, string memberId)
        {
            try
            {
                _writer.Write(MessageDictionary.RemoveConversationMember);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversationId);
                    _writer.Write(memberId);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void SaveConversationSettings(ConversationModel conversation)
        {
            try
            {
                _writer.Write(MessageDictionary.ChangeConversationSettings);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(conversation.Id);

                    _writer.Write(conversation.NotifyContactPersons);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddNewCompany(string companyName)
        {
            try
            {
                _writer.Write(MessageDictionary.NewCompany);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(companyName);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void RenameCompany(CompanyModel company, string newName)
        {
            try
            {
                _writer.Write(MessageDictionary.RenameCompany);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(company.Id);
                    _writer.Write(company.Name);
                    _writer.Write(newName);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void RemoveCompany(CompanyModel company)
        {
            try
            {
                _writer.Write(MessageDictionary.RemoveCompany);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(company.Id);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void UpdatePersonDetails(string id, string firstName, string lastName, Gender gender, CompanyModel company, List<EmailAddressModel> emailList, List<PhoneNumberModel> phoneList)
        {
            try
            {
                _writer.Write(MessageDictionary.UpdatePersonDetails);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(id);
                    _writer.Write(firstName);
                    _writer.Write(lastName);
                    _writer.Write(Convert.ToInt32(gender));
                    _writer.Write(company != null ? company.Id : "");

                    _writer.Write(emailList.Count);
                    foreach (EmailAddressModel email in emailList)
                    {
                        _writer.Write(email.Id);
                        _writer.Write(email.Name);
                        _writer.Write(email.Address);
                    }

                    _writer.Write(phoneList.Count);
                    foreach (PhoneNumberModel phone in phoneList)
                    {
                        _writer.Write(phone.Id);
                        _writer.Write(phone.Name);
                        _writer.Write(phone.Number);
                    }
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddExternalContact(string firstName, string lastName, Gender gender, CompanyModel company, List<EmailAddressModel> emailList, List<PhoneNumberModel> phoneList)
        {
            try
            {
                _writer.Write(MessageDictionary.NewExternalContact);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(firstName);
                    _writer.Write(lastName);
                    _writer.Write(Convert.ToInt32(gender));
                    _writer.Write(company != null ? company.Id : "");

                    _writer.Write(emailList.Count);
                    foreach (EmailAddressModel email in emailList)
                    {
                        _writer.Write(email.Id);
                        _writer.Write(email.Name);
                        _writer.Write(email.Address);
                    }

                    _writer.Write(phoneList.Count);
                    foreach (PhoneNumberModel phone in phoneList)
                    {
                        _writer.Write(phone.Id);
                        _writer.Write(phone.Name);
                        _writer.Write(phone.Number);
                    }
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void AddNewEmailAddress(NewEmailAddressEventArgs ea)
        {
            try
            {
                _writer.Write(MessageDictionary.AddEmailAddress);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(ea.Address);
                    _writer.Write(ea.Login);
                    _writer.Write(ea.UseLoginPassword);
                    _writer.Write(ea.ImapHost);
                    _writer.Write(ea.ImapPort);
                    _writer.Write(ea.ImapUseSsl);
                    _writer.Write(ea.SmtpHost);
                    _writer.Write(ea.SmtpPort);
                    _writer.Write(ea.SmtpUseSsl);
                    _writer.Write(ea.Name);
                    return;
                }
                throw new Exception("Connection unsynced");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
            }
        }

        public void SendFile(Stream data)
        {
            if (data == null)
            {
                _writer.Write(0);
                return;
            }

            _writer.Write(data.Length);
            if (_reader.Read() == MessageDictionary.OK)
            {
                data.Position = 0;
                data.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[1024 * 8];
                int count;
                while ((count = data.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, count);
            }
        }

        public byte[] ReceiveFile()
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
        #endregion
    }

    public class LoginFailedEventArgs : EventArgs
    {
        public byte ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public LoginFailedEventArgs(byte errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }

    public class LoginSuccedeedEventArgs : EventArgs
    {
        public string UserId { get; private set; }
        public string UserLogin { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public DateTime? LastLoggedOut { get; private set; }

        public LoginSuccedeedEventArgs(string userId, string userLogin, string firstName, string lastName, DateTime? lastLoggedOut)
        {
            UserId = userId;
            UserLogin = userLogin;
            FirstName = firstName;
            LastName = lastName;
            LastLoggedOut = lastLoggedOut;
        }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string PersonId { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? LastLoggedOut { get; set; }
    }
}
