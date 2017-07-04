using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new.CustomClasses
{
    public class CompanyModel : object
    {
        private string _name;
        public event EventHandler DataChanged;

        #region Properties
        public string Id { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<PersonModel> Persons { get; private set; }
        #endregion

        #region Constructors
        public CompanyModel(string id, string name)
        {
            Id = id;
            _name = name;
        }
        #endregion
    }
}
