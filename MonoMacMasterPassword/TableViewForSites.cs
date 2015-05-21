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
        NSTableView tableView;
        public List<SiteEntry> Sites { get; private set; }

        public TableViewForSites (List<SiteEntry> sites, NSTableView tableView)
		{
            this.Sites = sites;
            this.tableView = tableView;

            // register us as the view
            tableView.DataSource = this;
		}

        public void Add()
        {
            Sites.Add(new SiteEntry("new site", 1));
            tableView.ReloadData();
        }

        public void Remove()
        {
            if (this.tableView.SelectedRow > 0)
            {
                Sites.RemoveAt(tableView.SelectedRow);
                tableView.ReloadData();
            }
            this.tableView.AbortEditing ();
        }

		public override int GetRowCount(NSTableView tableView)
        {
            return Sites.Count;
        }

        public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn col, int row)      
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
            if (col.Identifier == "PwType")
            {
                return new NSString (item.Type.ToString());
            }

            return null;
		}

        public override void SetObjectValue (NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int row)
        {
            // http://blog.ac-graphic.net/cocoa-programming-l15-nstableview-editing-values/
            if (tableColumn.Identifier == "Counter")
            {
                int newValue;
                if (int.TryParse((NSString)theObject, out newValue))
                {
                    Sites[row].Counter = newValue;
                }
            }
            else if (tableColumn.Identifier == "Site")
            {
                Sites[row].SiteName = (NSString)theObject;
            }
            else if (tableColumn.Identifier == "Login")
            {
                Sites[row].Login = (NSString)theObject;
            }
            else if (tableColumn.Identifier == "PwType")
            {
                string nameOfItem = (NSString)theObject;

                if (nameOfItem.ToLower().Contains("medium"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.MediumPassword;
                }
                else if (nameOfItem.ToLower().Contains("long"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.LongPassword;
                }
                else if (nameOfItem.ToLower().Contains("pin"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.PIN;
                }
                else if (nameOfItem.ToLower().Contains("short"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.ShortPassword;
                }
                else if (nameOfItem.ToLower().Contains("max"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.MaximumSecurityPassword;
                }
                else if (nameOfItem.ToLower().Contains("basic"))
                {
                    Sites[row].Type = MasterPassword.Core.PasswordType.BasicPassword;
                }

                tableView.ReloadData();
            }
            else
                throw new NotImplementedException (string.Format ("{0} is not recognized", 
                    tableColumn.Identifier));
        }
	}
}

