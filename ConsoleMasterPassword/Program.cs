using MasterPassword.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMasterPassword
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine(".NET Master Password Console");
                Console.WriteLine("Generate derived passwords from a master password for a website.");

                InteractiveMode();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Program stops, error detected:  " + ex.Message);
                return 1;
            }
        }

        private static void InteractiveMode()
        {
            // Interactive mode
            Console.Write("Enter user name you used:");
            string userName = Console.ReadLine();

            Console.Write("Enter site name to generate the password for:");
            string siteName = Console.ReadLine();

            Console.WriteLine(" Types of passwords:");
            var types = Enum.GetValues(typeof(PasswordType)).Cast<PasswordType>().ToArray();

            for (int i = 0; i < types.Length; i++)
            {
                Console.WriteLine(" [" + i + "] " + types[i].ToString());
            }
            Console.Write("Enter type of password (0..." + (types.Length - 1) + "):");
            var type = types[int.Parse(Console.ReadLine())];

            Console.Write("Enter counter for password (allows to change something, use 1 as default):");
            int counter = int.Parse(Console.ReadLine());

            Console.Write("Enter your master password to generate a password for this site:");
            string masterPassword = ReadPassword('*');

            // perform
            Console.WriteLine("Generating password ...");

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

            while ((chr = System.Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP)
                {
                    if (pass.Count > 0)
                    {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (chr == CTRLBACKSP)
                {
                    while (pass.Count > 0)
                    {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else
                {
                    pass.Push((char)chr);
                    System.Console.Write(mask);
                }
            }

            System.Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }
    }
}
