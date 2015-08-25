using System;
using System.Collections.Generic;
using MasterPassword.Core;

namespace MasterPassword.Model
{
    public class SiteEntry
    {
        public string SiteName { get; set; }

        public string Login { get; set; }

        public int Counter { get; set; }

        public PasswordType Type  { get; set; }

        // optional, coming: login, URl, comment

        public SiteEntry(string name, int counter)
        {
            this.SiteName = name;
            this.Login = string.Empty;
            this.Counter = counter;
            this.Type = PasswordType.LongPassword;
        }

        public SiteEntry(string siteName, int counter, string login, PasswordType type)
        {
            this.SiteName = siteName;
            this.Login = login;
            this.Counter = counter;
            this.Type = type;
        }
   }
}

