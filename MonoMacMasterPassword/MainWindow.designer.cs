// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace MasterPassword.Mac
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField GeneratedPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField MasterKey { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView SitesTable { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserName { get; set; }

		[Action ("addSite:")]
		partial void addSite (MonoMac.Foundation.NSObject sender);

		[Action ("CopyToClipboard:")]
		partial void CopyToClipboard (MonoMac.Foundation.NSObject sender);

		[Action ("RecalcPassword:")]
		partial void RecalcPassword (MonoMac.Foundation.NSObject sender);

		[Action ("removeSite:")]
		partial void removeSite (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (GeneratedPassword != null) {
				GeneratedPassword.Dispose ();
				GeneratedPassword = null;
			}

			if (MasterKey != null) {
				MasterKey.Dispose ();
				MasterKey = null;
			}

			if (SitesTable != null) {
				SitesTable.Dispose ();
				SitesTable = null;
			}

			if (UserName != null) {
				UserName.Dispose ();
				UserName = null;
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
