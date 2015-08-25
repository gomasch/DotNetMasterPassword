namespace ConsoleMasterPassword
{
    /// <summary>
    /// helper class to go through command line arguments
    /// </summary>
    public class ParseArgs
    {
        private string[] Args;
        private int CurrentIndex;

        /// <summary>
        /// constructor to set the arguments.
        /// The first item is the current arg then. Use IsValid if there is one and Next() to go to the next one.
        /// </summary>
        /// <param name="args"></param>
        public ParseArgs(string[] args)
        {
            if (args == null)
            {
                args = new string[0]; // empty array
            }
            Args = args;
        }

        /// <summary>
        /// Switch to next argument.
        /// If there is no next argument anymore, IsValid will turn false
        /// </summary>
        /// <returns>state of IsValid after the change</returns>
        public bool Next()
        {
            if (IsValid)
            {   // we can go on
                CurrentIndex++;
            }

            return IsValid;
        }

        /// <summary>
        /// specifies if we have valid current argument
        /// </summary>
        public bool IsValid
        {
            get { return CurrentIndex < Args.Length;  }
        }

        /// <summary>
        /// gets the current argument.
        /// </summary>
        public string CurrentArg
        {
            get { return Args[CurrentIndex]; }
        }

    }
}
