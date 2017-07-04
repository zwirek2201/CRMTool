using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new.CustomClasses
{
    public class NotificationModel
    {
        #region Properties
        public string Id { get; set; }
        public string Text { get; set; }
        public string RuleString { get; private set; }
        public List<string> ReferenceFields { get; private set; }
        public bool Read { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool Local { get; set; }
        public bool Ghost { get; set; } = false;
        #endregion

        #region Constructors
        public NotificationModel(string id, string ruleString, List<string> referenceFields, DateTime notificationDate, bool notificationRead, bool local = false)
        {
            Id = id;
            RuleString = ruleString;
            ReferenceFields = referenceFields;
            NotificationDate = notificationDate;
            Read = notificationRead;
            Local = local;
        }
        #endregion
    }
}
