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

namespace WpfMasterPassword.ViewModel
{
    public class DocumendViewModel : BindableBase
    {
        public PropertyReadonlyModel<bool> HasChanges { get; private set; }
        public PropertyReadonlyModel<bool> FilePathValid { get; private set; }
        public PropertyReadonlyModel<string> FilePathName { get; private set; }

        public DelegateCommand Open { get; private set; }
        public DelegateCommand Save { get; private set; }
        public DelegateCommand SaveAs { get; private set; }
        public DelegateCommand New { get; private set; }

        public ConfigurationViewModel Config { get; private set; }

        const string FileFilter = "MasterPassword file (*.xml)|*.xml|All files (*.*)|*.*";
        const string FileNameNew = "<new>";

        public DocumendViewModel()
        {
            HasChanges = new PropertyReadonlyModel<bool>();
            FilePathValid = new PropertyReadonlyModel<bool>();
            FilePathName = new PropertyReadonlyModel<string>(FileNameNew);

            Open = new DelegateCommand(() => DoOpen());
            Save = new DelegateCommand(() => DoSave());
            SaveAs = new DelegateCommand(() => DoSaveAs());
            New = new DelegateCommand(DoNew);

            // data
            Config = new ConfigurationViewModel();
        }

        private bool CanDiscardOldData(string captionForQuestion)
        {
            if (!HasChanges.Value)
            {
                return true; // no unsaved changes
            }

            // ask user
            var result = MessageBox.Show("Do you want to discard your current changes?", captionForQuestion, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {   // not ok, do not discard
                return false;
            }

            return DoSave(); // return true (can discard) if file was saved successfully
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
                MessageBox.Show("Could not save file " + ex.Message);
                return false;
            }
        }

        private bool DoSave()
        {
            if (FilePathValid.Value)
            {
                try
                {
                    PerformOpen(FilePathName.Value);
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
