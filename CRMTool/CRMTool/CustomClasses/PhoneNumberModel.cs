using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new.CustomClasses
{
    public class PhoneNumberModel : object
    {
        #region Properties
        public string Id { get; private set; }
        public string Number { get; private set; }
        public string Name { get; private set; }
        public bool Active { get; private set; }

        public bool Default { get; private set; }
        #endregion

        #region Constructors
        public PhoneNumberModel(string id, string number, string name, bool active, bool defaultPhoneNumber)
        {
            Id = id;
            Number = number;
            Name = name;
            Active = active;
            Default = defaultPhoneNumber;
        }
        #endregion
        public override string ToString()
        {
            return Name + " (" + Number + ")";
        }
    }
}
