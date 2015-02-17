// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace MonoMacMasterPassword
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField Counter { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField GeneratedPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField MasterKey { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField SiteName { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView SitesTable { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserName { get; set; }

		[Action ("CopyToClipboard:")]
		partial void CopyToClipboard (MonoMac.Foundation.NSObject sender);

		[Action ("RecalcPassword:")]
		partial void RecalcPassword (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (Counter != null) {
				Counter.Dispose ();
				Counter = null;
			}

			if (GeneratedPassword != null) {
				GeneratedPassword.Dispose ();
				GeneratedPassword = null;
			}

			if (MasterKey != null) {
				MasterKey.Dispose ();
				MasterKey = null;
			}

			if (SiteName != null) {
				SiteName.Dispose ();
				SiteName = null;
			}

			if (UserName != null) {
				UserName.Dispose ();
				UserName = null;
			}

			if (SitesTable != null) {
				SitesTable.Dispose ();
				SitesTable = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
