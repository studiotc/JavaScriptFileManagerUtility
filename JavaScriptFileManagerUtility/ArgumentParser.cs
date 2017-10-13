using System;
using System.Collections.Generic;


namespace JavaScriptFileManagerUtility {

    /// <summary>
    /// Class Responsible for for handling command line arguments
    /// </summary>
    public class ArgumentParser {

        private Dictionary<String, CmdLineArg> m_arguments;
        private List< CmdLineArg> m_argument_objs;

        public ArgumentParser() {

            m_arguments = new Dictionary<String, CmdLineArg>();

            m_argument_objs = new List<CmdLineArg>();

        }//end constructor

        /// <summary>
        /// Add an Argument for the parser to look for
        /// </summary>
        /// <param name="sname">Short name of the argument</param>
        /// <param name="lname">Long name of the argument</param>
        /// <param name="has_opt">True if there is an option to follow the argument, i.e.: -a 'argument' </param>
        /// <param name="help">Help string to display when help is called</param>
        /// <returns>CmdLineArg object that can be queried after the command line options are parsed</returns>
        public CmdLineArg AddArgument(string sname, string lname, bool has_opt, string help) {

            CmdLineArg arg = new CmdLineArg(sname, lname, has_opt, help);
            

            if(m_arguments.ContainsKey(sname)) {
                Console.WriteLine("Argument name: " + sname + " is already in use. Disregarding name.");
                return null;
            }

            if (m_arguments.ContainsKey(lname)) {
                Console.WriteLine("Argument name: " + lname + " is already in use. Disregarding name.");
                return null;
            }

            //add argument names to the dictionary
            m_arguments.Add(sname, arg);
            m_arguments.Add(lname, arg);

            //add argument object to the list
            m_argument_objs.Add(arg);

            return arg;

        }

        /// <summary>
        /// Check if the argument name already exists in the dictionary
        /// </summary>
        /// <param name="arg">Arguemnt name to check</param>
        /// <returns>True if the argument name exists already</returns>
        private bool argExists(string arg) {
            if(m_arguments.ContainsKey(arg)) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        /// <param name="args"></param>
        public bool parseArgs(string[] args) {

            int arg_length = args.Length;
            int i = 0;

            while (i < arg_length) {

                //get the arguemnt and clean it up
                string arg = args[i];
                arg = arg.Trim();

                if(arg.Length >= 2) {

                    string head = arg.Substring(0, 1);
                    if (head == "-") {
                        arg = arg.Remove(0, 1);
                    } else {
                        //malformed option....
                        parseError("Malformed argument: " + arg);
                        return false;
                    }

                    if(argExists(arg)) {
                        CmdLineArg cla = m_arguments[arg];

                        if(cla.Present) {
                            //error - duplicate argument
                            parseError("Duplicate argument: " + arg);
                            return false;

                        } else {
                            //flag as present
                            cla.Present = true;

                            //check for option
                            if (cla.HasOption) {
                                int next = i + 1;
                                if (next < arg_length) {

                                    string arg_opt = args[next];
                                    cla.Option = arg_opt;

                                    //incemrent i for option
                                    i += 1;

                                } else {
                                    //error - out or arguments...
                                    parseError("Missing argument option for: " + arg);
                                    return false;

                                }

                            }


                        }
                        


                    } else {
                        //argument not recognized
                        parseError("Argument not recognized: " + arg);
                        return false;
                    }


                } else {
                    //malformed, needs to be at least "-a"
                    parseError("Malformed argument: " + arg);
                    return false;

                }

                //increment i for argument
                //i is also incremented above for options
                i += 1;


            }//end while

            return true;

        }//end parse arguments

        /// <summary>
        /// Wrapper for error handling
        /// </summary>
        /// <param name="err">Error message to display</param>
        private void parseError(string err) {
            Console.WriteLine("Error parsing command line arguments: " + err);
           
        }

        /// <summary>
        /// Display the help guid to the Console
        /// </summary>
        public void displayHelp() {

            Console.WriteLine(">> Help:");

            Console.WriteLine("The Javascript File Manager Utility performs two functions:");
            Console.WriteLine("1: Compiling multiple javascript files into one complete file.");
            Console.WriteLine("2: Generate HTML script tags for embedding in a web page - intended for use during development to allow for changess to file structure.");
            Console.WriteLine("Note: The manifest file is used to control the order in which files are processed.");
            Console.WriteLine("  You can generate a manifest file, then edit the order of the files.");
            Console.WriteLine("  The manifest will be read for -compile and -links arguments");

            foreach (CmdLineArg a in m_argument_objs) {
                Console.Write(a.getHelpText());
            }

        }


    }//end class
}
