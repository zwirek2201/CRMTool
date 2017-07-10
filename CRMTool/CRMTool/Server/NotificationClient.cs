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
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Licencjat_new.CustomClasses;
using Licencjat_new.Windows.HelperWindows;

namespace Licencjat_new.Server
{
    public class NotificationClient
    {
        #region Variables
        private Thread _connectionThread;
        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private readonly string _server = Properties.Settings.Default.ServerIP;
        private int _port = 2001;
        private bool _isConnected = false;
        private string _userId;

        public event EventHandler connectionSuccess;
        public event EventHandler connectionFailed;
        public event EventHandler<NewConversationMessageArrivedEventArgs> NewConversationMessageArrived;
        public event EventHandler<NewFilesArrivedEventArgs> NewFilesArrived;
        public event EventHandler<ConversationRenamedEventArgs> ConversationRenamed;
        public event EventHandler<FileRenamedEventArgs> FileRenamed;
        public event EventHandler<NewConversationArrivedEventArgs> NewConversationArrived;
        public event EventHandler<ConversationMembersAddedEventArgs> ConversationMembersAdded;
        public event EventHandler<ConversationMemberRemovedEventArgs> ConversationMemberRemoved;
        public event EventHandler<ConversationRemovedEventArgs> ConversationRemoved;
        public event EventHandler<ConversationSettingsChangedEventArgs> ConversationSettingsChanged;
        public event EventHandler<NewCompanyEventArgs> NewCompanyArrived;
        public event EventHandler<CompanyRenamedEventArgs> CompanyRenamed;
        public event EventHandler<NewEmailAddressEventArgs> NewEmailAddress;
        public event EventHandler<CompanyRemovedEventArgs> CompanyRemoved;
        public event EventHandler<ContactDetailsUpdatedEventArgs> ContactDetailsUpdated;
        #endregion

        #region Constructors
        public NotificationClient(string userId)
        {
            _userId = userId;
            Connect();
        }
        #endregion

        #region Methods
        public void Connect()
        {
            if (_connectionThread == null)
            {
                _connectionThread = new Thread(Setup);
                _connectionThread.IsBackground = true;
                _connectionThread.SetApartmentState(ApartmentState.STA);
                _connectionThread.Start();
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
                    _writer.Write(MessageDictionary.ImNotificationClient);
                    _writer.Write(_userId);

                    if (_reader.Read() == MessageDictionary.OK)
                    {
                        _isConnected = true;

                        connectionSuccess?.Invoke(this, EventArgs.Empty);

                        Receiver();
                    }
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

        public void Receiver()
        {
            try
            {
                string conversationId = "";
                string id = "";
                bool notifyContactPersons = false;
                string emailId = "";
                string emailAddress = "";
                while (_client.Connected)
                {
                    byte response = _reader.ReadByte();
                    switch (response)
                    {
                        case MessageDictionary.NewNotification:
                            _writer.Write(MessageDictionary.OK);

                            List<string> referenceFields = new List<string>();

                            string notificationId = _reader.ReadString();
                            string notificationText = _reader.ReadString();
                            DateTime notificationDate = DateTime.ParseExact(_reader.ReadString(), "dd-MM-yyyy HH:mm:ss",
                                CultureInfo.InvariantCulture);
                            int referenceFieldCount = _reader.ReadInt32();

                            for (int i = 0; i < referenceFieldCount; i++)
                            {
                                referenceFields.Add(_reader.ReadString());
                            }

                            if (_reader.Read() == MessageDictionary.EndOfMessage)
                            {
                                switch (_reader.ReadByte())
                                {
                                        #region NewMessage

                                    case MessageDictionary.NewMessage:
                                        id = _reader.ReadString();
                                        conversationId = _reader.ReadString();
                                        string authorId = _reader.ReadString();
                                        string authorFrom = _reader.ReadString();
                                        string initialDateString = _reader.ReadString();

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

                                        byte type = _reader.ReadByte();

                                        DateTime initialDate = DateTime.ParseExact(initialDateString,
                                            "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        ConversationMessageModel message = new ConversationMessageModel(id,
                                            conversationId,
                                            authorId,
                                            authorFrom, initialDate, true, previewImage, false, attachmentIds);

                                        switch (type)
                                        {
                                            case MessageDictionary.MessageTypeEmail:
                                                string subject = _reader.ReadString();
                                                string content = _reader.ReadString();

                                                ConversationEmailMessageModel emailMessage =
                                                    new ConversationEmailMessageModel(message,
                                                        subject, content);
                                                message = emailMessage;
                                                break;
                                            case MessageDictionary.MessageTypePhoneCall:
                                                string recipientPhoneNumber = _reader.ReadString();
                                                string description = _reader.ReadString();
                                                bool answered = _reader.ReadBoolean();

                                                ConversationPhoneMessageModel phoneMessage =
                                                    new ConversationPhoneMessageModel(message, recipientPhoneNumber,
                                                        description, answered);
                                                message = phoneMessage;
                                                break;
                                        }

                                        NewConversationMessageArrived?.Invoke(this,
                                            new NewConversationMessageArrivedEventArgs()
                                            {
                                                Message = message,
                                                Notification =
                                                    new NotificationModel(notificationId, notificationText,
                                                        referenceFields,
                                                        notificationDate, false)
                                            });
                                        break;

                                        #endregion

                                        #region RenameConversation

                                    case MessageDictionary.RenameConversation:
                                        conversationId = _reader.ReadString();
                                        string oldName = _reader.ReadString();
                                        string newName = _reader.ReadString();

                                        ConversationRenamed?.Invoke(this, new ConversationRenamedEventArgs()
                                        {
                                            ConversationId = conversationId,
                                            OldName = oldName,
                                            NewName = newName,
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                        #endregion

                                        #region RenameFile

                                    case MessageDictionary.RenameFile:
                                        conversationId = _reader.ReadString();
                                        oldName = _reader.ReadString();
                                        newName = _reader.ReadString();

                                        FileRenamed?.Invoke(this, new FileRenamedEventArgs()
                                        {
                                            FileId = conversationId,
                                            OldName = oldName,
                                            NewName = newName,
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                        #endregion

                                        #region AddConversationMembers

                                    case MessageDictionary.AddConversationMembers:
                                        conversationId = _reader.ReadString();
                                        string personId = _reader.ReadString();
                                        string personColor = _reader.ReadString();

                                        ConversationMembersAdded?.Invoke(this,
                                            new ConversationMembersAddedEventArgs()
                                            {
                                                ConversationId = conversationId,
                                                PersonId = personId,
                                                PersonColor = personColor,
                                                Notification = new NotificationModel(notificationId, notificationText,
                                                    referenceFields,
                                                    notificationDate, false)
                                            });
                                        break;

                                        #endregion

                                        #region RemoveConversationMember

                                    case MessageDictionary.RemoveConversationMember:
                                        conversationId = _reader.ReadString();
                                        personId = _reader.ReadString();

                                        ConversationMemberRemoved?.Invoke(this,
                                            new ConversationMemberRemovedEventArgs()
                                            {
                                                ConversationId = conversationId,
                                                PersonId = personId,
                                                Notification = new NotificationModel(notificationId, notificationText,
                                                    referenceFields,
                                                    notificationDate, false)
                                            });
                                        break;

                                        #endregion

                                        #region ConversationSettingsChanged

                                    case MessageDictionary.ChangeConversationSettings:
                                        conversationId = _reader.ReadString();
                                        notifyContactPersons = _reader.ReadBoolean();
                                        ConversationSettingsChanged?.Invoke(this,
                                            new ConversationSettingsChangedEventArgs()
                                            {
                                                ConversationId = conversationId,
                                                NotifyContactPersons = notifyContactPersons,
                                                Notification = new NotificationModel(notificationId, notificationText,
                                                    referenceFields,
                                                    notificationDate, false)
                                            });
                                        break;

                                        #endregion

                                        #region NewCompany

                                    case MessageDictionary.NewCompany:
                                        string companyId = _reader.ReadString();
                                        string companyName = _reader.ReadString();

                                        NewCompanyArrived?.Invoke(this, new NewCompanyEventArgs()
                                        {
                                            Company = new CompanyModel(companyId, companyName),
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                        #endregion

                                        #region RenameCompany

                                    case MessageDictionary.RenameCompany:
                                        companyId = _reader.ReadString();
                                        newName = _reader.ReadString();

                                        CompanyRenamed?.Invoke(this, new CompanyRenamedEventArgs()
                                        {
                                            CompanyId = companyId,
                                            NewName = newName,
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                        #endregion

                                        #region RemoveCompanyEvent

                                    case MessageDictionary.RemoveCompany:
                                        companyId = _reader.ReadString();

                                        CompanyRemoved?.Invoke(this, new CompanyRemovedEventArgs()
                                        {
                                            CompanyId = companyId,
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                    #endregion

                                    #region UpdatePersonDetails

                                    case MessageDictionary.UpdatePersonDetails:
                                        _writer.Write(MessageDictionary.OK);

                                        string PersonId = _reader.ReadString();
                                        string PersonFirstName = _reader.ReadString();
                                        string PersonLastName = _reader.ReadString();
                                        string PersonGenderCode = _reader.ReadString();
                                        string PersonCompanyId = _reader.ReadString();

                                        Gender PersonGender = (Gender)Convert.ToInt32(PersonGenderCode);

                                        PersonModel contactPerson = new PersonModel(PersonId, PersonFirstName, PersonLastName,
                                            PersonGender,
                                            PersonCompanyId, true);

                                        int emailAddressCount = _reader.ReadInt32();

                                        for (int j = 0; j < emailAddressCount; j++)
                                        {
                                            emailId = _reader.ReadString();
                                            string emailName = _reader.ReadString();
                                            emailAddress = _reader.ReadString();

                                            EmailAddressModel emailAddressModel = new EmailAddressModel(emailId, emailAddress,
                                                emailName,
                                                true, true);
                                            contactPerson.EmailAddresses.Add(emailAddressModel);
                                        }

                                        int phoneNumberCount = _reader.ReadInt32();

                                        for (int j = 0; j < phoneNumberCount; j++)
                                        {
                                            string phoneNumberId = _reader.ReadString();
                                            string phoneName = _reader.ReadString();
                                            string phoneNumber = _reader.ReadString();

                                            PhoneNumberModel phoneNumberModel = new PhoneNumberModel(phoneNumberId, phoneNumber,
                                                phoneName,
                                                true, true);
                                            contactPerson.PhoneNumbers.Add(phoneNumberModel);
                                        }

                                        ContactDetailsUpdated?.Invoke(this, new ContactDetailsUpdatedEventArgs()
                                        {
                                            NewData = contactPerson,
                                            Notification = new NotificationModel(notificationId, notificationText,
                                                referenceFields,
                                                notificationDate, false)
                                        });
                                        break;

                                        #endregion
                                }
                            }
                            break;

                            #region NewFiles

                        case MessageDictionary.NewFiles:
                            List<FileModel> files = new List<FileModel>();
                            files.Clear();
                            _writer.Write(MessageDictionary.OK);
                            int fileCount = _reader.ReadInt32();

                            for (int i = 0; i < fileCount; i++)
                            {
                                conversationId = _reader.ReadString();
                                id = _reader.ReadString();
                                string name = _reader.ReadString();
                                string contentType = _reader.ReadString();
                                long size = _reader.ReadInt64();
                                string dateAdded = _reader.ReadString();
                                FileModel file = new FileModel(id, name, new ContentType(contentType), size,
                                    DateTime.ParseExact(dateAdded, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                                {
                                    ConversationId = conversationId
                                };

                                files.Add(file);
                            }

                            NewFilesArrived?.Invoke(this, new NewFilesArrivedEventArgs() {Files = files});
                            break;

                            #endregion

                            #region NewConversation

                        case MessageDictionary.NewConversation:
                            _writer.Write(MessageDictionary.OK);

                            conversationId = _reader.ReadString();
                            string conversationName = _reader.ReadString();
                            string visibleId = _reader.ReadString();
                            string dateCreatedString = _reader.ReadString();
                            notifyContactPersons = _reader.ReadBoolean();


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

                            List<ConversationMessageModel> messages = new List<ConversationMessageModel>();

                            if (_reader.Read() == MessageDictionary.OK)
                            {
                                int messageCount = _reader.ReadInt32();
                                for (int i = 0; i < messageCount; i++)
                                {
                                    id = _reader.ReadString();
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

                                    DateTime initialDate = DateTime.ParseExact(initialDateString, "dd-MM-yyyy HH:mm:ss",
                                        CultureInfo.InvariantCulture);
                                    ConversationMessageModel message = new ConversationMessageModel(id, conversationId,
                                        authorId,
                                        authorFrom, initialDate, received, previewImage, false, attachmentIds);

                                    byte type = _reader.ReadByte();

                                    if (type == MessageDictionary.MessageTypeEmail)
                                    {
                                        string subject = _reader.ReadString();
                                        string content = _reader.ReadString();

                                        ConversationEmailMessageModel emailMessage =
                                            new ConversationEmailMessageModel(message,
                                                subject, content);
                                        message = emailMessage;
                                    }
                                    else if (type == MessageDictionary.MessageTypePhoneCall)
                                    {
                                        string recipientPhoneNumber = _reader.ReadString();
                                        string callDescription = _reader.ReadString();
                                        bool callAnswered = _reader.ReadBoolean();
                                        ConversationPhoneMessageModel phoneMessage =
                                            new ConversationPhoneMessageModel(message, recipientPhoneNumber,
                                                callDescription, callAnswered);
                                        message = phoneMessage;
                                    }

                                    if (message != null)
                                        messages.Add(message);
                                }

                                if (_reader.Read() == MessageDictionary.EndOfMessage)
                                {
                                    ConversationModel conversation = new ConversationModel(conversationId,
                                        conversationName, visibleId, dateCreated,
                                        memberIds, memberColors, notifyContactPersons);
                                    messages.ForEach(obj => conversation.AddMessage(obj));
                                    NewConversationArrived?.Invoke(this,
                                        new NewConversationArrivedEventArgs() {Conversation = conversation});
                                }
                            }
                            break;

                            #endregion

                            #region RemoveConversation

                        case MessageDictionary.RemoveConversation:
                            _writer.Write(MessageDictionary.OK);
                            conversationId = _reader.ReadString();
                            ConversationRemoved?.Invoke(this,
                                new ConversationRemovedEventArgs() {ConversationId = conversationId});
                            break;

                            #endregion

                            #region NewEmailAddress

                        case MessageDictionary.AddEmailAddress:
                            _writer.Write(MessageDictionary.OK);

                            emailId = _reader.ReadString();
                            emailAddress = _reader.ReadString();
                            string login = _reader.ReadString();
                            string imapHost = _reader.ReadString();
                            int imapPort = _reader.ReadInt32();
                            bool imapUseSel = _reader.ReadBoolean();
                            string smtpHost = _reader.ReadString();
                            int smtpPort = _reader.ReadInt32();
                            bool smtpUseSsl = _reader.ReadBoolean();
                            string addressName = _reader.ReadString();

                            NewEmailAddress?.Invoke(this, new NewEmailAddressEventArgs()
                            {
                                Id = emailId,
                                Login = login,
                                Address = emailAddress,
                                ImapHost = imapHost,
                                ImapPort = imapPort,
                                ImapUseSsl = imapUseSel,
                                SmtpHost = smtpHost,
                                SmtpPort = smtpPort,
                                SmtpUseSsl = smtpUseSsl,
                                Name = addressName
                            });

                            break;

                            #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
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

    public class ContactDetailsUpdatedEventArgs
    {
        public PersonModel NewData { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class CompanyRemovedEventArgs
    {
        public string CompanyId { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class CompanyRenamedEventArgs
    {
        public string CompanyId { get; set; }
        public string NewName { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class NewCompanyEventArgs
    {
        public CompanyModel Company { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class ConversationSettingsChangedEventArgs
    {
        public string ConversationId { get; set; }
        public bool NotifyContactPersons { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class ConversationRemovedEventArgs
    {
        public string ConversationId { get; set; }
    }

    public class ConversationMemberRemovedEventArgs
    {
        public string ConversationId { get; set; }
        public string PersonId { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class ConversationMembersAddedEventArgs : EventArgs
    {
        public string ConversationId { get; set; }
        public string PersonId { get; set; }
        public string PersonColor { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class NewConversationMessageArrivedEventArgs : EventArgs
    {
        public ConversationMessageModel Message { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class NewFilesArrivedEventArgs : EventArgs
    {
        public List<FileModel> Files { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class ConversationRenamedEventArgs : EventArgs
    {
        public string ConversationId { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class FileRenamedEventArgs : EventArgs
    {
        public string FileId { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }
        public NotificationModel Notification { get; set; }
    }

    public class NewConversationArrivedEventArgs
    {
        public ConversationModel Conversation { get; set; }
    }
}
