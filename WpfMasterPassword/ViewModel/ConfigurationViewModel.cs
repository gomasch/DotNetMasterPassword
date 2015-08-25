using MasterPassword.Core;
using MasterPassword.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMasterPassword.Common;

namespace WpfMasterPassword.ViewModel
{
    public class ConfigurationViewModel
    {
        // Data
        public PropertyModel<string> UserName { get; private set; }
        public ObservableCollection<ConfigurationSiteViewModel> Sites { get; private set; }

        public PropertyModel<ConfigurationSiteViewModel> SelectedItem { get; private set; }

        public PropertyReadonlyModel<string> GeneratedForSite { get; private set; }
        public PropertyReadonlyModel<string> GeneratedPassword { get; private set; }

        public PropertyModel<SecureString> CurrentMasterPassword { get; private set; }

        // Commands
        public DelegateCommand Add { get; private set; }
        public DelegateCommand<ConfigurationSiteViewModel> Remove { get; private set; }

        public DelegateCommand GeneratePassword { get; private set; }
        public DelegateCommand CopyToClipBoard { get; private set; }

        // Change Detection
        public GenericChangeDetection DetectChanges { get; private set; }

        public ConfigurationViewModel()
        {
            // Data
            UserName = new PropertyModel<string>();
            Sites = new ObservableCollection<ConfigurationSiteViewModel>();

            SelectedItem = new PropertyModel<ConfigurationSiteViewModel>();

            GeneratedForSite = new PropertyReadonlyModel<string>();
            GeneratedPassword = new PropertyReadonlyModel<string>();
            CurrentMasterPassword = new PropertyModel<SecureString>();

            // Commands
            Add = new DelegateCommand(() => Sites.Add(new ConfigurationSiteViewModel()));
            Remove = new DelegateCommand<ConfigurationSiteViewModel>(item => Sites.Remove(item));
            GeneratePassword = new DelegateCommand(DoGeneratePassword, () => CurrentMasterPassword.Value != null && SelectedItem.Value != null);
            CopyToClipBoard = new DelegateCommand(() => Clipboard.SetText(GeneratedPassword.Value), () => !string.IsNullOrEmpty(GeneratedPassword.Value) && CurrentMasterPassword.Value != null);

            // Change detection
            DetectChanges = new GenericChangeDetection();
            DetectChanges.AddINotifyPropertyChanged(UserName);
            DetectChanges.AddCollectionOfIDetectChanges(Sites, item => item.DetectChanges);
        }

        private void DoGeneratePassword()
        {
            var selectedEntry = SelectedItem.Value;

            string pw = new System.Net.NetworkCredential(string.Empty, CurrentMasterPassword.Value).Password;
            var masterkey = Algorithm.CalcMasterKey(UserName.Value, pw);
            var templateSeed = Algorithm.CalcTemplateSeed(masterkey, selectedEntry.SiteName.Value, selectedEntry.Counter.Value);            GeneratedPassword.SetValue(Algorithm.CalcPassword(templateSeed, selectedEntry.TypeOfPassword.Value));
            GeneratedForSite.SetValue(selectedEntry.SiteName.Value);
        }

        public void SaveXml(Stream s)
        {
            var config = new Configuration();

            // fill config
            config.UserName = UserName.Value;
            SynchronizeLists.Sync(config.Sites, Sites, (a, b) => false, site => new SiteEntry(site.SiteName.Value, site.Counter.Value, site.Login.Value, site.TypeOfPassword.Value));

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

            SynchronizeLists.Sync(Sites, config.Sites, siteXml =>
            {
                var site = new ConfigurationSiteViewModel();
                site.Login.Value = siteXml.Login;
                site.SiteName.Value = siteXml.SiteName;
                site.Counter.Value = siteXml.Counter;
                site.TypeOfPassword.Value = siteXml.Type;
                return site;
            }
            );

            SelectedItem.Value = Sites.FirstOrDefault();
        }

        public void Reset()
        {
            
        }
    }
}
