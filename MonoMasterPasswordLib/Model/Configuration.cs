using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace MasterPassword.Model
{
    public class Configuration
    {
        public string UserName { get; set;} 

        public List<SiteEntry> Sites { get; private set; }

        public Configuration()
        {
            UserName = string.Empty;
            Sites = new List<SiteEntry>();
        }

        public void Load(Stream s)
        {
            XDocument doc = XDocument.Load(s);

            UserName = doc.Root.Element("UserName").Value;

            Sites.Clear();
            foreach (var node in doc.Root.Element("Sites").Elements("Site"))
            {
                SiteEntry entry = new SiteEntry(
                                      siteName: node.Element("SiteName").Value,
                                      counter: int.Parse(node.Element("Counter").Value),
                                      login: node.Element("Login").Value
                                  );
                Sites.Add(entry);
            }
        }

        public void Save(Stream s)
        {
            XDocument doc = new XDocument(new XElement("MasterPassword"));
                         
            doc.Root.Add(new XElement("UserName", UserName));

            var sitesNode = new XElement("Sites");
            doc.Root.Add(sitesNode);

            foreach (var entry in Sites)
            {
                sitesNode.Add(new XElement("Site",
                    new XElement("SiteName", entry.SiteName),
                    new XElement("Counter", entry.Counter),
                    new XElement("Login", entry.Login)
                ));
            }
        }

        public void Clear()
        {
            UserName = string.Empty;
            Sites.Clear();
        }

    }


}

