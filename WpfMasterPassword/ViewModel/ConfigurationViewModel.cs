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
        public PropertyReadonlyModel<ConfigurationSiteViewModel> SelectedItemScrollTo { get; private set; }

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
            SelectedItemScrollTo = new PropertyReadonlyModel<ConfigurationSiteViewModel>(); // helper to influence grid selection

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

            // let's do a trick with a 2nd property to make the grid scroll to the selected item:
            SelectedItemScrollTo.SetValue(newItem);
            // view wants to do this, let's hope it does (by binding DataGridSelectingItem to this)
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

        public void DoMergeImport(string otherFileName)
        {
            // our...
            var our = GetConfiguration();

            var imported = new Configuration();

            // read second
            try
            {
                using (var file = File.OpenRead(otherFileName))
                {
                    imported.Load(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open file for import: " + ex.Message);
                return;
            }

            // merge
            Merge.Result result;
            try
            {
                result = Merge.Perform(our, imported);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not merge from imported file : " + ex.Message);
                return;
            }

            // preview result...
            var sb = new StringBuilder();
            int changes = 0;

            var noChanges = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.Identical).ToList();
            sb.AppendLine("Identical: " + noChanges.Count + " entries");

            var firstNew = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.FirstNew).ToList();
            if (firstNew.Count > 0)
            {
                // this is just interesting and will not result in changes here
                sb.AppendLine("Will not affect import - not found in imported: " + firstNew.Count + " entries");
                foreach (var item in firstNew)
                {
                    sb.AppendLine("  site: " + item.First.SiteName + " (login='" + item.First.Login + "' c=" + item.First.Counter + " t=" + item.First.Type + ")");
                }
            }
            var firstNewer = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.FirstNewer).ToList();
            if (firstNewer.Count > 0)
            {
                // this is just interesting and will not result in changes here
                sb.AppendLine("Will not affect import - older in imported: " + firstNewer.Count + " entries");
                foreach (var item in firstNewer)
                {
                    sb.AppendLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                }
            }

            var secondNew = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.SecondNew).ToList();
            if (secondNew.Count > 0)
            {
                changes += secondNew.Count;

                sb.AppendLine("Would be added: " + secondNew.Count + " entries");
                foreach (var item in secondNew)
                {
                    sb.AppendLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                }
            }
            var secondNewer = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.SecondNewer).ToList();
            if (secondNewer.Count > 0)
            {
                changes += secondNewer.Count;

                sb.AppendLine("Would be updated: " + secondNewer.Count + " entries");
                foreach (var item in secondNewer)
                {
                    sb.AppendLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                }
            }
            var conflicts = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.Conflict).ToList();
            if (conflicts.Count > 0)
            {
                changes += conflicts.Count;

                sb.AppendLine("Conflicts: " + conflicts.Count + " entries (would be added)");
                foreach (var item in conflicts)
                {
                    sb.AppendLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                }
            }

            if (changes == 0)
            {   // no changes found
                CustomMessageBox.Show("Merge found no candidates for changes.", "Import for Merge");
                return;
            }

            sb.AppendLine();
            sb.AppendLine("Summary Of Planned Changes:");
            sb.AppendLine(" Add=" + secondNew.Count + " (new) Update=" + secondNewer.Count + " Add=" + conflicts.Count + " (conflict)");
            if (CustomMessageBox.ShowOKCancel(sb.ToString(), "Do you want to apply these changes?",
                "Apply Changes", "Cancel") != MessageBoxResult.OK)
            {   // not sure
                return;
            }

            // OK, apply changes
            foreach (var add in secondNew)
            {
                our.Sites.Add(add.Second);
            }
            foreach (var update in secondNewer)
            {
                // update.First is the original item in our.Sites -> update all from second
                update.First.Counter = update.Second.Counter;
                update.First.Login = update.Second.Login;
                update.First.Type = update.Second.Type;
            }
            foreach (var conflict in conflicts)
            {
                for (int i = 0; i < our.Sites.Count; i++)
                {
                    if (our.Sites[i] == conflict.First)
                    {   // add here, after the original
                        our.Sites.Insert(i + 1, conflict.Second);
                        break;
                    }
                }
            }

            // Update UI
            SynchronizeLists.Sync(Sites, our.Sites, siteXml =>
            {
                var site = new ConfigurationSiteViewModel();
                site.Login.Value = siteXml.Login;
                site.SiteName.Value = siteXml.SiteName;
                site.Counter.Value = siteXml.Counter;
                site.TypeOfPassword.Value = siteXml.Type;
                return site;
            });

            SelectedItem.Value = Sites.FirstOrDefault();
        }

        public void SaveXml(Stream s)
        {
            Configuration config = GetConfiguration();

            // save it
            config.Save(s);
        }

        private Configuration GetConfiguration()
        {
            var config = new Configuration();

            // fill config
            config.UserName = UserName.Value;
            SynchronizeLists.Sync(config.Sites, Sites, (a, b) => false, site => new SiteEntry(site.SiteName.Value, site.Counter.Value, site.Login.Value, site.TypeOfPassword.Value));
            return config;
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
