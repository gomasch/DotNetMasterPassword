using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MasterPassword.Model;

namespace MasterPassword.Mac
{
    /// <summary>
    /// Table view for sites.
    /// 
    /// Example from http://www.netneurotic.net/Mono/MonoMac-NSTableView.html
    /// </summary>
    [Register ("TableViewDataSource")]
	public class TableViewForSites : NSTableViewDataSource
	{
        public List<SiteEntry> Sites { get; private set; }

        public TableViewForSites (List<SiteEntry> sites)
		{
            Sites = sites;
		}

		// This method will be called by the NSTableView control to learn the number of rows to display.
		[Export ("numberOfRowsInTableView:")]
		public int NumberOfRowsInTableView (NSTableView table)
		{
            return Sites.Count;
		}

		// This method will be called by the control for each column and each row.
		[Export ("tableView:objectValueForTableColumn:row:")]
		public NSObject ObjectValueForTableColumn (NSTableView table, NSTableColumn col, int row)
		{
            if (row < 0)
            {   // invalid index
                return null;
            }
            if (row > Sites.Count - 1)
            {   // invalid index
                return null;
            }
                
            var item = Sites[row];

            if (col.Identifier == "Counter")
            {
                return new NSString (item.Counter.ToString());
            }
            if (col.Identifier == "Site")
            {
                return new NSString (item.SiteName.ToString());
            }
            if (col.Identifier == "Login")
            {
                return new NSString (item.Login.ToString());
            }

            return null;
		}

        [Export ("tableViewSelectionDidChange:")]       
        public void SelectionDidChange (MonoMac.Foundation.NSNotification notification)
        {

        }
	}
}

