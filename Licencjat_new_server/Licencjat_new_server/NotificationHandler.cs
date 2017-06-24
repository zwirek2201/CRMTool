using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public class NotificationHandler
    {
        private static List<NotificationRule> NotificationRules = new List<NotificationRule>()
        {
            new NotificationRule(NotificationType.MessageAdded,"{SenderId} dodał wiadomość do konwersacji {ConversationId}",new List<string>() { "Persons.FullName", "Conversations.Name"}),
            new NotificationRule(NotificationType.ConversationMemberAdded,"{SenderId} dodał użytkownika {PersonId} do konwersacji {ConversationId}",new List<string>() { "Persons.FullName", "Persons.FullName", "Conversations.Name",  }),
            new NotificationRule(NotificationType.ConversationMemberRemoved,"{SenderId} usunął użytkownika {PersonId} z konwersacji {ConversationId}",new List<string>() { "Persons.FullName", "Persons.FullName", "Conversations.Name",  }),
            new NotificationRule(NotificationType.RenamedConversation,"{SenderId} zmienił nazwę konwersacji [OldName] na [NewName]",new List<string>() { "Persons.FullName"}),
            new NotificationRule(NotificationType.RenamedFile,"{SenderId} zmienił nazwę pliku [OldName] na [NewName]",new List<string>() { "Persons.FullName"}),
            new NotificationRule(NotificationType.ConversationSettingsChanged,"{SenderId} zmienił ustawienia konwersacji {ConversationId}",new List<string>() { "Persons.FullName", "Conversations.Name"}),
            new NotificationRule(NotificationType.NewCompany,"{SenderId} dodał firmę {CompanyId} do listy kontaktów",new List<string>() { "Persons.FullName", "Companies.Name"}),
            new NotificationRule(NotificationType.RenamedCompany,"{SenderId} zmienił nazwę firmy [OldName] na [NewName]",new List<string>() { "Persons.FullName" })

        };

        public static NotificationModel ProcessNotification(NotificationResultInfo notification)
        {
            try
            {
                NotificationRule appliedRule = NotificationRules.Find(obj => obj.NotificationType == notification.Type);

                MatchCollection ruleObjects = Regex.Matches(appliedRule.NotificationRuleString, @"\{.[^{}]+\}");

                string processedString = appliedRule.NotificationRuleString;

                foreach (Match ruleObject in ruleObjects)
                {
                    string objectName = ruleObject.Value.Trim('{', '}');

                    processedString = processedString.Replace(objectName,
                        notification.GetType().GetProperty(objectName).GetValue(notification).ToString());
                }

                ruleObjects = Regex.Matches(appliedRule.NotificationRuleString, @"\[.[^\[\]]+\]");

                foreach (Match ruleObject in ruleObjects)
                {
                    string objectName = ruleObject.Value.Trim('[', ']');

                    processedString = processedString.Replace(objectName,
                        notification.GetType().GetProperty(objectName).GetValue(notification).ToString());
                }

                NotificationModel notificationModel = new NotificationModel(notification.Id, processedString,
                    appliedRule.NotificationRuleReferenceFields, notification.NotificationDate, notification.NotificationRead);

                return notificationModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    class NotificationRule
    {
        public NotificationType NotificationType { get; set; }

        public List<string> NotificationRuleReferenceFields { get; set; }

        public string NotificationRuleString { get; set; }

        public NotificationRule(NotificationType type, string ruleString, List<string> referenceFields)
        {
            NotificationType = type;
            NotificationRuleString = ruleString;
            NotificationRuleReferenceFields = referenceFields;
        }
    }

    public class NotificationModel
    {
        public string NotificationId { get; set; }
        public string NotificationText { get; set; }
        public List<string> NotificationReferenceFields { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool NotificationRead { get; set; }

        public NotificationModel(string notificationId, string notificationText, List<string> notificationReferenceFields, DateTime notificationDate, bool notificationRead)
        {
            NotificationId = notificationId;
            NotificationText = notificationText;
            NotificationReferenceFields = notificationReferenceFields;
            NotificationDate = notificationDate;
            NotificationRead = notificationRead;
        }
    }

    public enum NotificationType
    {
        MessageAdded = 1,
        FileShared = 2,
        ConversationMemberAdded = 3,
        ConversationMemberRemoved = 4,
        RenamedConversation = 5,
        RenamedFile = 6,
        ConversationSettingsChanged = 7,
        NewCompany,
        RenamedCompany
    }
}
