using MasterPassword.Core;
using MasterPassword.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleMasterPassword
{
    /// <summary>
    /// Console application for Master Password.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {   // no args - print help
                    PrintHelp();
                    return 0; // do not continue
                }
                else
                {
                    return ParseCommands(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Program stops, error detected:  " + ex.Message);
                return 1;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(".NET Master Password Console");
            Console.WriteLine("Generate derived passwords from a master password for a website.");
            Console.WriteLine("Usage:");
            Console.WriteLine("  ConsoleMasterPassword -i");
            Console.WriteLine("  ConsoleMasterPassword -u user -s sitename -t type -c counter -p password");
            Console.WriteLine("  ConsoleMasterPassword -m cfg.xml cfg2.xml -o cfg3.xml");
            Console.WriteLine("Example:");
            Console.WriteLine("  ConsoleMasterPassword -u \"John Doe\" -s \"ebay.com\" - t long -c 1 -p \"a pwd\"");
            Console.WriteLine("Commands:");
            Console.WriteLine("  -?  help");
            Console.WriteLine("  -i  use interactive mode to enter all data for one password");
            Console.WriteLine("  -u  all properties to generate a password as options");
            Console.WriteLine("    -u  specify user name");
            Console.WriteLine("    -s  specify site name");
            Console.WriteLine("    -t  specify type of password as an index value");
            Console.WriteLine("        Available types of passwords:");
            var types = GetPasswordTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Console.WriteLine("        " + i + "=" + types[i].ToString());
            }
            Console.WriteLine("    -c  specify counter for this site, optional, default is 1");
            Console.WriteLine("    -p  specify masterpassword to derive the new password from");
            Console.WriteLine("  -m  read two configuration files, merge them and print result");
            Console.WriteLine("    -o  save merged result into new file, both entries for conflicts");
        }

        private static int ParseCommands(string[] args)
        {
            // try to parse commands
            var cmd = new ParseArgs(args);

            // set/update configuration based on commands
            while (cmd.IsValid)
            {
                if (cmd.CurrentArg == "-i")
                {   // interactive mode
                    cmd.Next();

                    if (cmd.IsValid)
                    {
                        Console.WriteLine("Unexpected argument for interactive mode.");
                        return 1; // error
                    }

                    InteractiveMode();
                    return 0; // do not continue
                }
                else if (cmd.CurrentArg == "-?")
                {   // help
                    PrintHelp();
                    return 0; // do not continue
                }
                else if (cmd.CurrentArg == "-u")
                {   // user name for non-interactive mode
                    cmd.Next();

                    return NonInteractiveMode(cmd);
                }
                else if (cmd.CurrentArg == "-m")
                {   // merge mode
                    cmd.Next();

                    if (!cmd.IsValid)
                    {
                        Console.WriteLine("missing first filename.");
                        return 1; // error
                    }
                    var firstFile = cmd.CurrentArg;
                    cmd.Next();

                    if (!cmd.IsValid)
                    {
                        Console.WriteLine("missing second filename.");
                        return 1; // error
                    }
                    var secondFile = cmd.CurrentArg;
                    cmd.Next();

                    string saveHere = null; // null means don't save merged result
                    if (cmd.IsValid)
                    {
                        if (cmd.CurrentArg != "-o")
                        {
                            Console.WriteLine("unknown option " + cmd.CurrentArg);
                            return 1; // error
                        }
                        cmd.Next();

                        if (!cmd.IsValid)
                        {
                            Console.WriteLine("missing second filename.");
                            return 1; // error
                        }
                        saveHere = cmd.CurrentArg;
                        cmd.Next();

                        if (cmd.IsValid)
                        {
                            Console.WriteLine("unknown option " + cmd.CurrentArg);
                            return 1; // error
                        }

                        if (string.Equals(saveHere, firstFile, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(saveHere, secondFile, StringComparison.InvariantCultureIgnoreCase))
                        {   // overwriting input files is not supported
                            Console.WriteLine("Not merging: overwriting the input files is not supported." + cmd.CurrentArg);
                            return 1; // error
                        }
                    }


                    // read both files
                    var firstConfig = new Configuration();
                    using (var file = File.OpenRead(firstFile))
                    {
                        firstConfig.Load(file);
                    }
                    var secondConfig = new Configuration();
                    using (var file = File.OpenRead(secondFile))
                    {
                        secondConfig.Load(file);
                    }

                    Console.WriteLine("Found entries: " + firstConfig.Sites.Count + " sites in 1st, " + secondConfig.Sites.Count + " sites in 2nd");

                    // merge
                    Merge.Result result;
                    try
                    {
                        result = Merge.Perform(firstConfig, secondConfig);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Merge failed.");
                        Console.WriteLine(ex.Message);                        
                        return 1;
                    }

                    // print result
                    var identical = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.Identical).ToList();
                    if (identical.Count > 0)
                    {
                        Console.WriteLine("Same in both both configs: " + identical.Count + " entries");
                        foreach (var item in identical)
                        {
                            Console.WriteLine("  site: " + item.First.SiteName + " (login='" + item.First.Login + "' c=" + item.First.Counter + " t=" + item.First.Type + ")");
                        }
                    }

                    // Display overview
                    var firstNew = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.FirstNew).ToList();
                    if (firstNew.Count > 0)
                    {
                        Console.WriteLine("New in 1st (not found in 2nd): " + firstNew.Count + " entries");
                        foreach (var item in firstNew)
                        {
                            Console.WriteLine("  site: " + item.First.SiteName + " (login='" + item.First.Login + "' c=" + item.First.Counter + " t=" + item.First.Type + ")");
                        }
                    }
                    var firstNewer = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.FirstNewer).ToList();
                    if (firstNewer.Count > 0)
                    {
                        Console.WriteLine("Newer in 1st (also found in 2nd but older): " + firstNewer.Count + " entries");
                        foreach (var item in firstNewer)
                        {
                            Console.WriteLine("  site: " + item.First.SiteName + " (login='" + item.First.Login + "' c=" + item.First.Counter + " t=" + item.First.Type + ")");
                        }
                    }

                    var secondNew = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.SecondNew).ToList();
                    if (secondNew.Count > 0)
                    {
                        Console.WriteLine("New in 2nd (not found in 1st): " + secondNew.Count + " entries");
                        foreach (var item in secondNew)
                        {
                            Console.WriteLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                        }
                    }
                    var secondNewer = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.SecondNewer).ToList();
                    if (secondNewer.Count > 0)
                    {
                        Console.WriteLine("Newer in 2nd (also found in 1st but older): " + secondNewer.Count + " entries");
                        foreach (var item in secondNewer)
                        {
                            Console.WriteLine("  site: " + item.Second.SiteName + " (login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + ")");
                        }
                    }
                    var conflicts = result.SitesMerged.Where(m => m.Which == Merge.MergedEntry.Resolution.Conflict).ToList();
                    if (conflicts.Count > 0)
                    {
                        Console.WriteLine("Conflicts (site found in both and unclear which is 'better'): " + conflicts.Count + " entries (merged would contain both)");
                        foreach (var item in conflicts)
                        {
                            Console.WriteLine("  site: " + item.First.SiteName);
                            Console.WriteLine("   in 1st: login='" + item.First.Login + "' c=" + item.First.Counter + " t=" + item.First.Type + "");
                            Console.WriteLine("   in 2nd: login='" + item.Second.Login + "' c=" + item.Second.Counter + " t=" + item.Second.Type + "");
                        }
                    }

                    if (null != saveHere)
                    {   // save
                        var merged = new Configuration();

                        merged.UserName = firstConfig.UserName;
                        foreach (var item in result.SitesMerged)
                        {
                            switch (item.Which)
                            {
                                case Merge.MergedEntry.Resolution.Identical:
                                case Merge.MergedEntry.Resolution.FirstNew:
                                case Merge.MergedEntry.Resolution.FirstNewer:
                                    merged.Sites.Add(new SiteEntry(item.First));
                                    break;
                                case Merge.MergedEntry.Resolution.SecondNew:
                                case Merge.MergedEntry.Resolution.SecondNewer:
                                    merged.Sites.Add(new SiteEntry(item.Second));
                                    break;
                                case Merge.MergedEntry.Resolution.Conflict:
                                    // we include both
                                    merged.Sites.Add(new SiteEntry(item.First));
                                    merged.Sites.Add(new SiteEntry(item.Second));
                                    break;
                                default:
                                    break;
                            }
                        }

                        using (var file = File.OpenWrite(saveHere))
                        {
                            merged.Save(file);
                        }
                        Console.WriteLine("Saved merged " + merged.Sites.Count + " entries to '" + saveHere + "'" + 
                            (conflicts.Count > 0 ? " including 2 x " + conflicts.Count + " entries for the conflicts" : "") + ".");
                    }
                    return 1;
                }
                else
                {
                    Console.WriteLine("unknown command " + cmd.CurrentArg);
                    return 1;
                }
            }

            Console.WriteLine("Missing command.");
            PrintHelp();

            return 1; // error
        }

        private static void InteractiveMode()
        {
            Console.WriteLine("Interactive mode. Use -? to show help for further options");

            // Interactive mode
            Console.Write("Enter user name you used:");
            string userName = Console.ReadLine();

            Console.Write("Enter site name to generate the password for:");
            string siteName = Console.ReadLine();

            Console.WriteLine(" Types of passwords:");
            var types = GetPasswordTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Console.WriteLine(" [" + i + "] " + types[i].ToString());
            }

            Console.Write("Enter type of password (0..." + (types.Length - 1) + "):");
            var typeAsString = Console.ReadLine();
            var type = ExtractTypeFromString(types, typeAsString);

            Console.Write("Enter counter for password (allows to change something, use 1 as default):");
            int counter = int.Parse(Console.ReadLine());

            Console.Write("Enter your master password to generate a password for this site:");
            string masterPassword = ReadPassword('*');

            // perform
            Console.WriteLine("Generating password ...");

            GeneratePassword(userName, siteName, type, counter, masterPassword);
        }

        private static int NonInteractiveMode(ParseArgs cmd)
        {
            // configuration
            string userName = null;
            string siteName = null;
            PasswordType? type = null;
            int counter = 1;
            string masterPassword = null;

            if (!cmd.IsValid)
            {
                Console.WriteLine("missing user name");
            }
            else
            {
                userName = cmd.CurrentArg;
            }

            while (cmd.IsValid)
            {
                if (cmd.CurrentArg == "-s")
                {   // site name
                    cmd.Next();
                    if (!cmd.IsValid)
                    {
                        Console.WriteLine("missing site name");
                    }
                    else
                    {
                        siteName = cmd.CurrentArg;
                    }
                }
                else if (cmd.CurrentArg == "-t")
                {   // type of password
                    cmd.Next();
                    if (!cmd.IsValid)
                    {
                        Console.WriteLine("missing type of password");
                    }
                    else
                    {
                        type = ExtractTypeFromString(GetPasswordTypes(), cmd.CurrentArg);
                    }
                }
                else if (cmd.CurrentArg == "-p")
                {   // master password
                    cmd.Next();
                    if (!cmd.IsValid)
                    {
                        Console.WriteLine("missing master password");
                    }
                    else
                    {
                        masterPassword = cmd.CurrentArg;
                    }
                }
                else
                {
                    Console.WriteLine("unexpected argument " + cmd.CurrentArg);
                    return 1;
                }

                cmd.Next();
            }

            // check configuration values - are they configured now?
            bool ok = true;

            if (string.IsNullOrEmpty(userName))
            {   // missing
                Console.WriteLine("Cannot generate password, specify user name in options.");
                ok = false;
            }
            if (string.IsNullOrEmpty(siteName))
            {   // missing
                Console.WriteLine("Cannot generate password, specify site name in options.");
                ok = false;
            }
            if (!type.HasValue)
            {   // missing
                Console.WriteLine("Cannot generate password, specify type of password in options.");
                ok = false;
            }
            if (string.IsNullOrEmpty(masterPassword))
            {   // missing
                Console.WriteLine("Cannot generate password, specify master password in options.");
                ok = false;
            }

            // OK?
            if (!ok)
            {   // configuration not OK
                return 1;
            }

            // perform actual generation
            GeneratePassword(userName, siteName, type.Value, counter, masterPassword);
            return 0;
        }

        /// <summary>
        /// helper: select password type from list based on string
        /// </summary>
        private static PasswordType ExtractTypeFromString(PasswordType[] types, string typeAsString)
        {
            int index;

            // try index
            if (int.TryParse(typeAsString, out index))
            {   // is a number
                return types[index];
            }

            // try to find best match
            foreach (var type in types)
            {
                if (type.ToString().ToLowerInvariant().Contains(typeAsString.ToLowerInvariant()))
                {   // match, OK
                    return type;
                }
            }

            // not found
            throw new ArgumentException("Could not find type, specify as index or a part of the name (case insensitive).");
        }

        /// <summary>
        /// create list of password types
        /// </summary>
        private static PasswordType[] GetPasswordTypes()
        {
            return Enum.GetValues(typeof(PasswordType)).Cast<PasswordType>().ToArray();
        }

        /// <summary>
        /// main algorithm: generate the site specific password
        /// </summary>
        private static void GeneratePassword(string userName, string siteName, PasswordType type, int counter, string masterPassword)
        {
            var masterkey = Algorithm.CalcMasterKey(userName, masterPassword);
            var templateSeed = Algorithm.CalcTemplateSeed(masterkey, siteName, counter);
            var generatedPassword = Algorithm.CalcPassword(templateSeed, type);

            Console.WriteLine(generatedPassword);
        }

        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask.
        /// from http://stackoverflow.com/questions/3404421/password-masking-console-application
        /// </summary>
        /// <param name="mask">a <c>char</c> representing your choice of console mask</param>
        /// <returns>the string the user typed in </returns>
        public static string ReadPassword(char mask)
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

            var pass = new Stack<char>();
            char chr = (char)0;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP)
                {
                    if (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == CTRLBACKSP)
                {
                    while (pass.Count > 0)
                    {
                        Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else
                {
                    pass.Push((char)chr);
                    Console.Write(mask);
                }
            }

            Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }
    }
}
