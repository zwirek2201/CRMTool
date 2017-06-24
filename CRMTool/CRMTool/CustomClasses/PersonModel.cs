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
        #region Properties
        public string Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }
        public Gender Gender { get; private set; }
        public CompanyModel Company { get; set; }
        public string CompanyId { get; private set; }
        public List<EmailAddressModel> EmailAddresses { get; private set; }
        public List<PhoneNumberModel> PhoneNumbers { get; private set; }
        #endregion

        #region Constructors
        public PersonModel(string id, string firstName, string lastName, Gender gender, string companyId)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            FullName = FirstName + " " + LastName;
            Gender = gender;
            CompanyId = companyId;
            EmailAddresses = new List<EmailAddressModel>();
            PhoneNumbers = new List<PhoneNumberModel>();
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
        #endregion
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }
}
