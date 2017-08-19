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
        public string Number { get; set; }
        public string Name { get; set; }

        #endregion

        #region Constructors
        public PhoneNumberModel(string id, string number, string name)
        {
            Id = id;
            Number = number;
            Name = name;
        }
        #endregion
        public override string ToString()
        {
            return Name + " (" + Number + ")";
        }
    }
}
