using MasterPassword.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfMasterPassword.Common;

namespace WpfMasterPassword.ViewModel
{
    public class SiteViewModel
    {
        public PropertyModel<string> SiteName { get; private set; }
        public PropertyModel<string> Login { get; private set; }
        public PropertyModel<int> Counter { get; private set; }
        public PropertyModel<PasswordType> Type { get; private set; }

        public GenericChangeDetection DetectChanges { get; private set; }

        public SiteViewModel()
        {
            SiteName = new PropertyModel<string>();
            Login = new PropertyModel<string>();
            Counter = new PropertyModel<int>();
            Type = new PropertyModel<PasswordType>();

            DetectChanges = new GenericChangeDetection();
            DetectChanges.AddINotifyPropertyChanged(SiteName);
            DetectChanges.AddINotifyPropertyChanged(Login);
            DetectChanges.AddINotifyPropertyChanged(Counter);
            DetectChanges.AddINotifyPropertyChanged(Type);
        }
    }
}
