using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new.CustomClasses
{
    public class EmailAddressModel
    {
        #region Properties
        public string Id { get; private set; }
        public string Address { get; private set; }
        public string Name { get; private set; }
        public bool Active { get; private set; }
        public bool Default { get; private set; }
        #endregion

        #region Constructors
        public EmailAddressModel(string id, string address, string name, bool active, bool defaultAddress)
        {
            Id = id;
            Address = address;
            Name = name;
            Active = active;
            Default = defaultAddress;
        }
        #endregion
    }
}
