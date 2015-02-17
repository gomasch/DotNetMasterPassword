using System;
using System.Collections.Generic;

namespace MonoMasterPasswordLib
{
    public class SiteList
    {
        public List<Site> Sites { get; private set; }

        public SiteList()
        {
            Sites = new List<Site>();
        }
    }

    public class Site
    {
        public string SiteName { get; set; }

        public string Login { get; set; }

        public int Counter { get; set; }

        // optional, coming: login, URl, comment

        public Site(string name, int counter)
        {
            if (null == name)
                throw new ArgumentNullException("name");

            this.SiteName = name;
            this.Login = string.Empty;
            this.Counter = counter;
        }

        public Site(string name, int counter, string login)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            if (null == login)
                throw new ArgumentNullException("login");

            this.SiteName = name;
            this.Login = login;
            this.Counter = counter;
        }
   }
}

