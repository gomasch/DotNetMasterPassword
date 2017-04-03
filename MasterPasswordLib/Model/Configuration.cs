using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using MasterPassword.Core;
using System.Linq;

namespace MasterPassword.Model
{
    /// <summary>
    /// Represents the configuration for one user
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Name of the user, is used to generate the passwords for the sites
        /// </summary>
        public string UserName { get; set;} 

        /// <summary>
        /// List of sites to generate passwords for
        /// </summary>
        public List<SiteEntry> Sites { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Configuration()
        {
            UserName = string.Empty;
            Sites = new List<SiteEntry>();
        }

        /// <summary>
        /// Load from a stream (typically: an XML file)
        /// </summary>
        /// <param name="s">open stream</param>
        public void Load(Stream s)
        {
            XDocument doc = XDocument.Load(s);

            UserName = doc.Root.Element("UserName").Value;

            Sites.Clear();
            foreach (var node in doc.Root.Element("Sites").Elements("Site"))
            {
                PasswordType pwType = PasswordType.LongPassword; // standard

                var typeElement = node.Element("Type");

                if (null != typeElement)
                {   
                    pwType = SerializePasswordTypeType.First(row => row.Value == typeElement.Value).Key;
                }

                SiteEntry entry = new SiteEntry(
                                      siteName: node.Element("SiteName").Value,
                                      counter: int.Parse(node.Element("Counter").Value),
                                      login: node.Element("Login").Value,
                                      type: pwType
                                  );
                Sites.Add(entry);
            }
        }

        /// <summary>
        /// Save to a stream (typically: an XML file)
        /// </summary>
        /// <param name="s">open stream</param>
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
                    new XElement("Login", entry.Login),
                    new XElement("Type", SerializePasswordTypeType[entry.Type])
                ));
            }

            doc.Save(s);
        }

        /// <summary>
        /// Clear sites and reset user name to default value.
        /// </summary>
        public void Clear()
        {
            UserName = "User";
            Sites.Clear();
        }

        /// <summary>
        /// Helper structure for XML serialization: strings to use for the password types.
        /// </summary>
        public static Dictionary<PasswordType, string> SerializePasswordTypeType = new Dictionary<PasswordType, string> 
        {
            { PasswordType.MaximumSecurityPassword, "MaximumSecurityPassword" },
            { PasswordType.LongPassword, "LongPassword" },
            { PasswordType.MediumPassword, "MediumPassword" },
            { PasswordType.ShortPassword, "ShortPassword" },
            { PasswordType.BasicPassword, "BasicPassword" },
            { PasswordType.PIN, "PIN" }
        };
    }


}

