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
using WpfMasterPassword.Dialogs;

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
        public PropertyReadonlyModel<bool> ResetMasterPassword { get; private set; } // we set this to true to allow resetting 

        public PropertyReadonlyModel<string> LastClipboardAction { get; private set; } // show what we did last time "copied login for ... to clipboard"

        // Commands
        public DelegateCommand Add { get; private set; }
        public DelegateCommand RemoveSelected { get; private set; }

        public DelegateCommand GeneratePassword { get; private set; }
        public DelegateCommand CopyToClipBoard { get; private set; }
        public DelegateCommand CopyLoginToClipBoard { get; private set; }

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
            ResetMasterPassword = new PropertyReadonlyModel<bool>();

            LastClipboardAction = new PropertyReadonlyModel<string>();

            CurrentMasterPassword.PropertyChanged += delegate { ResetMasterPassword.SetValue(false); };

            // Commands
            Add = new DelegateCommand(() => PerformAdd());
            RemoveSelected = new DelegateCommand(() => { if (CanRemove(SelectedItem.Value)) Sites.Remove(SelectedItem.Value); }, () => SelectedItem.Value != null);
            GeneratePassword = new DelegateCommand(DoGeneratePassword, () => CurrentMasterPassword.Value != null && SelectedItem.Value != null);
            CopyToClipBoard = new DelegateCommand(DoCopyPassToClipboard, () => !string.IsNullOrEmpty(GeneratedPassword.Value) && CurrentMasterPassword.Value != null);
            CopyLoginToClipBoard = new DelegateCommand(DoCopyLoginToClipboard, () => SelectedItem.Value != null);

            // Change detection
            DetectChanges = new GenericChangeDetection();
            DetectChanges.AddINotifyPropertyChanged(UserName);
            DetectChanges.AddCollectionOfIDetectChanges(Sites, item => item.DetectChanges);
        }

        private void DoCopyLoginToClipboard()
        {
            Clipboard.SetText(SelectedItem.Value.Login.Value);
            LastClipboardAction.SetValue("login for '" + SelectedItem.Value.SiteName.Value + "' copied");
        }

        private void DoCopyPassToClipboard()
        {
            Clipboard.SetText(GeneratedPassword.Value);
            LastClipboardAction.SetValue("password for '" + GeneratedForSite.Value + "' copied");
        }

        private void PerformAdd()
        {
            var newItem = new ConfigurationSiteViewModel();
            Sites.Add(newItem);
            SelectedItem.Value = newItem;
            // view wants to trigger this, let's hope it does
            // grid.UpdateLayout();
            // grid.ScrollIntoView(grid.SelectedItem, null);
        }

        private bool CanRemove(ConfigurationSiteViewModel value)
        {
            // ask user
            var result = CustomMessageBox.ShowYesNoCancel(
                "Do you want to remote site '" + value.SiteName.Value + "' from your list?", ".NET Master Password",
                "Remove", "Don't Remove", "Cancel"
                );
            if (result == MessageBoxResult.Yes)
            {   // yes, remove
                return true; // can remove
            }

            // cancel, default: cannot remove
            return false;
        }

        private void DoGeneratePassword()
        {
            var selectedEntry = SelectedItem.Value;

            string pw = new System.Net.NetworkCredential(string.Empty, CurrentMasterPassword.Value).Password;
            var masterkey = Algorithm.CalcMasterKey(UserName.Value, pw);
            var templateSeed = Algorithm.CalcTemplateSeed(masterkey, selectedEntry.SiteName.Value, selectedEntry.Counter.Value);
            GeneratedPassword.SetValue(Algorithm.CalcPassword(templateSeed, selectedEntry.TypeOfPassword.Value));
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

            ResetMasterPassword.SetValue(true);
        }

        public void Reset()
        {
            UserName.Value = null;
            Sites.Clear();

            GeneratedPassword.SetValue(string.Empty);
            GeneratedForSite.SetValue(string.Empty);

            // Notify Window to reset the entered passwd
            ResetMasterPassword.SetValue(true);

        }
    }
}
