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
        public string Address { get; set; }
        public string Name { get; set; }
        #endregion

        #region Constructors
        public EmailAddressModel(string id, string address, string name)
        {
            Id = id;
            Address = address;
            Name = name;
        }
        #endregion
    }
}
