using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public static class MessageDictionary
    {
        //Basics
        public const byte Hello = 99;
        public const byte OK = 98;
        public const byte Error = 97;
        public const byte EndOfMessage = 96;
        public const byte Exists = 95;
        public const byte DoesNotExist = 94;

        //Users
        public const byte Login = 0;
        public const byte WrongPassword = 1;
        public const byte UserNotFound = 2;

        //Emails
        public const byte GetEmailAddresses = 10;
        public const byte SetLastDownloadedUid = 11;
        public const byte AddUnhandledMessages = 12;
        public const byte AddEmailAddress = 13;

        //Files
        public const byte ImUploadClient = 20;
        public const byte ImDownloadClient = 21;
        public const byte DownloadFile = 22;
        public const byte UploadFile = 23;
        public const byte GetFilesInfo = 24;
        public const byte NewFiles = 25;
        public const byte RenameFile = 26;

        //Conversations
        public const byte GetUserConversations = 30;
        public const byte GetConversationMessages = 31;
        public const byte CheckConversationExists = 32;
        public const byte RenameConversation = 33;
        public const byte NewConversation = 34;
        public const byte AddConversationMembers = 35;
        public const byte RemoveConversationMember = 36;
        public const byte RemoveConversation = 37;
        public const byte ChangeConversationSettings = 38;

        //ConversationMessages
        public const byte MessageTypeEmail = 40;
        public const byte MessageTypePhoneCall = 41;
        public const byte MessageTypeUnknown = 42;
        public const byte NewMessage = 43;
        public const byte HandleMessage = 44;

        //Contacts
        public const byte GetAllContacts = 50;
        public const byte UpdatePersonDetails = 51;
        public const byte NewExternalContact = 52;

        //Companies
        public const byte GetAllCompanies = 60;
        public const byte NewCompany = 61;
        public const byte RenameCompany = 62;
        public const byte RemoveCompany = 63;

        //Notifications
        public const byte ImNotificationClient = 70;
        public const byte NewNotification = 71;
        public const byte GetUserNotifications = 72;
        public const byte NotificationsRead = 73;
    }

    public static class ErrorMessageDictionary
    {
        private static List<ErrorMessage> _messages = new List<ErrorMessage>()
        {
            new ErrorMessage()
            {
                ErrorCode = MessageDictionary.WrongPassword,
                Message = "Podane hasło jest nieprawidłowe"
            },
            new ErrorMessage()
            {
                ErrorCode = MessageDictionary.UserNotFound,
                Message = "Podany użytkownik nie istnieje"
            }
        };

        public static string GetErrorMessageByCode(byte code)
        {
            return _messages.Find(msg => msg.ErrorCode == code).Message;
        }
    }

    internal class ErrorMessage
    {
        public byte ErrorCode { get; set; }

        public string Message { get; set; }

        public ErrorMessage(byte code, string message)
        {
            ErrorCode = code;
            Message = message;
        }

        public ErrorMessage()
        { }
    }
}
