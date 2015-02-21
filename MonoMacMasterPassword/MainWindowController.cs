
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
			var dict = new NSMutableDictionary ();
			dict ["UserName"] = (NSString)"User";
			dict ["SiteName"] = (NSString)"site";
			dict ["SiteCounter"] = (NSString)"1";
			NSUserDefaults.StandardUserDefaults.RegisterDefaults (dict);


            // using a delegate
            this.Window.WillClose += delegate(object sender, EventArgs e)
                {
                    NSApplication.SharedApplication.Terminate(this);
                };
		}

        // terminate application on windows close:
        // usage: this.Window.Delegate = new MyWindowDelegate() in Initialize of Window Controller
        //private class MyWindowDelegate : NSWindowDelegate
        //{
        //    public override void WillClose(NSNotification notification)
        //    {
        //        // close app
        //        //NSApplication.SharedApplication.Terminate(notification);
        //    }
        //}
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

            SitesTable.DataSource = new TableViewForSites(Config.Sites);
		}            

        // UI Actions
		partial void RecalcPassword (MonoMac.Foundation.NSObject sender)
		{
            // get data from UI
			string userName = UserName.StringValue;
			string masterPass = MasterKey.StringValue;
			string siteName = SiteName.StringValue;

			int counter = 1;
			if (!int.TryParse(Counter.StringValue, out counter))
			{	// failed
				counter = 1;
				Counter.StringValue = counter.ToString();
			}

			SaveSettings();

            // calculate result
			var masterkey = Algorithm.CalcMasterKey(userName, masterPass);
            var siteKey = Algorithm.CalcTemplateSeed(masterkey, siteName, counter);
            string result = Algorithm.CalcPassword(siteKey, PasswordType.LongPassword);

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

			UserName.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("UserName");
			SiteName.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("SiteName");
			Counter.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("SiteCounter");

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
            }
		}

		void SaveSettings ()
		{
			NSUserDefaults.StandardUserDefaults.SetString(UserName.StringValue, "UserName");
			NSUserDefaults.StandardUserDefaults.SetString(SiteName.StringValue, "SiteName");
			NSUserDefaults.StandardUserDefaults.SetString(Counter.StringValue, "SiteCounter");

            NSUserDefaults.StandardUserDefaults.SetBool(FileNameOK, "LastFileNameOK");
            if (FileNameOK)
            {
                NSUserDefaults.StandardUserDefaults.SetString(FileName, "LastFileName");

                try
                {
                    using (var s = File.OpenWrite(FileName))
                    {
                        Config.Save(s);
                    }
                }
                catch
                {
                    // what to do? log....
                }
            }

            NSUserDefaults.StandardUserDefaults.Synchronize ();
		}
	}
}

