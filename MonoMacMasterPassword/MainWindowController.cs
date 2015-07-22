
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.IO;
using MasterPassword.Model;
using MasterPassword.Core;

namespace MasterPassword.Mac
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
        public Configuration Config = new Configuration();
        public TableViewForSites TableController;

        public string FileName;
        public bool FileNameOK;

        #region Constructors

		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}
            

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();


			LoadSettings();

            TableController = new TableViewForSites(Config.Sites, SitesTable);
		}            

        // UI Actions
        partial void addSite (MonoMac.Foundation.NSObject sender)
        {
            TableController.Add();
        }

        partial void removeSite (MonoMac.Foundation.NSObject sender)
        {
            TableController.Remove();
        }

		partial void RecalcPassword (MonoMac.Foundation.NSObject sender)
		{
            // get data from UI
			string userName = UserName.StringValue;
			string masterPass = MasterKey.StringValue;

            if (SitesTable.SelectedRow < 0) return;

            var currentSite = Config.Sites[SitesTable.SelectedRow];
            string siteName = currentSite.SiteName;
            int counter = currentSite.Counter;
            PasswordType pwType = currentSite.Type;

			SaveSettings();

            // calculate result
			var masterkey = Algorithm.CalcMasterKey(userName, masterPass);
            var siteKey = Algorithm.CalcTemplateSeed(masterkey, siteName, counter);
            string result = Algorithm.CalcPassword(siteKey, pwType);

            // display result
			GeneratedPassword.StringValue = result;
		}

        private static string[] pboardTypes = new string[] { "NSStringPboardType" };

        partial void CopyToClipboard (MonoMac.Foundation.NSObject sender)
        {
            NSPasteboard.GeneralPasteboard.DeclareTypes(pboardTypes, null);
            NSPasteboard.GeneralPasteboard.SetStringForType(GeneratedPassword.StringValue, pboardTypes[0]);
        }


		// Helper
		void LoadSettings ()
		{
			if (null == UserName)            
            {
				return;
			}
                
            // Load file
            FileNameOK = NSUserDefaults.StandardUserDefaults.BoolForKey("LastFileNameOK");
            if (FileNameOK)
            {
                FileName = NSUserDefaults.StandardUserDefaults.StringForKey("LastFileName");

                try
                {
                    using (var s = File.OpenRead(FileName))
                    {
                        Config.Load(s);
                    }

                }
                catch (Exception ex)
                {
                    var alert = new NSAlert
                    {
                        MessageText = "Failed to open file " + ex.Message
                    };
                    alert.AddButton("OK");
                    alert.RunModal();

                    Config.Clear();
                    FileNameOK = false;
                }

                SitesTable.ReloadData();
                UserName.StringValue = Config.UserName;
            }
		}

		void SaveSettings ()
		{
            NSUserDefaults.StandardUserDefaults.SetBool(FileNameOK, "LastFileNameOK");
            if (FileNameOK)
            {
                Config.UserName = UserName.StringValue;

                NSUserDefaults.StandardUserDefaults.SetString(FileName, "LastFileName");

                try
                {
                    using (var s = File.Create(FileName))
                    {
                        Config.Save(s);
                    }
                }
                catch (Exception ex)
                {
                    var alert = new NSAlert
                        {
                            MessageText = "Failed to save file " + ex.Message
                        };
                    alert.AddButton("OK");
                    alert.RunModal();
                }
            }

            NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

        public void NewDocument()
        {
            FileNameOK = false;
            Config.Clear();

            // Reset display
            SitesTable.ReloadData();
            UserName.StringValue = Config.UserName;
            MasterKey.StringValue = "";
        }

        public void OpenDocument()
        {
            NSOpenPanel openDlg= new NSOpenPanel();

            openDlg.CanChooseFiles= true;

            int result=openDlg.RunModal();

            if (result== 1)
            {
                FileNameOK = true;
                FileName = openDlg.Url.Path;
                LoadSettings();
                MasterKey.StringValue = "";
            }
        }

        public void SaveDocument()
        {
            if (FileNameOK)
            {
                SaveSettings();
            }
            else
            {
                SaveDocumentAs();
            }
        }

        public void SaveDocumentAs()
        {
            NSSavePanel saveDlg = new NSSavePanel();
            saveDlg.CanCreateDirectories = true;
            saveDlg.AllowedFileTypes = new string[] { "xml", "PDF" };
            saveDlg.AllowsOtherFileTypes = true;

            if (FileNameOK)
            {
                //openDlg.Sta = FileName;
            }

            int result = saveDlg.RunModal();

            if (result == 1)
            {
                FileNameOK = true;
                FileName = saveDlg.Url.Path;
                SaveSettings();
            }
        }

	}
}

