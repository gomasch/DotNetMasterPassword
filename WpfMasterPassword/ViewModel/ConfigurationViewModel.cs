using MasterPassword.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfMasterPassword.Common;

namespace WpfMasterPassword.ViewModel
{
    public class ConfigurationViewModel
    {
        public PropertyModel<string> UserName { get; private set; }
        public ObservableCollection<SiteViewModel> Sites { get; private set; }

        public GenericChangeDetection DetectChanges { get; private set; }

        public ConfigurationViewModel()
        {
            UserName = new PropertyModel<string>();

            Sites = new ObservableCollection<SiteViewModel>();

            DetectChanges = new GenericChangeDetection();
            DetectChanges.AddINotifyPropertyChanged(UserName);
            DetectChanges.AddCollectionOfIDetectChanges(Sites, item => item.DetectChanges);
        }

        public void SaveXml(Stream s)
        {
            var config = new Configuration();

            // fill config
            config.UserName = UserName.Value;
            SynchronizeLists.Sync(config.Sites, Sites, (a, b) => false, site => new SiteEntry(site.SiteName.Value, site.Counter.Value, site.Login.Value, site.Type.Value));

            // save it
            config.Save(s);
        }

        public void ReadXml(Stream s)
        {
            // load config
            var config = new Configuration();

            config.Load(s);

            // apply it's changes to us
            UserName.Value = config.UserName;
            UserName.Value = config.UserName;

            SynchronizeLists.Sync(config.Sites, Sites, (a, b) => false, site => new SiteEntry(site.SiteName.Value, site.Counter.Value, site.Login.Value, site.Type.Value));

        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
