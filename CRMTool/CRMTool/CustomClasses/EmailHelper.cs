using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImapX;
using ImapX.Authentication;

namespace Licencjat_new.CustomClasses
{
    static class EmailHelper
    {
        #region Variables
        private static List<ImapClient> _clients = new List<ImapClient>();
        #endregion

        #region Methods
        public static bool ConnectToServer(ImapClient client)
        {
            if (client.Connect())
            {
                _clients.Add(client);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ImapClient ConnectToServer(string server, int port = 993, bool useSsl = true)
        {
            ImapClient client = new ImapClient();
            client.Behavior.NoopIssueTimeout = 120;
            if (client.Connect(server, port, useSsl))
            {
                _clients.Add(client);
                return client;
            }
            else
            {
                return null;
            }
        }

        public static bool AuthenticateClient(ImapClient client, string username, string password)
        {
            try
            {
                return client.Login(username, password);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool OAuthenticateClient(ImapClient client, string username, string token)
        {
            OAuth2Credentials credentials = new OAuth2Credentials(username, token);
            return client.Login(credentials);
        }

        public static List<Folder> GetClientFolders(ImapClient client)
        {
            List<Folder> folders = client.Folders.ToList();
            if (folders.Count > 0)
            {
                return folders;
            }
            else
            {
                return null;
            }
        }

        public static ImageSource GetFolderIcon(Folder folder, ImapClient client)
        {
            if (folder == client.Folders.Inbox)
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/inbox.png"));
            if (folder == client.Folders.Archive || folder.Name == "Archives")
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/archive.png"));
            if (folder == client.Folders.Trash)
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/full_trash.png"));
            if (folder == client.Folders.Drafts || folder.Name == "Drafts")
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/drafts.png"));
            if (folder == client.Folders.Junk || folder.Name == "Junk" || folder.Name == "Spam")
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/junk.png"));
            if (folder == client.Folders.Sent || folder.Name == "Sent")
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/sent.png"));

            return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/folder.png"));
        }
        #endregion
    }

    public class EmailModel
    {
        #region Variables
        private int _unseenCount;
        public event EventHandler UnseenCountChanged;
        #endregion

        #region Properties
        public string Id { get; set; }
        public string Address { get; set; }
        public string Login { get; set; }
        public string ImapHost { get; set; }
        public int ImapPort { get; set; }
        public bool ImapUseSsl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string LastUid { get; set; }

        public List<string> UnhandledMessagesIds { get; set; }

        public List<Message> UnhandledMessages { get; set; } = new List<Message>();

        public int UnseenCount
        {
            get { return _unseenCount; }
            set
            {
                _unseenCount = value;
                UnseenCountChanged?.Invoke(this,EventArgs.Empty);
            }
        }

        public ImapClient ImapClient { get; set; }
        public SmtpClient SmtpClient { get; set; }

        public bool CannotConnect { get; set; } = false;
        #endregion

        #region Constructors
        public EmailModel(string id, string address, string login, string imapHost, int imapPort, bool imapUsessl, string smtpHost, int smtpPort, bool smtpUsessl, List<string> unhandledMessagesIds, string lastUid)
        {
            Id = id;
            Address = address;
            Login = login;
            ImapHost = imapHost;
            ImapPort = imapPort;
            ImapUseSsl = imapUsessl;
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SmtpUseSsl = smtpUsessl;
            LastUid = lastUid;
            UnhandledMessagesIds = unhandledMessagesIds;
        }
        #endregion
    }

  
}
