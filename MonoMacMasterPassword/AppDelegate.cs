using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MasterPassword.Mac;

namespace MasterPassword.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		public AppDelegate ()
		{
		}

        public override void DidFinishLaunching (NSNotification notification)
		{
			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}

        [Action ("newDocument:")]
        public void NewDocument (MonoMac.Foundation.NSObject sender)
        {
            mainWindowController.Window.MakeKeyAndOrderFront(this);
            // call mainWindowController and tell him to do something new

            mainWindowController.NewDocument();
        }

        [Action ("openDocument:")]
        public void OpenDocument (MonoMac.Foundation.NSObject sender)
        {
            mainWindowController.OpenDocument();
        }

        [Action ("saveDocument:")]
        public void SaveDocument (MonoMac.Foundation.NSObject sender)
        {
            mainWindowController.SaveDocument();
        }

        [Action ("saveDocumentAs:")]
        public void SaveDocumentAs (MonoMac.Foundation.NSObject sender)
        {
            mainWindowController.SaveDocumentAs();
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }
	}
}

