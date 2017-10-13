using System;


namespace JavaScriptFileManagerUtility {

    /// <summary>
    /// Class for representing a commandline argument.  It stores the short and long name as well
    /// as a flag for optional values and a help string.
    /// </summary>
    public class CmdLineArg {

        /// <summary>
        /// The short name of the argument
        /// i.e.: "-a"
        /// </summary>
        private string m_short_name;

        /// <summary>
        /// The long name of the argument
        /// i.e.e: "-argument"
        /// </summary>
        private string m_long_name;

        /// <summary>
        /// Flag indicatiing wether there is an option
        /// associated with the argument
        /// i.e.: "-directory C:\web_devel\project1"
        /// </summary>
        private bool m_has_option;

        /// <summary>
        /// If there is an option found during parsing, it is stored here
        /// </summary>
        private string m_option;


        /// <summary>
        /// Flag to indicate if the argument was present
        /// after parsing
        /// </summary>
        private bool m_present;


        /// <summary>
        /// A help string for onscreen display of help
        /// </summary>
        private string m_help;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sname">Argument's short name</param>
        /// <param name="lname">Argument's long name</param>
        /// <param name="has_opt">Flag for argument option</param>
        /// <param name="help">Help string for guide</param>
        public CmdLineArg(string sname, string lname, bool has_opt, string help) {

            m_short_name = sname;
            m_long_name = lname;


            m_has_option = has_opt;
            m_present = false;

            m_option = "";

            m_help = help;

        }

        /// <summary>
        /// The argument's short name
        /// </summary>
        public string  ShortName {
            get {
                return m_short_name;
            }

            set {
                m_short_name = value;
            }
        }

        /// <summary>
        /// The arguemnt's long name
        /// </summary>
        public string LongName {
            get {
                return m_long_name;
            }

            set {
                m_long_name = value;
            }
        }

        /// <summary>
        /// Flag indicatiing wether there is an option
        /// associated with the argument
        /// </summary>
        public bool HasOption {
            get {
                return m_has_option;
            }

            set {
                m_has_option = value;
            }
        }

        /// <summary>
        /// This is the Option text for arguemnts with options
        /// Only single options are supported
        /// </summary>
        public string Option {
            get {
                return m_option;
            }
            set {
                m_option = value;
            }
        }


        /// <summary>
        /// This is the flag to indicate wether the option was found during parsing
        /// </summary>
        public bool Present {
            get {
                return m_present;
            }
            set {
                m_present = value;
            }
            
        }


        /// <summary>
        /// Get a formatted help string
        /// </summary>
        /// <returns></returns>
        public string getHelpText() {

            string help = "Argument: -" + m_short_name + " | -" + m_long_name;
            
            if(HasOption) {
                help += " [Option]";
            }

            help += Environment.NewLine;
            help += "   " + m_help + Environment.NewLine; 

            return help;

        }

    }//end class
}
