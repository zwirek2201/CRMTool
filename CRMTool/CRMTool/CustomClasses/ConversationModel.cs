using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Licencjat_new.CustomClasses
{
    public class ConversationModel : object
    {
        #region Variables
        private string _conversationName;

        public event EventHandler DataChanged;
        #endregion

        #region Properties
        public string Id { get; private set; }
        public string Name
        {
            get { return _conversationName; }
            set
            {
                _conversationName = value;
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string VisibleId { get; set; }
        public DateTime DateCreated { get; set; } 
        public List<string> MemberIds { get; private set; }
        public List<string> MemberColors { get; private set; } = new List<string>();
        public List<PersonModel> Members { get; private set; } = new List<PersonModel>();
        public Dictionary<PersonModel, Color> ColorDictionary { get; private set; } = new Dictionary<PersonModel, Color>();
        public List<ConversationMessageModel> Messages { get; set; }
        public bool NotifyContactPersons { get; set; }
        #endregion

        #region Constructors
        public ConversationModel(string id, string conversationName, string visibleId, DateTime dateCreated, List<string> memberIds, List<string> memberColors, bool notifyContactPersons)
        {
            Id = id;
            Name = conversationName;
            MemberIds = memberIds;
            MemberColors = memberColors;
            VisibleId = visibleId;
            DateCreated = dateCreated;
            Messages = new List<ConversationMessageModel>();
            NotifyContactPersons = notifyContactPersons;
        }
        #endregion

        #region Methods
        public void AddMessage(ConversationMessageModel message)
        {
            if (!Messages.Contains(message))
                Messages.Add(message);
        }
        #endregion

        public void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
