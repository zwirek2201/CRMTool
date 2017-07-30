using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    internal static class DB
    {
        private static string _host = "localhost";
        private static int _port;
        private static string _user = "sa";
        private static string _password = "sasa";
        private static string _initialCatalog = "CRMTool";
        private static bool _integratedSecurity = false;

        private static bool _isConnected;
        private static SqlConnection _connection;
        private static SqlConnectionStringBuilder _builder;

        public static bool IsConnected
        {
            get { return _isConnected; }
        }

        public static SqlConnection GetConnection()
        {
            if (_connection != null)
            {
                return _connection;
            }
            else
            {
                _builder = new SqlConnectionStringBuilder()
                {
                    DataSource = _host,
                    UserID = _user,
                    Password = _password,
                    IntegratedSecurity = _integratedSecurity,
                    InitialCatalog = _initialCatalog
                };

                try
                {
                    _connection = new SqlConnection(_builder.ConnectionString);
                    _connection.Open();
                    _isConnected = true;
                    return _connection;
                }
                catch (Exception ex)
                {
                    _connection = null;
                    _isConnected = false;
                    Logger.Log("ERROR: " + ex.StackTrace);
                    return null;
                }
            }
        }


        #region Commands

        //Select
        public static DataTable RunSelectCommand(String command, DataTable parameters = null)
        {
            if (command != "")
            {
                DataTable lvarTable = new DataTable();
                SqlConnection lvarConnection = GetConnection();
                SqlCommand lvarCommand = new SqlCommand(command, lvarConnection);
                if (parameters != null && parameters.Rows.Count > 0)
                {
                    foreach (DataRow lvarRow in parameters.Rows)
                    {
                        lvarCommand.Parameters.AddWithValue("@" + lvarRow[0], lvarRow[1]);
                    }
                }

                SqlDataReader lvarReader = lvarCommand.ExecuteReader();
                lvarTable.Load(lvarReader);
                lvarReader.Close();
                return lvarTable;
            }
            return null;
        }

        public static DataTable RunSelectCommand(String command, List<SqlParameter> parameters)
        {
            if (command != "")
            {
                DataTable lvarTable = new DataTable();
                SqlConnection lvarConnection = GetConnection();
                SqlCommand lvarCommand = new SqlCommand(command, lvarConnection);

                foreach(SqlParameter parameter in parameters)
                {
                    lvarCommand.Parameters.Add(parameter);
                }

                SqlDataReader lvarReader = lvarCommand.ExecuteReader();
                lvarTable.Load(lvarReader);
                lvarReader.Close();
                return lvarTable;
            }
            return null;
        }

        //Simple command
        public static void RunSimpleCommand(String command, DataTable parameters = null)
        {
            SqlConnection lvarConnection = GetConnection();
            SqlCommand lvarCommand = new SqlCommand(command, lvarConnection);
            if (parameters != null && parameters.Rows.Count > 0)
            {
                foreach (DataRow lvarRow in parameters.Rows)
                {
                    lvarCommand.Parameters.AddWithValue("@" + lvarRow[0], lvarRow[1]);
                }
            }
            lvarCommand.ExecuteNonQuery();
        }

        public static void RunSimpleCommand(String command, List<SqlParameter> parameters)
        {
            SqlConnection lvarConnection = GetConnection();
            SqlCommand lvarCommand = new SqlCommand(command, lvarConnection);

            foreach (SqlParameter parameter in parameters)
            {
                lvarCommand.Parameters.Add(parameter);
            }

            lvarCommand.ExecuteNonQuery();
        }

        public static SqlDataAdapter GetDataAdapter(String command, DataTable parameters = null)
        {
            SqlConnection lvarConnection = GetConnection();

            SqlCommand lvarCommand = new SqlCommand(command, lvarConnection);
            if (parameters != null && parameters.Rows.Count > 0)
            {
                foreach (DataRow lvarRow in parameters.Rows)
                {
                    lvarCommand.Parameters.AddWithValue("@" + lvarRow[0], lvarRow[1]);
                }
            }

            SqlDataAdapter lvarAdapter = new SqlDataAdapter(lvarCommand);
            SqlCommandBuilder lvarCommandBuilder = new SqlCommandBuilder(lvarAdapter);
            return lvarAdapter;
        }

        #endregion

        public static DataTable GetParametersDataTable()
        {
            DataTable lvarTable = new DataTable();
            lvarTable.Columns.Add("Name");
            lvarTable.Columns.Add("Value");
            return lvarTable;
        }
    }

    public static class DBApi
    {
        public static LoginResultInfo CheckUserCredentials(string login, string password)
        {
            Logger.Log($"Authenticating: {login}");
            string hashedLogin = Cryptography.HashString(login, 0);

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("Login", hashedLogin);

            DataTable users =
                DB.RunSelectCommand(
                    "SELECT t1.Id, t2.Id as PersonId, t2.FirstName, t2.LastName, t1.Password, t1.LastLoggedOut FROM Users as t1 join Persons as t2 on t2.Id = t1.Person WHERE Login = @Login",
                    parameters);
            if (users.Rows.Count > 0)
            {
                string passwordString = users.Rows[0]["Password"].ToString();
                string salt = passwordString.Substring(0, passwordString.IndexOf('|'));

                string hashedPassword = Cryptography.HashString(password, salt);

                if (hashedPassword == users.Rows[0]["Password"].ToString())
                {
                    string userId = users.Rows[0]["Id"].ToString();
                    string personId = users.Rows[0]["PersonId"].ToString();
                    string firstName = users.Rows[0]["FirstName"].ToString();
                    string lastName = users.Rows[0]["LastName"].ToString();
                    string lastLoggedOut = users.Rows[0]["LastLoggedOut"] == DBNull.Value ? "" : users.Rows[0]["LastLoggedOut"].ToString();

                    return new LoginResultInfo(MessageDictionary.OK, MessageDictionary.OK, userId, personId, firstName, lastName, lastLoggedOut);
                }
                else
                {
                    return new LoginResultInfo(MessageDictionary.Error, MessageDictionary.WrongPassword);
                }
            }
            else
            {
                return new LoginResultInfo(MessageDictionary.Error, MessageDictionary.UserNotFound);
            }
        }

        public static List<EmailResultInfo> GetUserEmailAddresses(string userId)
        {
            List<EmailResultInfo> emailResults = new List<EmailResultInfo>();

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("UserId", userId);

            DataTable emails =
                DB.RunSelectCommand(
                    "SELECT Id, Address, Login, ImapHost, ImapPort, ImapUseSsl, SmtpHost, SmtpPort, SmtpUseSsl, LastUidDownloaded FROM v_UserEmailAddresses WHERE [User] = @UserId",
                    parameters);

            DataTable unhandledMessagesTable =
                DB.RunSelectCommand("SELECT * FROM v_EmailAddressUnhandledMessages WHERE [User] = @UserId", parameters);

            foreach (DataRow email in emails.Rows)
            {
                string id = email["Id"].ToString();
                string address = email["Address"].ToString();
                string login = email["Login"].ToString();
                string imapHost = email["ImapHost"].ToString();
                int imapPort = Convert.ToInt32(email["ImapPort"]);
                bool imapUsessl = Convert.ToBoolean(email["ImapUseSsl"]);
                string smtpHost = email["SmtpHost"].ToString();
                int smtpPort = Convert.ToInt32(email["SmtpPort"]);
                bool smtpUsessl = Convert.ToBoolean(email["SmtpUseSsl"]);
                List<string> unhandledMessages = new List<string>();
 
                foreach (DataRow message in unhandledMessagesTable.Rows)
                {
                    if (message["EmailAddress"].ToString() == id)
                        unhandledMessages.Add(message["MessageUid"].ToString());
                }

                string lastUid = email["LastUidDownloaded"] != DBNull.Value ? email["LastUidDownloaded"].ToString() : "";

                EmailResultInfo result = new EmailResultInfo(id, address, login, imapHost, imapPort, imapUsessl, smtpHost, smtpPort, smtpUsessl, unhandledMessages, lastUid);
                emailResults.Add(result);
            }

            return emailResults;
        }

        public static List<ConversationResultInfo> GetUserConversations(string userId)
        {
            List<ConversationResultInfo> conversationResults = new List<ConversationResultInfo>();

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("UserId", userId);

            DataTable conversations = DB.RunSelectCommand("select t1.Conversation, t2.Name, t2.VisibleId, t2.DateStarted, t2.NotifyContactPersons, t3.Id as [User]  from PersonConversations t1 join Conversations t2 on t1.Conversation = t2.Id join Users t3 on t3.Person = t1.Person WHERE t3.Id = @UserId",
                parameters);

            foreach (DataRow conversation in conversations.Rows)
            {
                string id = conversation["Conversation"].ToString();
                string name = conversation["Name"].ToString();
                string visibleId = conversation["VisibleId"].ToString();
                string dateCreatedString =
                    DateTime.ParseExact(conversation["DateStarted"].ToString(), "dd.MM.yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm:ss");
                bool notifyContactPersons = Convert.ToBoolean(conversation["NotifyContactPersons"]);


                parameters.Clear();
                parameters.Rows.Add("ConversationId", id);
                DataTable conversationMembers =
                    DB.RunSelectCommand(
                        "SELECT * from v_PersonsInConversations WHERE Conversation = @ConversationId",
                        parameters);

                List<string> memberIds = new List<string>();
                List<string> memberColors = new List<string>();

                foreach (DataRow member in conversationMembers.Rows)
                {
                    memberIds.Add(member["Person"].ToString());
                    memberColors.Add(member["Color"].ToString());
                }

                ConversationResultInfo result = new ConversationResultInfo(id, name,
                    memberIds, memberColors, visibleId, dateCreatedString, notifyContactPersons);
                conversationResults.Add(result);
            }

            return conversationResults;
        }

        public static List<ConversationMessageResultInfo> GetConversationMessages(string conversationId)
        {
            List<ConversationMessageResultInfo> conversationResults = new List<ConversationMessageResultInfo>();

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("ConversationId", conversationId);

            DataTable messages =
                DB.RunSelectCommand("SELECT * FROM v_ConversationMessages WHERE [Conversation] = @ConversationId order by InitialDate asc",
                    parameters);

            DataTable attachments =
                DB.RunSelectCommand(
                    "SELECT * FROM v_ConversationMessageAttachments WHERE [Conversation] = @ConversationId", parameters);

            List<DataRow> attachmentList = attachments.Rows.Cast<DataRow>().ToList();

            foreach (DataRow message in messages.Rows)
            {
                string id = message["Message"].ToString();
                string authorId = message["AuthorId"].ToString();
                string authorFrom = message["AuthorFrom"].ToString();
                string initialDate = DateTime.ParseExact(message["InitialDate"].ToString(),"dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd-MM-" +
                                                                                                                                                         "yyyy" +
                                                                                                                                                         " HH:mm:ss");
                Boolean received = Convert.ToBoolean(message["Received"]);
                byte type = MessageDictionary.MessageTypeUnknown;
                byte[] previewImage = (byte[])message["PreviewData"];

                List<string> attachmentIds = attachmentList.Where(obj => obj["Message"].ToString() == id).Select(obj => obj["FileId"].ToString()).ToList();

                switch (message["MessageType"].ToString())
                {
                    case "1":
                        type = MessageDictionary.MessageTypeEmail;
                        break;
                    case "2":
                        type = MessageDictionary.MessageTypePhoneCall;
                        break;
                }


                ConversationMessageResultInfo result = new ConversationMessageResultInfo(id, authorId, authorFrom,
                    initialDate, received,
                    type, previewImage, attachmentIds);

                if (type == MessageDictionary.MessageTypeEmail)
                {
                    result.Subject = message["Subject"].ToString();
                    result.MessageContent = message["MessageContent"].ToString();
                }
                else if (type == MessageDictionary.MessageTypePhoneCall)
                {
                    result.CallDescription = message["Description"].ToString();
                    result.CallAnswered = Convert.ToBoolean(message["Answered"].ToString());
                    result.RecipientToId = message["RecipientTo"].ToString();

                }

                conversationResults.Add(result);
            }

            return conversationResults;
        }

        public static CheckExistsResultInfo CheckConversationExistsByVisibleId(string visibleConversationId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("ConversationVisibleId", visibleConversationId);

            DataTable result = DB.RunSelectCommand("exec CheckConversationExistsByVisibleId @ConversationVisibleId",
                parameters);

            bool exists = result.Rows[0]["ConversationId"] != DBNull.Value;
            string conversationId = null;

            if (exists)
                conversationId = result.Rows[0]["ConversationId"].ToString();

            return new CheckExistsResultInfo(exists, conversationId);
        }

        public static bool CheckConversationExistsById(string conversationId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("ConversationVisibleId", conversationId);

            DataTable result = DB.RunSelectCommand("exec CheckConversationExistsById @ConversationVisibleId",
                parameters);

            bool exists = result.Rows[0]["ConversationId"] != DBNull.Value;
            return exists;
        }

        public static CheckExistsResultInfo CheckPersonExistsByEmailAddress(string emailAddress)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("EmailAddress", emailAddress);

            DataTable result = DB.RunSelectCommand("exec CheckPersonExistsByEmailAddress @EmailAddress",
                parameters);

            bool exists = result.Rows[0]["PersonId"] != DBNull.Value;
            string personId = null;

            if (exists)
                personId = result.Rows[0]["PersonId"].ToString();

            return new CheckExistsResultInfo(exists, personId);
        }

        public static List<CompanyResultInfo> GetAllCompanies()
        {
            List<CompanyResultInfo> companyResults = new List<CompanyResultInfo>();

            DataTable companies = DB.RunSelectCommand("select Id, Name from Companies");

            foreach (DataRow company in companies.Rows)
            {
                CompanyResultInfo companyResultInfo = new CompanyResultInfo(company["Id"].ToString(),
                    company["Name"].ToString());

                companyResults.Add(companyResultInfo);
            }

            return companyResults;
        }

        public static List<PersonResultInfo> GetAllContacts()
        {
            List<PersonResultInfo> contactResults = new List<PersonResultInfo>();

            DataTable contacts = DB.RunSelectCommand("exec GetAllContacts");

            foreach (DataRow contact in contacts.Rows)
            {
                bool personExists = contactResults.Any(obj => obj.Id == contact["Id"].ToString());

                PersonResultInfo personInfo = null;

                if (!personExists)
                {
                    personInfo = new PersonResultInfo(contact["Id"].ToString(), contact["FirstName"].ToString(), contact["LastName"].ToString(), contact["Gender"].ToString(), contact["Company"].ToString(), null, null, Convert.ToBoolean(contact["IsInternalUser"]));
                    contactResults.Add(personInfo);
                }
                else
                {
                    personInfo = contactResults.Single(obj => obj.Id == contact["Id"].ToString());
                }

                if (contact["DetailValue"] != DBNull.Value)
                {
                    switch (contact["Type"].ToString())
                    {
                        case "EmailAddress":
                            EmailAddressResultInfo emailAddress =
                                new EmailAddressResultInfo(contact["DetailId"].ToString(),
                                    contact["DetailName"].ToString(), contact["DetailValue"].ToString());

                            personInfo.EmailAddresses.Add(emailAddress);
                            break;
                        case "PhoneNumber":
                            PhoneNumberResultInfo phoneNumber =
                                new PhoneNumberResultInfo(contact["DetailId"].ToString(),
                                    contact["DetailName"].ToString(), contact["DetailValue"].ToString());

                            personInfo.PhoneNumbers.Add(phoneNumber);
                            break;
                    }
                }
            }

            return contactResults;
        }

        public static string AddNewEmailMessage(string conversationId, string authorId, string authorEmailAddressId, string messageSubject,
            string messageContent, string initialDate, byte[] messagePreview, List<string> attachmentIds)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("conversationId", conversationId));
            parameters.Add(new SqlParameter("authorId", authorId));
            parameters.Add(new SqlParameter("initialDate", initialDate));

            DataTable returnTable = DB.RunSelectCommand(
                "INSERT INTO ConversationMessages (Author, Conversation, InitialDate) VALUES (@authorId, @conversationId, @initialDate); select scope_identity()", parameters);

            string messageId = returnTable.Rows[0][0].ToString();

            parameters.Clear();
            parameters.Add(new SqlParameter("messageId", messageId));
            parameters.Add(new SqlParameter("from", authorEmailAddressId));
            parameters.Add(new SqlParameter("subject", messageSubject));
            parameters.Add(new SqlParameter("messageContent", messageContent));

            DB.RunSimpleCommand("INSERT INTO EmailMessages(Message, [From], Subject, MessageContent) values (@messageId, @from, @subject, @messageContent)", parameters);

            parameters.Clear();
            parameters.Add(new SqlParameter("previewData", SqlDbType.VarBinary, messagePreview.Length) { Value = messagePreview });
            parameters.Add(new SqlParameter("messageId", messageId));

            DB.RunSimpleCommand(
                "INSERT INTO ConversationMessagePreviews (Id, [Message], PreviewData) values (newId(), @messageId, @previewData)", parameters);

            foreach (string attachmentId in attachmentIds)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("messageId", messageId));
                parameters.Add(new SqlParameter("fileId", attachmentId));

                DB.RunSimpleCommand(
                    "INSERT INTO ConversationMessageAttachments ([Message], [File]) values (@messageId, @fileId)",
                    parameters);
            }

            return messageId;
        }

        public static List<NotificationResultInfo> GetUserNotifications(string userId, int from, int count)
        {
            List<NotificationResultInfo> notificationResults = new List<NotificationResultInfo>();

            DataTable parameters = DB.GetParametersDataTable();

            parameters.Rows.Add("UserId", userId);
            parameters.Rows.Add("From", from);
            parameters.Rows.Add("Count", count);

            DataTable notificationTable = DB.RunSelectCommand(
                "SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY Date DESC) AS 'RowNumber' FROM Notifications WHERE Recipient = @UserId) as t1 WHERE RowNumber BETWEEN @From AND @Count",
                parameters);

            foreach (DataRow notification in notificationTable.Rows.Cast<DataRow>().Reverse())
            {
                NotificationResultInfo notificationResult = new NotificationResultInfo()
                {
                    Id = notification["Id"].ToString(),
                    SenderId = notification["Sender"].ToString(),
                    ConversationId = notification["ConversationId"].ToString(),
                    NotificationDate =
                        DateTime.ParseExact(notification["Date"].ToString(), "dd.MM.yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture),
                    NotificationRead = Convert.ToBoolean(notification["Read"]),
                    Type = (NotificationType)notification["Type"],
                    OldName = notification["OldName"].ToString(),
                    NewName = notification["NewName"].ToString(),
                    CompanyId = notification["CompanyId"].ToString(),
                    PersonId = notification["PersonId"].ToString()
                };

                notificationResults.Add(notificationResult);
            }

            return notificationResults;
        }

        public static string AddNewPhoneMessage(string conversationId, string authorId, string authorPhoneNumberId, string recipientPhoneNumberId, string callDescription,
    bool callAnswered, string initialDate, byte[] messagePreview, List<string> attachmentIds)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("conversationId", conversationId));
            parameters.Add(new SqlParameter("authorId", authorId));
            parameters.Add(new SqlParameter("initialDate", initialDate));

            DataTable returnTable = DB.RunSelectCommand(
                "INSERT INTO ConversationMessages (Author, Conversation, InitialDate) VALUES (@authorId, @conversationId, @initialDate); select scope_identity()", parameters);

            string messageId = returnTable.Rows[0][0].ToString();

            parameters.Clear();
            parameters.Add(new SqlParameter("to", recipientPhoneNumberId));
            parameters.Add(new SqlParameter("messageId", messageId));
            parameters.Add(new SqlParameter("from", authorPhoneNumberId));
            parameters.Add(new SqlParameter("description", callDescription));
            parameters.Add(new SqlParameter("answered", callAnswered));

            DB.RunSimpleCommand("INSERT INTO PhoneMessages(Message, [From], [To], Description, Answered) values (@messageId, @from, @to, @description, @answered)", parameters);

            parameters.Clear();
            parameters.Add(new SqlParameter("previewData", SqlDbType.VarBinary, messagePreview.Length) { Value = messagePreview });
            parameters.Add(new SqlParameter("messageId", messageId));

            DB.RunSimpleCommand(
                "INSERT INTO ConversationMessagePreviews (Id, [Message], PreviewData) values (newId(), @messageId, @previewData)", parameters);

            foreach (string attachmentId in attachmentIds)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("messageId", messageId));
                parameters.Add(new SqlParameter("fileId", attachmentId));

                DB.RunSimpleCommand(
                    "INSERT INTO ConversationMessageAttachments ([Message], [File]) values (@messageId, @fileId)",
                    parameters);
            }

            return messageId;
        }

        public static List<string> GetUsersSubscribedToConversation(string conversationId)
        {
            List<string> returnTable = new List<string>();

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationId", conversationId);

            DataTable personsTable =
                DB.RunSelectCommand(
                    "SELECT t2.Id as UserId from Persons t1 join Users t2 on t2.Person = t1.Id join PersonConversations t3 on t1.Id = t3.Person where t3.Conversation = @conversationId",
                    parameters);

            foreach (DataRow row in personsTable.Rows)
            {
                returnTable.Add(row["UserId"].ToString());
            }

            return returnTable;
        }

        public static List<string> GetAllUsers()
        {
            List<string> returnTable = new List<string>();

            DataTable personsTable =
                DB.RunSelectCommand(
                    "SELECT t2.Id as UserId from Persons t1 join Users t2 on t2.Person = t1.Id");

            foreach (DataRow row in personsTable.Rows)
            {
                returnTable.Add(row["UserId"].ToString());
            }

            return returnTable;
        }

        public static string AddNewNotification(NotificationResultInfo notification)
        {
            try
            {
                DataTable parameters = DB.GetParametersDataTable();

                parameters.Rows.Add("SenderId", notification.SenderId);
                parameters.Rows.Add("RecipientId", notification.RecipientId);
                parameters.Rows.Add("Type", Convert.ToInt32(notification.Type));
                parameters.Rows.Add("PersonId", notification.PersonId);
                parameters.Rows.Add("ConversationId", notification.ConversationId);
                parameters.Rows.Add("MessageId", notification.MessageId);
                parameters.Rows.Add("OldName", notification.OldName);
                parameters.Rows.Add("NewName", notification.NewName);
                parameters.Rows.Add("CompanyId", notification.CompanyId);
                parameters.Rows.Add("Date", notification.NotificationDate.ToString("yyyy-MM-dd HH:mm:ss"));

                DataTable returnTable = DB.RunSelectCommand(
                    "INSERT INTO Notifications (Type, Sender, Recipient, Date, PersonId, ConversationId, MessageId, OldName, NewName, CompanyId) VALUES (@Type, @SenderId, @RecipientId, @Date, @PersonId, @ConversationId, @MessageId, @OldName, @NewName, @CompanyId); select scope_identity();", parameters);

                return returnTable.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void ReportNotificationsRead(List<string> notificationIds)
        {
            DataTable parameters = DB.GetParametersDataTable();

            foreach (string notificationId in notificationIds)
            {
                parameters.Clear();
                parameters.Rows.Add("notificationId", notificationId);

                DB.RunSimpleCommand("UPDATE Notifications SET [Read] = 1 WHERE Id = @notificationId",parameters);
            }
        }

        public static void SetLastDownloadedUid(string emailAddress, string uid)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("emailAddress", emailAddress);
            parameters.Rows.Add("uid", uid);

            DB.RunSimpleCommand("UPDATE PersonEmailAddresses SET LastUidDownloaded = @uid WHERE Id = @emailAddress",
                parameters);
        }

        public static void AddUnhandledMessages(string emailAddress, List<string> messages)
        {
            DataTable parameters = DB.GetParametersDataTable();
            foreach (string message in messages)
            {
                parameters.Rows.Clear();
                parameters.Rows.Add("emailAddress", emailAddress);
                parameters.Rows.Add("messageUid", message);
                DB.RunSimpleCommand("INSERT INTO EmailAddressUnhandledMessages (EmailAddress, MessageUid) values (@emailAddress, @messageUid)", parameters);
            }
        }

        public static void HandleMessage(string emailAddress, string message)
        {
            DataTable parameters = DB.GetParametersDataTable();

            parameters.Rows.Add("emailAddress", emailAddress);
            parameters.Rows.Add("messageUid", message);

            DB.RunSimpleCommand(
                "DELETE FROM EmailAddressUnhandledMessages WHERE EmailAddress = @emailAddress AND MessageUid = @messageUid",
                parameters);
        }

        public static string NewFile(string fileName, string fileExtension, int fileSize, byte[] fileData, DateTime dateAdded)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("fileName", fileName));
            parameters.Add(new SqlParameter("fileExtension", fileExtension));
            parameters.Add(new SqlParameter("fileSize", fileSize));
            parameters.Add(new SqlParameter("fileDateAdded", dateAdded.ToString("yyyy-MM-dd")));
            parameters.Add(new SqlParameter("fileData", SqlDbType.VarBinary, fileData.Length) { Value = fileData });

            DataTable returnTable =
                DB.RunSelectCommand(
                    "INSERT INTO Files ([FileName], Extension, Size, DateAdded, RowId,  FileData) values (@fileName, @fileExtension, @fileSize, @fileDateAdded, NewId(), @fileData); SELECT SCOPE_IDENTITY();",
                    parameters);

            return returnTable.Rows[0][0].ToString();

        }

        public static List<FileResultInfo> GetFilesInfo(List<string> fileIds = null)
        {
            List<FileResultInfo> files = new List<FileResultInfo>();

            string query = "SELECT top 1 * FROM v_FilesInfo";

            if (fileIds != null && fileIds.Count > 0)
            {
                query += " WHERE FileId in (";

                foreach (string fileId in fileIds)
                {
                    query += "'" + fileId + "'" + (fileId == fileIds.Last() ? "" : ",");
                }

                query += ")";
            }

            DataTable returnTable = DB.RunSelectCommand(query);

            foreach (DataRow row in returnTable.Rows)
            {
                FileResultInfo file = new FileResultInfo()
                {
                    ConversationId = row["Conversation"].ToString(),
                    Id = row["FileId"].ToString(),
                    Name = row["FileName"].ToString(),
                    ContentType = row["ContentType"].ToString(),
                    DateAdded = DateTime.ParseExact(row["DateAdded"].ToString(),"dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    Size = Convert.ToInt32(row["Size"])
                };

                files.Add(file);
            }
            return files;
        }

        public static List<FileResultInfo> GetUserConversationFilesInfo(string userId)
        {
            List<FileResultInfo> files = new List<FileResultInfo>();

            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("userId", userId);

            string query = "SELECT * FROM v_FilesInfo WHERE Conversation in (select Conversation from PersonConversations where Person = (select Person from Users where Id = @userId))";

            DataTable returnTable = DB.RunSelectCommand(query, parameters);

            foreach (DataRow row in returnTable.Rows)
            {
                FileResultInfo file = new FileResultInfo()
                {
                    ConversationId = row["Conversation"].ToString(),
                    Id = row["FileId"].ToString(),
                    Name = row["FileName"].ToString(),
                    ContentType = row["ContentType"].ToString(),
                    DateAdded =
                        DateTime.ParseExact(row["DateAdded"].ToString(), "dd.MM.yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture),
                    Size = Convert.ToInt32(row["Size"])
                };

                files.Add(file);
            }
            return files;
        }

        public static void RenameConversation(string conversationId, string newName)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationId", conversationId);
            parameters.Rows.Add("newName", newName);

            DB.RunSimpleCommand("UPDATE Conversations SET [Name] = @newName WHERE Id = @conversationId", parameters);
        }

        public static void RenameFile(string fileId, string newName)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("fileId", fileId);
            parameters.Rows.Add("newName", newName);

            DB.RunSimpleCommand("UPDATE Files SET [FileName] = @newName WHERE Id = @fileId", parameters);
        }

        public static byte[] GetFileData(string fileId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("fileId", fileId);

            DataTable fileTable = DB.RunSelectCommand("SELECT FileData FROM Files WHERE Id = @fileId", parameters);

            return (byte[])fileTable.Rows[0]["FileData"];
        }

        public static ConversationResultInfo AddNewConversation(string conversationName)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationName", conversationName);

            DataTable conversation = DB.RunSelectCommand("declare @visibleId nvarchar(8);exec GetRandomConversationVisibleId @visibleId OUTPUT;INSERT INTO Conversations(DateStarted, Name, VisibleId) values(GETDATE(), 1, @conversationName, @visibleId);SELECT * FROM Conversations WHERE Id = (select scope_identity())", parameters);
            return new ConversationResultInfo(
                conversation.Rows[0]["Id"].ToString(), conversation.Rows[0]["Name"].ToString(), new List<string>(),
                new List<string>(), conversation.Rows[0]["VisibleId"].ToString(), DateTime.ParseExact(conversation.Rows[0]["DateStarted"].ToString(), "dd.MM.yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm:ss"), false);

        }

        public static List<string> AddConversationMembers(List<string> personIds, string conversationId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            List<string> personColors = new List<string>();
            foreach (string personId in personIds)
            {
                parameters.Rows.Clear();
                parameters.Rows.Add("personId", personId);
                parameters.Rows.Add("conversationId", conversationId);

                DataTable colorTable =
                    DB.RunSelectCommand(
                        "INSERT INTO PersonConversations (Conversation, Person, DateJoined, LastChecked, Muted, Color) values (@conversationId, @personId, GETDATE(),NULL, 0,(SELECT TOP 1 Id from Colors WHERE Id not in (Select Color FROM PersonConversations WHERE Conversation = @conversationId))); SELECT Hex from PersonConversations t1 join Colors t2 on t2.Id = t1.Color where Conversation = @conversationId AND Person = @personId",
                        parameters);
                personColors.Add(colorTable.Rows[0]["Hex"].ToString());
            }
            return personColors;
        }

        public static ConversationResultInfo GetUserConversation(string conversationId, string userId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationId", conversationId);
            parameters.Rows.Add("userId", userId);

            DataTable conversations = DB.RunSelectCommand("select t1.Conversation, t2.NotifyContactPersons, t2.Name, t2.VisibleId, t2.DateStarted, t3.Id as [User]  from PersonConversations t1 join Conversations t2 on t1.Conversation = t2.Id join Users t3 on t3.Person = t1.Person WHERE Conversation = @conversationId AND t3.Id = @userId",
                parameters);

            string id = conversationId;
            string name = conversations.Rows[0]["Name"].ToString();
            string visibleId = conversations.Rows[0]["VisibleId"].ToString();
            string dateCreatedString =
                DateTime.ParseExact(conversations.Rows[0]["DateStarted"].ToString(), "dd.MM.yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm:ss");
            bool notifyContactPersons = Convert.ToBoolean(conversations.Rows[0]["NotifyContactPersons"]);


            parameters.Clear();
            parameters.Rows.Add("conversationId", conversationId);

            DataTable conversationMembers =
                    DB.RunSelectCommand(
                        "SELECT * from v_PersonsInConversations WHERE Conversation = @conversationId",
                        parameters);

                List<string> memberIds = new List<string>();
                List<string> memberColors = new List<string>();

                foreach (DataRow member in conversationMembers.Rows)
                {
                    memberIds.Add(member["Person"].ToString());
                    memberColors.Add(member["Color"].ToString());
                }

                ConversationResultInfo result = new ConversationResultInfo(id, name,
                    memberIds, memberColors, visibleId, dateCreatedString, notifyContactPersons);

            return result;
        }

        public static void RemoveConversationMember(string conversationId, string memberId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationId", conversationId);
            parameters.Rows.Add("personId", memberId);

            DB.RunSimpleCommand(
                "DELETE FROM Notifications WHERE [Recipient] = (SELECT Id from [Users] WHERE [Person] = @personId) AND ConversationId = @conversationId",
                parameters);

            DB.RunSimpleCommand(
                "DELETE FROM PersonConversations WHERE Conversation = @conversationId AND [Person] = @personId", parameters);
        }

        public static void ChangeConversationSettings(string conversationId, bool notifyContactPersons)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("conversationId", conversationId);
            parameters.Rows.Add("notifyContactPersons", notifyContactPersons);

            DB.RunSimpleCommand("UPDATE Conversations SET NotifyContactPersons = @notifyContactPersons WHERE Id = @conversationId", parameters);
        }

        public static string AddCompany(string companyName)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("companyName", companyName);

            DataTable returnTable = DB.RunSelectCommand("INSERT INTO Companies ([Name]) values (@companyName); select scope_identity()", parameters);
            return returnTable.Rows[0][0].ToString();
        }

        public static void RenameCompany(string companyId, string newName)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("companyId", companyId);
            parameters.Rows.Add("newName", newName);

            DB.RunSimpleCommand("UPDATE Companies SET [Name] = @newName WHERE Id = @companyId", parameters);
        }

        public static string AddEmailAddress(string personId, string emailAddress, string login, string imapHost, int imapPort, bool imapUseSsl, string smtpHost, int smtpPort, bool smtpUseSsl, string name)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("emailAddress", emailAddress);
            if(login == "")
                parameters.Rows.Add("login",  DBNull.Value);
            else
                parameters.Rows.Add("login", login);

            parameters.Rows.Add("imapHost", imapHost);
            parameters.Rows.Add("imapPort", imapPort);
            parameters.Rows.Add("imapUseSsl", imapUseSsl);
            parameters.Rows.Add("smtpHost", smtpHost);
            parameters.Rows.Add("smtpPort", smtpPort);
            parameters.Rows.Add("smtpUseSsl", smtpUseSsl);
            parameters.Rows.Add("name", name);
            parameters.Rows.Add("personId", personId);

            DataTable returnTable = DB.RunSelectCommand("INSERT INTO PersonEmailAddresses ([Address], [Login], [ImapHost], [ImapPort], [ImapUseSsl], [Person], [SmtpHost], [SmtpPort], [SmtpUseSsl], [Name]) values (@emailAddress, @login, @imapHost, @imapPort, @imapUseSsl, @personId, @smtpHost, @smtpPort, @smtpUseSsl, @name); select scope_identity()", parameters);
            return returnTable.Rows[0][0].ToString();
        }

        public static string RemoveCompany(string companyId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("companyId", companyId);

            DB.RunSimpleCommand("delete from ContactPersons WHERE Company = @companyId", parameters);
            DB.RunSimpleCommand("delete from Notifications WHERE CompanyId = @companyId", parameters);
            DataTable returnTable = DB.RunSelectCommand("select [Name] from Companies where Id = @companyId", parameters);
            DB.RunSimpleCommand("delete from Companies WHERE Id = @companyId", parameters);

            return returnTable.Rows[0][0].ToString();
        }

        public static void UpdatePersonDetails(string id, string firstName, string lastName, int gender, string companyId, List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("id", id);
            parameters.Rows.Add("firstName", firstName);
            parameters.Rows.Add("lastName", lastName);
            parameters.Rows.Add("gender", gender);
            DB.RunSimpleCommand("update Persons set FirstName = @firstName, LastName = @lastName, Gender = @gender WHERE Id = @id", parameters);

            if (companyId != "")
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);
                parameters.Rows.Add("companyId", companyId);

                DB.RunSimpleCommand(
                    "IF EXISTS (Select * from ContactPersons where Person = @personId) UPDATE ContactPersons Set Company = @companyId WHERE Person = @personId ELSE INSERT INTO ContactPersons (Person, Company) values (@personId, @companyId)",
                    parameters);
            }
            else
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);

                DB.RunSimpleCommand("delete from ContactPersons where Person = @personId",parameters);
            }

            if (emailAddressesList != null)
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);

                DataTable personEmailAddresses =
                    DB.RunSelectCommand(
                        "SELECT Id, [Name], [Address] FROM PersonEmailAddresses WHERE Person = @personId", parameters);

                List<EmailAddressResultInfo> emailsToAdd = new List<EmailAddressResultInfo>();
                List<EmailAddressResultInfo> emailsToUpdate = new List<EmailAddressResultInfo>();
                List<string> emailsToDelete = new List<string>();


                foreach (DataRow row in personEmailAddresses.Rows)
                {
                    EmailAddressResultInfo email = emailAddressesList.Find(obj => obj.Id == row[0].ToString());
                    if (email == null)
                    {
                        emailsToDelete.Add(row[0].ToString());
                    }
                    else
                    {
                        emailsToUpdate.Add(email);
                    }
                }

                foreach (EmailAddressResultInfo email in emailAddressesList)
                {
                    bool exists = false;

                    for (int i = 0; i < personEmailAddresses.Rows.Count; i++)
                    {
                        if (personEmailAddresses.Rows[i][0].ToString() == email.Id)
                        {
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        emailsToAdd.Add(email);
                    }
                }

                foreach (EmailAddressResultInfo email in emailsToAdd)
                {
                    parameters.Clear();
                    parameters.Rows.Add("personId", id);
                    parameters.Rows.Add("emailName", email.Name);
                    parameters.Rows.Add("emailAddress", email.Address);
                    DataTable returnTable =
                        DB.RunSelectCommand(
                            "insert into PersonEmailAddresses ([Person], [Name], [Address]) values (@personId, @emailName, @emailAddress); select scope_identity()",
                            parameters);
                    email.Id = returnTable.Rows[0][0].ToString();
                }

                foreach (EmailAddressResultInfo email in emailsToUpdate)
                {
                    parameters.Clear();
                    parameters.Rows.Add("emailId", email.Id);
                    parameters.Rows.Add("emailName", email.Name);
                    parameters.Rows.Add("emailAddress", email.Address);
                    DB.RunSimpleCommand(
                        "update PersonEmailAddresses set [Name] = @emailName, Address = @emailAddress WHERE Id = @emailId",
                        parameters);
                }

                foreach (string email in emailsToDelete)
                {
                    parameters.Clear();
                    parameters.Rows.Add("emailId", email);
                    parameters.Rows.Add("personId", id);
                    DB.RunSimpleCommand("delete from PersonEmailAddresses WHERE Id = @emailId", parameters);
                }
            }

            // PHONES

            parameters.Clear();
            parameters.Rows.Add("personId", id);

            DataTable personPhoneNumbers = DB.RunSelectCommand("SELECT Id, [Name], [PhoneNumber] FROM PersonPhoneNumbers WHERE Person = @personId", parameters);

            List<PhoneNumberResultInfo> phonesToAdd = new List<PhoneNumberResultInfo>();
            List<PhoneNumberResultInfo> phonesToUpdate = new List<PhoneNumberResultInfo>();
            List<string> phonesToDelete = new List<string>();

            foreach (DataRow row in personPhoneNumbers.Rows)
            {
                PhoneNumberResultInfo phone = phoneNumbersList.Find(obj => obj.Id == row[0].ToString());
                if (phone == null)
                {
                    phonesToDelete.Add(row[0].ToString());
                }
                else
                {
                    phonesToUpdate.Add(phone);
                }
            }

            foreach (PhoneNumberResultInfo phone in phoneNumbersList)
            {
                bool exists = false;

                for (int i = 0; i < personPhoneNumbers.Rows.Count; i++)
                {
                    if (personPhoneNumbers.Rows[i][0].ToString() == phone.Id)
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    phonesToAdd.Add(phone);
                }
            }

            foreach (PhoneNumberResultInfo phone in phonesToAdd)
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);
                parameters.Rows.Add("phoneName", phone.Name);
                parameters.Rows.Add("phoneNumber", phone.Number);
                DataTable returnTable = DB.RunSelectCommand("insert into PersonPhoneNumbers ([Person], [Name], [PhoneNumber]) values (@personId, @phoneName, @phoneNumber); select scope_identity()", parameters);

                phone.Id = returnTable.Rows[0][0].ToString();
            }

            foreach (PhoneNumberResultInfo phone in phonesToUpdate)
            {
                parameters.Clear();
                parameters.Rows.Add("phoneId", phone.Id);
                parameters.Rows.Add("phoneName", phone.Name);
                parameters.Rows.Add("phoneNumber", phone.Number);
                DB.RunSimpleCommand("update PersonPhoneNumbers set [Name] = @phoneName, PhoneNumber = @phoneNumber WHERE Id = @phoneId", parameters);
            }

            foreach (string phone in phonesToDelete)
            {
                parameters.Clear();
                parameters.Rows.Add("phoneId", phone);
                DB.RunSimpleCommand("delete from PersonPhoneNumbers WHERE Id = @phoneId", parameters);
            }
        }

        public static string NewExternalContact(string firstName, string lastName, int gender, string companyId, List<EmailAddressResultInfo> emailAddressesList, List<PhoneNumberResultInfo> phoneNumbersList)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("firstName", firstName);
            parameters.Rows.Add("lastName", lastName);
            parameters.Rows.Add("gender", gender);
            DataTable returnTable = DB.RunSelectCommand("insert into Persons (FirstName, LastName, Gender) values (@firstName, @lastName, @gender);select scope_identity()", parameters);

            string id = returnTable.Rows[0][0].ToString();

            if (companyId != "")
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);
                parameters.Rows.Add("companyId", companyId);

                DB.RunSimpleCommand("INSERT INTO ContactPersons (Person, Company) values (@personId, @companyId)",
                    parameters);
            }

            foreach (EmailAddressResultInfo email in emailAddressesList)
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);
                parameters.Rows.Add("emailName", email.Name);
                parameters.Rows.Add("emailAddress", email.Address);
                returnTable = DB.RunSelectCommand("insert into PersonEmailAddresses ([Person], [Name], [Address]) values (@personId, @emailName, @emailAddress); select scope_identity()", parameters);
                email.Id = returnTable.Rows[0][0].ToString();
            }

            foreach (PhoneNumberResultInfo phone in phoneNumbersList)
            {
                parameters.Clear();
                parameters.Rows.Add("personId", id);
                parameters.Rows.Add("phoneName", phone.Name);
                parameters.Rows.Add("phoneNumber", phone.Number);
                returnTable = DB.RunSelectCommand("insert into PersonPhoneNumbers ([Person], [Name], [PhoneNumber]) values (@personId, @phoneName, @phoneNumber); select scope_identity()", parameters);

                phone.Id = returnTable.Rows[0][0].ToString();
            }

            return id;
        }

        public static string RemoveExternalContact(string personId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("personId", personId);

            DB.RunSimpleCommand("delete from Notifications where PersonId = @personId", parameters);
            DB.RunSimpleCommand("delete from ContactPersons where Person = @personId", parameters);
            DB.RunSimpleCommand("delete from PersonEmailAddresses where Person = @personId", parameters);
            DB.RunSimpleCommand("delete from PersonPhoneNumbers where Person = @personId", parameters);
            DataTable returnTable = DB.RunSelectCommand("select FirstName, LastName from Persons where Id = @personId", parameters);
            DB.RunSimpleCommand("delete from Persons where Id = @personId", parameters);

            return returnTable.Rows[0][0] + " " + returnTable.Rows[0][1];
        }

        public static void RemoveConversation(string converastionId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("converastionId", converastionId);

            DB.RunSimpleCommand("delete from Notifications where ConversationId = @converastionId", parameters);
            DB.RunSimpleCommand("delete from PersonConversations where Conversation = @converastionId", parameters);
            DB.RunSimpleCommand("delete from Conversations where Id = @converastionId", parameters);
        }

        public static string GetConversationName(string conversationId)
        {
            DataTable parameters = DB.GetParametersDataTable();
            parameters.Rows.Add("converastionId", conversationId);

            DataTable returnTable = DB.RunSelectCommand("select Name from Conversations where Id = @converastionId", parameters);

            return returnTable.Rows[0][0].ToString();
        }
    }

    public class EmailResultInfo
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Login { get; set; }
        public string ImapHost { get; set; }
        public int ImapPort { get; set; }
        public bool ImapUseSsl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpUseSsl { get; set; }
        public List<string> UnhandledMessages { get; set; }
        public string LastUid { get; set; }

        public EmailResultInfo(string id, string address, string login, string imapHost, int imapPort, bool imapUsessl, string smtpHost, int smtpPort, bool smtpUsessl, List<string> unhandledMessages, string lastUid)
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
            UnhandledMessages = unhandledMessages;
            LastUid = lastUid;
        }
    }

    public class LoginResultInfo
    {
        public byte Status { get; set; }
        public byte Error { get; set; }
        public string UserId { get; set; }
        public string PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastLoggedOut { get; set; }

        public LoginResultInfo(byte status, byte error)
        {
            Status = status;
            Error = error;
        }

        public LoginResultInfo(byte status, byte error, string userId, string personId, string firstName, string lastName, string lastLoggedOut)
        {
            Status = status;
            Error = error;
            UserId = userId;
            PersonId = personId;
            FirstName = firstName;
            LastName = lastName;
            LastLoggedOut = lastLoggedOut;
        }
    }

    public class ConversationResultInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VisibleId { get; set; }
        public string DateStarted { get; set; }
        public List<string> MemberIds { get; set; }
        public List<string> MemberColors { get; set; }
        public bool NotifyContactPersons { get; set; }

        public ConversationResultInfo(string id, string name, List<string> memberIds, List<string> memberColors, string visibleId, string dateStarted, bool notifyContactPersons)
        {
            Id = id;
            Name = name;
            MemberIds = memberIds;
            MemberColors = memberColors;
            VisibleId = visibleId;
            DateStarted = dateStarted;
            NotifyContactPersons = notifyContactPersons;
        }
    }

    public class ConversationMessageResultInfo
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorFromId { get; set; }
        public string InitialDate { get; set; }
        public Boolean Received { get; set; }
        public byte MessageType { get; set; }
        public string Subject { get; set; }
        public string MessageContent { get; set; }
        public string PhoneNumber { get; set; }
        public string CallDescription { get; set; }
        public bool CallAnswered { get; set; }
        public byte[] PreviewImage { get; set; }
        public List<string> AttachmentIds { get; set; }
        public string RecipientToId { get; set; }

        public ConversationMessageResultInfo(string id, string authorId, string authorFromId, string initialDate, Boolean received,
            byte messageType, byte[] previewImage, List<string> attachmentIds)
        {
            Id = id;
            AuthorId = authorId;
            AuthorFromId = authorFromId;
            InitialDate = initialDate;
            Received = received;
            MessageType = messageType;
            PreviewImage = previewImage;
            AttachmentIds = attachmentIds;
        }
    }

    public class CheckExistsResultInfo
    {
        public bool Exists { get; set; }
        public string Id { get; set; }

        public CheckExistsResultInfo(bool exists, string id)
        {
            Exists = exists;
            Id = id;
        }
    }

    public class CompanyResultInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public CompanyResultInfo(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class PersonResultInfo
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string CompanyId { get; set; }
        public List<EmailAddressResultInfo> EmailAddresses { get; set; }
        public List<PhoneNumberResultInfo> PhoneNumbers { get; set; }
        public bool IsInternalUser { get; set; }

        public PersonResultInfo(string id, string firstName, string lastName, string gender, string companyId,
            List<EmailAddressResultInfo> emailAddresses, List<PhoneNumberResultInfo> phoneNumbers, bool isInternalUser)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            CompanyId = companyId;
            EmailAddresses = emailAddresses ?? new List<EmailAddressResultInfo>();
            PhoneNumbers = phoneNumbers ?? new List<PhoneNumberResultInfo>();
            IsInternalUser = isInternalUser;
        }
    }

    public class EmailAddressResultInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public EmailAddressResultInfo(string id, string name, string address)
        {
            Id = id;
            Name = name;
            Address = address;
        }
    }

    public class PhoneNumberResultInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }

        public PhoneNumberResultInfo(string id, string name, string number)
        {
            Id = id;
            Name = name;
            Number = number;
        }
    }

    public class NotificationResultInfo
    {
        public string Id { get; set; }
        public NotificationType Type { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public string PersonId { get; set; }
        public string MessageId { get; set; }
        public string ConversationId { get; set;}
        public string FileId { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }
        public string CompanyId { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool NotificationRead { get; set; }
    }

    public class FileResultInfo
    {
        public string ConversationId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public DateTime DateAdded { get; set; }
        public long Size { get; set; }
    }
}
