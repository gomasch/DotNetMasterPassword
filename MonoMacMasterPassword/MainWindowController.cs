
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMasterPassword;

namespace MonoMacMasterPassword
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
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
			LoadSettings ();
		}

		// Clipboard
		private static string[] pboardTypes = new string[] { "NSStringPboardType" };

		partial void CopyToClipboard (MonoMac.Foundation.NSObject sender)
		{
			NSPasteboard.GeneralPasteboard.DeclareTypes(pboardTypes, null);
			NSPasteboard.GeneralPasteboard.SetStringForType(GeneratedPassword.StringValue, pboardTypes[0]);
		}

		// UI Actions
		partial void RecalcPassword (MonoMac.Foundation.NSObject sender)
		{
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

			var masterkey = MasterPassword.CalcMasterKey(userName, masterPass);
			var siteKey = MasterPassword.CalcTemplateSeed(masterkey, siteName, counter);
			string result = MasterPassword.CalcPassword(siteKey, PasswordType.LongPassword);

			GeneratedPassword.StringValue = result;
		}

		// Helper
		void LoadSettings ()
		{
			if (null == UserName) {
				return;
			}
			UserName.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("UserName");
			SiteName.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("SiteName");
			Counter.StringValue = NSUserDefaults.StandardUserDefaults.StringForKey ("SiteCounter");
		}

		void SaveSettings ()
		{
			NSUserDefaults.StandardUserDefaults.SetString(UserName.StringValue, "UserName");
			NSUserDefaults.StandardUserDefaults.SetString(SiteName.StringValue, "SiteName");
			NSUserDefaults.StandardUserDefaults.SetString(Counter.StringValue, "SiteCounter");
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}
	}
}

