using MasterPassword.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfMasterPassword.Common;

namespace WpfMasterPassword.ViewModel
{
    public class ConfigurationSiteViewModel
    {
        public PropertyModel<string> SiteName { get; private set; }
        public PropertyModel<string> Login { get; private set; }
        public PropertyModel<int> Counter { get; private set; }
        public PropertyModel<PasswordType> TypeOfPassword { get; private set; }

        public GenericChangeDetection DetectChanges { get; private set; }

        private static Tuple<PasswordType, string>[] ThePasswordTypes = Enum.GetValues(typeof(PasswordType)).Cast<PasswordType>().Select(v => Tuple.Create(v, v.ToString())).ToArray();

        public Tuple<PasswordType, string>[] PasswordTypes { get { return ThePasswordTypes; } }

        public ConfigurationSiteViewModel()
        {
            SiteName = new PropertyModel<string>();
            Login = new PropertyModel<string>();
            Counter = new PropertyModel<int>(1); // default should be 1?
            TypeOfPassword = new PropertyModel<PasswordType>();

            DetectChanges = new GenericChangeDetection();
            DetectChanges.AddINotifyPropertyChanged(SiteName);
            DetectChanges.AddINotifyPropertyChanged(Login);
            DetectChanges.AddINotifyPropertyChanged(Counter);
            DetectChanges.AddINotifyPropertyChanged(TypeOfPassword);
        }
    }
}
