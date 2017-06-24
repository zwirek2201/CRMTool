using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Licencjat_new.CustomClasses
{
    public static class RegistryHelper
    {
        #region Variables
        private const string _defaultDirectory = @"SOFTWARE\\CRMTool";
        #endregion

        #region Constructors
        public static string[] GetRegistryValueNames()
        {
            string[] values = Registry.CurrentUser.CreateSubKey(_defaultDirectory).GetValueNames();
            return values;
        }

        public static string GetRegistryValue(string name)
        {
            string value = (string)Registry.CurrentUser.CreateSubKey(_defaultDirectory).GetValue(name);
            return value;
        }

        public static void AddRegistryValue(string name, string value)
        {
         Registry.CurrentUser.CreateSubKey(_defaultDirectory).SetValue(name, value);
        }
        #endregion
    }
}
