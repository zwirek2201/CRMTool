using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public class NotificationClient
    {
        public Program program { get; set; }

        private TcpClient _client;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private Stream _stream;

        public delegate void UserLogStateChangedEventHandler(object sender);

        public NotificationClient(TcpClient client, string userId)
        {
            _client = client;
            Logger.Log("New connection");

            Thread thread = new Thread(ClientSetup);
            thread.Start();
        }

        private void ClientSetup()
        {
            _stream = _client.GetStream();

            _reader = new BinaryReader(_stream, Encoding.UTF8);
            _writer = new BinaryWriter(_stream, Encoding.UTF8);

        }

        public void NewMessage(ConversationMessageResultInfo message, NotificationModel notification)
        {
            try
            {
                _writer.Write(MessageDictionary.NewNotification);

                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(notification.NotificationId);
                    _writer.Write(notification.NotificationText);
                    _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                    _writer.Write(notification.NotificationReferenceFields.Count);

                    foreach (string referenceField in notification.NotificationReferenceFields)
                    {
                        _writer.Write(referenceField);
                    }

                    _writer.Write(MessageDictionary.EndOfMessage);
                    _writer.Write(MessageDictionary.NewMessage);

                    _writer.Write(message.Id);
                    _writer.Write(message.ConversationId);
                    _writer.Write(message.AuthorId);
                    _writer.Write(message.AuthorFromId);
                    _writer.Write(
                        DateTime.ParseExact(message.InitialDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                            .ToString("dd-MM-yyyy HH:mm:ss"));

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
            }
            catch (Exception ex)
            {

            }
        }

        public void ConversationRenamed(string conversationId, string oldName, string newName,
            NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RenameConversation);

                _writer.Write(conversationId);
                _writer.Write(oldName);
                _writer.Write(newName);
            }
        }

        public void FileRenamed(string fileId, string oldName, string newName, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RenameFile);

                _writer.Write(fileId);
                _writer.Write(oldName);
                _writer.Write(newName);
            }
        }

        public void NewFiles(List<FileResultInfo> files)
        {
            try
            {
                _writer.Write(MessageDictionary.NewFiles);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    _writer.Write(files.Count);
                    foreach (FileResultInfo file in files)
                    {
                        _writer.Write(file.ConversationId);
                        _writer.Write(file.Id);
                        _writer.Write(file.Name);
                        _writer.Write(file.ContentType);
                        _writer.Write(file.Size);
                        _writer.Write(file.DateAdded.ToString("dd-MM-yyy"));
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void NewConversation(ConversationResultInfo conversation)
        {
            _writer.Write(MessageDictionary.NewConversation);
            if (_reader.Read() == MessageDictionary.OK)
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

                List<ConversationMessageResultInfo> conversationMessages =
                    DBApi.GetConversationMessages(conversation.Id);
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
            }
        }

        public void NewConversationMember(string conversationId, string personId, string personColor,
            NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.AddConversationMembers);

                _writer.Write(conversationId);
                _writer.Write(personId);
                _writer.Write(personColor);
            }
        }

        public void ConversationMemberRemoved(string conversationId, string memberId, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RemoveConversationMember);

                _writer.Write(conversationId);
                _writer.Write(memberId);
            }
        }

        public void RemoveConversation(string conversationId)
        {
            _writer.Write(MessageDictionary.RemoveConversation);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(conversationId);
            }
        }

        public void ConversationSettingsChanged(string conversationId, bool notifyContactPersons,
            NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.ChangeConversationSettings);

                _writer.Write(conversationId);
                _writer.Write(notifyContactPersons);
            }
        }

        public void NewCompany(string companyId, string companyName, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.NewCompany);

                _writer.Write(companyId);
                _writer.Write(companyName);
            }
        }

        public void CompanyRenamed(string companyId, string newName, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RenameCompany);

                _writer.Write(companyId);
                _writer.Write(newName);
            }
        }

        public void NewEmailAddress(string id, string newEmailAddress, string login, string imapHost, int imapPort,
            bool imapUseSsl, string smtpHost, int smtpPort, bool smtpUseSsl, string name)
        {
            _writer.Write(MessageDictionary.AddEmailAddress);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(id);
                _writer.Write(newEmailAddress);
                _writer.Write(login);
                _writer.Write(imapHost);
                _writer.Write(imapPort);
                _writer.Write(imapUseSsl);
                _writer.Write(smtpHost);
                _writer.Write(smtpPort);
                _writer.Write(smtpUseSsl);
                _writer.Write(name);
            }
        }

        public void CompanyRemoved(string companyId, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RemoveCompany);

                _writer.Write(companyId);
            }
        }

        public void PersonDetailsChanged(string id, string firstName, string lastName, int gender, string companyId,
            List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList,
            NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.UpdatePersonDetails);

                _writer.Write(id);
                _writer.Write(firstName);
                _writer.Write(lastName);
                _writer.Write(gender.ToString());
                _writer.Write(companyId);

                _writer.Write(emailAddressesList.Count);
                foreach (EmailAddressResultInfo emailAddress in emailAddressesList)
                {
                    _writer.Write(emailAddress.Id);
                    _writer.Write(emailAddress.Name);
                    _writer.Write(emailAddress.Address);
                }

                _writer.Write(phoneNumbersList.Count);
                foreach (PhoneNumberResultInfo phoneNumber in phoneNumbersList)
                {
                    _writer.Write(phoneNumber.Id);
                    _writer.Write(phoneNumber.Name);
                    _writer.Write(phoneNumber.Number);
                }
            }
        }

        private byte[] ReceiveFile()
        {
            byte[] buffer = new byte[1024*8];
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

            _writer.Write((Int64) data.Length);
            if (_reader.Read() == MessageDictionary.OK)
            {
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[1024*8];
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, count);
            }
        }

        public void NewExternalContact(string id, string firstName, string lastName, int gender, string companyId,
            List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList,
            NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.NewExternalContact);

                _writer.Write(id);
                _writer.Write(firstName);
                _writer.Write(lastName);
                _writer.Write(gender.ToString());
                _writer.Write(companyId);

                _writer.Write(emailAddressesList.Count);
                foreach (EmailAddressResultInfo emailAddress in emailAddressesList)
                {
                    _writer.Write(emailAddress.Id);
                    _writer.Write(emailAddress.Name);
                    _writer.Write(emailAddress.Address);
                }

                _writer.Write(phoneNumbersList.Count);
                foreach (PhoneNumberResultInfo phoneNumber in phoneNumbersList)
                {
                    _writer.Write(phoneNumber.Id);
                    _writer.Write(phoneNumber.Name);
                    _writer.Write(phoneNumber.Number);
                }
            }
        }

        public void ExternalContactRemoved(string personId, NotificationModel notification)
        {
            _writer.Write(MessageDictionary.NewNotification);

            if (_reader.Read() == MessageDictionary.OK)
            {
                _writer.Write(notification.NotificationId);
                _writer.Write(notification.NotificationText);
                _writer.Write(notification.NotificationDate.ToString("dd-MM-yyyy HH:mm:ss"));
                _writer.Write(notification.NotificationReferenceFields.Count);

                foreach (string referenceField in notification.NotificationReferenceFields)
                {
                    _writer.Write(referenceField);
                }

                _writer.Write(MessageDictionary.EndOfMessage);
                _writer.Write(MessageDictionary.RemoveExternalContact);

                _writer.Write(personId);
            }
        }
    }
}
