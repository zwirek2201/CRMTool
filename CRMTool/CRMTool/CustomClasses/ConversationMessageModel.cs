using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImapX;

namespace Licencjat_new.CustomClasses
{
    public class ConversationMessageModel
    {
        #region Properties
        public string Id { get; internal set; }

        public string ConversationId { get; set; }

        public PersonModel Author { get; set; }

        public string AuthorId { get; set; }

        public string AuthorFrom { get; set; }

        public DateTime? InitialDate { get; set; }

        public Boolean Received { get; set; }

        public Color Color { get; set; }

        public BitmapSource PreviewImage { get; set; }

        public Boolean Read { get; set; }

        public List<FileModel> Attachments { get; set; } = new List<FileModel>();
        public List<string> AttachmentsIds { get; set; } = new List<string>();

        public ConversationMessageModel() { }
        #endregion

        #region Constructors
        public ConversationMessageModel(string id, string conversationId, string authorId, string authorFrom, DateTime initialDate, Boolean received, BitmapImage previewImage, Boolean read, List<string> attachmentsIds)
        {
            Id = id;
            AuthorId = authorId;
            AuthorFrom = authorFrom;
            InitialDate = initialDate;
            Received = received;
            ConversationId = conversationId;
            PreviewImage = previewImage;
            Read = read;
            AttachmentsIds = attachmentsIds;
        }

        public ConversationMessageModel(PersonModel author, DateTime? initialDate)
        {
            Author = author;
            AuthorId = Author.Id;
            InitialDate = initialDate;
        }
        #endregion
    }

    public class ConversationEmailMessageModel : ConversationMessageModel
    {
        #region Properties
        public EmailAddressModel AuthorEmailaddress { get; set; }
        public string MessageSubject { get; set; }
        public string MessageContent { get; set; }
        #endregion

        #region Constructors
        public ConversationEmailMessageModel(ConversationMessageModel message, EmailAddressModel authorEmailaddress, string messageSubject, string messageContent)
        {
            Id = message.Id;
            ConversationId = message.ConversationId;
            AuthorId = message.AuthorId;
            AuthorFrom = message.AuthorFrom;
            InitialDate = message.InitialDate;
            AuthorEmailaddress = authorEmailaddress;
            Received = message.Received;
            PreviewImage = message.PreviewImage;
            MessageContent = messageContent;
            MessageSubject = messageSubject;
            AttachmentsIds = message.AttachmentsIds;
        }

        public ConversationEmailMessageModel(ConversationMessageModel message, string messageSubject, string messageContent)
        {
            AuthorId = message.AuthorId;
            ConversationId = message.ConversationId;
            AuthorFrom = message.AuthorFrom;
            InitialDate = message.InitialDate;
            Received = message.Received;
            PreviewImage = message.PreviewImage;
            MessageContent = messageContent;
            MessageSubject = messageSubject;
            AttachmentsIds = message.AttachmentsIds;
        }
        #endregion
    }

    public class ConversationPhoneMessageModel : ConversationMessageModel
    {
        #region Properties
        public string CallDescription { get; set; }
        public PhoneNumberModel AuthorPhoneNumber { get; set; }
        public bool CallAnswered { get; set; }
        public string RecipientPhoneNumberId { get; set; }
        public PhoneNumberModel RecipientPhoneNumber { get; set; }

        public PersonModel Recipient { get; set; }
        #endregion

        #region Constructors
        public ConversationPhoneMessageModel(ConversationMessageModel message, PhoneNumberModel authorPhoneNumber, PhoneNumberModel recipientPhoneNumber, string callDescription, bool callAnswered)
        {
            AuthorId = message.AuthorId;
            AuthorFrom = message.AuthorFrom;
            AuthorPhoneNumber = authorPhoneNumber;
            RecipientPhoneNumber = recipientPhoneNumber;
            InitialDate = message.InitialDate;
            Received = message.Received;
            CallDescription = callDescription;
            CallAnswered = callAnswered;
            PreviewImage = message.PreviewImage;
        }

        public ConversationPhoneMessageModel(ConversationMessageModel message, string recipientPhoneNumberId, string messageDescription, bool answered)
        {
            AuthorId = message.AuthorId;
            ConversationId = message.ConversationId;
            AuthorFrom = message.AuthorFrom;
            RecipientPhoneNumberId = recipientPhoneNumberId;
            InitialDate = message.InitialDate;
            Received = message.Received;
            PreviewImage = message.PreviewImage;
            CallDescription = messageDescription;
            CallAnswered = answered;
            AttachmentsIds = message.AttachmentsIds;
        }
        #endregion
    }
}
