using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfMasterPassword.ViewModel;

namespace WpfMasterPassword.UserControls
{
    /// <summary>
    /// Interaction logic for ConfigurationUserControl.xaml
    /// </summary>
    public partial class ConfigurationUserControl : UserControl
    {
        public ConfigurationUserControl()
        {
            InitializeComponent();
        }
    }

    public class ConfigurationUserControl_DesignTimeData : ConfigurationViewModel
    {
        public ConfigurationUserControl_DesignTimeData()
        {
            UserName.Value = "John Doe";

            var site = new ConfigurationSiteViewModel();
            site.SiteName.Value = "ebay.com";
            site.Login.Value = "jdoe@gmail.com";
            site.Type.Value = MasterPassword.Core.PasswordType.LongPassword;
            site.Counter.Value = 3;
            Sites.Add(site);

            site = new ConfigurationSiteViewModel();
            site.SiteName.Value = "ripeyesteaks.com";
            site.Login.Value = "john@gorgelmail.com";
            site.Type.Value = MasterPassword.Core.PasswordType.PIN;
            Sites.Add(site);

            site = new ConfigurationSiteViewModel();
            site.SiteName.Value = "othersite.com";
            site.Login.Value = "doe@john.org";
            site.Type.Value = MasterPassword.Core.PasswordType.MaximumSecurityPassword;
            Sites.Add(site);

        }
    }

}
