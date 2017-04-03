using MasterPassword.Core;

namespace MasterPassword.Model
{
    /// <summary>
    /// represents one entry for a website or account
    /// </summary>
    public class SiteEntry
    {
        /// <summary>
        /// Name of the website or service. This is influencing the generated password. 
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Comment on the used login on the site. This is not influencing the generated password.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Counter, typically 1. This is influencing the generated password. Increase to generate a "new" password for the site.
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// Type of password. This is influencing the generated password. 
        /// </summary>
        public PasswordType Type  { get; set; }

        // optional, coming: login, URl, comment

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">initial site name</param>
        /// <param name="counter">initial counter value</param>
        public SiteEntry(string name, int counter)
        {
            SiteName = name;
            Login = string.Empty;
            Counter = counter;
            Type = PasswordType.LongPassword;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public SiteEntry(SiteEntry copyFrom)
        {
            SiteName = copyFrom.SiteName;
            Login = copyFrom.Login;
            Counter = copyFrom.Counter;
            Type = copyFrom.Type;
        }

        /// <summary>
        /// Constructor setting all properties
        /// </summary>
        /// <param name="siteName">initial site name</param>
        /// <param name="counter">initial counter value</param>
        /// <param name="login">initial login value</param>
        /// <param name="type">initial type</param>
        public SiteEntry(string siteName, int counter, string login, PasswordType type)
        {
            SiteName = siteName;
            Login = login;
            Counter = counter;
            Type = type;
        }
   }
}

