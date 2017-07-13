using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Licencjat_new.CustomClasses
{
    public class PersonModel: object
    {
        private string _firstName;
        private string _lastName;
        private Gender _gender;
        private CompanyModel _company;
        private List<EmailAddressModel> _emailAddresses;
        private List<PhoneNumberModel> _phoneNumbers;

        #region Properties
        public string Id { get; private set; }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
            }
        }
        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
            }
        }

        public string FullName { get; private set; }

        public Gender Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
            }
        }

        public CompanyModel Company
        {
            get { return _company; }
            set
            {
                _company = value;
            }
        }

        public string CompanyId { get; private set; }

        public List<EmailAddressModel> EmailAddresses
        {
            get { return _emailAddresses; }
            set
            {
                _emailAddresses = value;
            }
            
        }

        public List<PhoneNumberModel> PhoneNumbers
        {
            get { return _phoneNumbers; }
            set
            {
                _phoneNumbers = value;
            }
        }

        public bool IsInternalUser { get; set; }
        #endregion

        public event EventHandler DataChanged;

        #region Constructors
        public PersonModel(string id, string firstName, string lastName, Gender gender, string companyId, bool isInternalUser)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            FullName = FirstName + " " + LastName;
            Gender = gender;
            CompanyId = companyId;
            IsInternalUser = isInternalUser;
            EmailAddresses = new List<EmailAddressModel>();
            PhoneNumbers = new List<PhoneNumberModel>();
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        public void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }
}
