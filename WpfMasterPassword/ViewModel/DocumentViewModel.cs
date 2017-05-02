using MasterPassword.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfMasterPassword.Common;
using System.ComponentModel;
using WpfMasterPassword.Properties;
using System.Diagnostics;
using WpfMasterPassword.Dialogs;

namespace WpfMasterPassword.ViewModel
{
    public class DocumentViewModel : BindableBase
    {
        public PropertyDelegateReadonlyModel<string> WindowTitle { get; private set; }

        public PropertyReadonlyModel<bool> HasChanges { get; private set; }
        public PropertyReadonlyModel<bool> FilePathValid { get; private set; }
        public PropertyReadonlyModel<string> FilePathName { get; private set; }

        public DelegateCommand Open { get; private set; }
        public DelegateCommand ImportForMerge { get; private set; }
        public DelegateCommand Save { get; private set; }
        public DelegateCommand SaveAs { get; private set; }
        public DelegateCommand New { get; private set; }

        public ConfigurationViewModel Config { get; private set; }

        const string FileFilter = "MasterPassword file (*.xml)|*.xml|All files (*.*)|*.*";
        const string FileNameNew = "<new>";

        public DocumentViewModel()
        {
            WindowTitle = new PropertyDelegateReadonlyModel<string>(() => ".NET Master Password - " + FilePathName.Value + " " + (HasChanges.Value ? "*" : ""));
            HasChanges = new PropertyReadonlyModel<bool>();
            FilePathValid = new PropertyReadonlyModel<bool>();
            FilePathName = new PropertyReadonlyModel<string>(FileNameNew);
            WindowTitle.MonitorForChanges(HasChanges, FilePathName);

            Open = new DelegateCommand(() => DoOpen());
            ImportForMerge = new DelegateCommand(DoImportMerge);
            Save = new DelegateCommand(() => DoSave());
            SaveAs = new DelegateCommand(() => DoSaveAs());
            New = new DelegateCommand(DoNew);

            // data
            Config = new ConfigurationViewModel();
            Config.DetectChanges.DataChanged += () => HasChanges.SetValue(true);

            // try to load last file
            try
            {
                if (Settings.Default.HasMostRecentFile)
                {   // we have an old path
                    PerformOpen(Settings.Default.MostRecentFile);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }

            HasChanges.SetValue(false);
        }

        /// <summary>
        /// call from MainWindow when dropping a file
        /// </summary>
        public void OpenFileFromDrop(string fileName)
        {
            try
            {
                PerformOpen(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not read/interpret dropped file " + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// call from MainWindow in Closing event
        /// </summary>
        public void OnClose(CancelEventArgs e)
        {
            if (!CanDiscardOldData("Closing App"))
            {   // this would lose data, do not close
                e.Cancel = true;
            }

            // store file name in settings
            Settings.Default.HasMostRecentFile = FilePathValid.Value;
            Settings.Default.MostRecentFile = FilePathValid.Value ? FilePathName.Value : string.Empty; // store empty string if no valid file name
        }

        private bool CanDiscardOldData(string captionForQuestion)
        {
            if (!HasChanges.Value)
            {
                return true; // no unsaved changes
            }

            // ask user
            var result = CustomMessageBox.ShowYesNoCancel(
                "Do you want to save your current changes?", captionForQuestion, 
                "Save", "Don't Save", "Cancel"
                );
            if (result == MessageBoxResult.Yes)
            {   // yes, save
                return DoSave(); // return true (can discard) if file was saved successfully
            }
            if (result != MessageBoxResult.Yes)
            {   // not dont save
                return true; // can discard
            }

            // cancel, default: cannot discard
            return false; // cannot discard
        }

        private bool DoOpen()
        {
            if (!CanDiscardOldData("Open new file")) return false;

            // ask for file name
            var dlg = new OpenFileDialog();

            dlg.Filter = FileFilter;

            if (dlg.ShowDialog() != true)
            {   // user did not want to save
                return false;
            }

            string newFileName = dlg.FileName;

            try
            {
                PerformOpen(newFileName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open file: " + ex.Message);
                return false;
            }
        }

        private void DoImportMerge()
        {
            // ask for file name
            var dlg = new OpenFileDialog();

            dlg.Filter = FileFilter;

            if (dlg.ShowDialog() != true)
            {   // user did not want to save
                return;
            }

            string newFileName = dlg.FileName;

            Config.DoMergeImport(newFileName);
        }


        private bool DoSave()
        {
            if (FilePathValid.Value)
            {
                try
                {
                    PerformSave(FilePathName.Value);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not save file " + ex.Message);
                    return false;
                }
            }
            else
            {   // no valid 
                return DoSaveAs();
            }
        }

        private bool DoSaveAs()
        {
            // ask for file name
            var dlg = new SaveFileDialog();

            dlg.Filter = FileFilter;

            if (FilePathValid.Value)
            {   // we had a previous valid file name
                dlg.FileName = FilePathName.Value;
            }

            if (dlg.ShowDialog() != true)
            {   // user did not want to save
                return false;
            }

            string newFileName = dlg.FileName;

            try
            {
                PerformSave(newFileName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save file " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// can throw exception in case of error
        /// </summary>
        private void PerformSave(string fileName)
        {
            using (var file = File.Create(fileName))
            {
                Config.SaveXml(file);
            }

            // succeeded 
            FilePathName.SetValue(fileName);
            FilePathValid.SetValue(true);
            HasChanges.SetValue(false);
        }

        /// <summary>
        /// can throw exception in case of error
        /// </summary>
        private void PerformOpen(string fileName)
        {
            try
            {
                using (var file = File.OpenRead(fileName))
                {
                    Config.ReadXml(file);
                }
            }
            catch (Exception )
            {
                Config.Reset();
                throw;
            }

            // succeeded 
            FilePathName.SetValue(fileName);
            FilePathValid.SetValue(true);
            HasChanges.SetValue(false);
        }

        private void DoNew()
        {
            if (!CanDiscardOldData("Create New Configuration")) return;

            Config.Reset();

            FilePathName.SetValue(FileNameNew);
            FilePathValid.SetValue(false);
            HasChanges.SetValue(false);
        }
    }
}
