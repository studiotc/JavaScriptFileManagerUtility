using System;
using System;
using System.Collections.Generic;

using System.IO;




namespace JavaScriptFileManagerUtility {
    class Program {

        /// <summary>
        /// Standard file prefix used to identify files for filtering
        /// otherwise it will consume it's own compiled files...
        /// </summary>
        private const string FILE_PREFIX = "jsfmu_";

        /// <summary>
        /// Manifest file name
        /// </summary>
        private const string FILE_MANIFEST = "manifest.txt";

        /// <summary>
        /// HTML Links file name
        /// </summary>
        private const string FILE_HTML = "htmllinks.txt";

        /// <summary>
        /// Compiled file name
        /// </summary>
        private const string FILE_COMPILE = "compiled.js";

        /// <summary>
        /// Compiled Minimum file name
        /// </summary>
        private const string FILE_COMPILE_MIN = "compiled_min.js";


        /// <summary>
        /// Begining of long comment string
        /// </summary>
        private const string COMM_LB = "/*";
        /// <summary>
        /// End of long comment string
        /// </summary>
        private const string COMM_LE = "*/";
        /// <summary>
        /// Single line comment string
        /// </summary>
        private const string COMM_SL = "//";


        /// <summary>
        /// Empty constructor - eneded up not needing it
        /// </summary>
        public Program() {

            //nothing needed here
        }


        /// <summary>
        /// INtialize and run the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {

            Program p = new Program();
            p.run(args);


        }



        /// <summary>
        /// Run the Program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public void run(string[] args) {

            ArgumentParser parser = new ArgumentParser();

            string local_path = Environment.CurrentDirectory;
            Console.WriteLine("<<< Javascript File Manager Utility >>>");



            //init the arguments
            CmdLineArg arg_help = parser.AddArgument("h", "help", false, "Displays the program arguments");
            CmdLineArg arg_dir = parser.AddArgument("d", "directory", true, "Sets the working directory, current program directory is used if omitted.");

            CmdLineArg arg_manifest = parser.AddArgument("m", "manifest", false, "Generate manifest file.  The manifest can be edited for compilation order.  This argument must be used alone, with no other arguments");

            CmdLineArg arg_compile = parser.AddArgument("c", "compile", false, "Compile all the javascript files into one text file.  Uses manifest if present.");
            CmdLineArg arg_compile_min = parser.AddArgument("min", "minimum", false, "Compile option argument to a minimum compilation by removing comments and line breaks.");
            CmdLineArg arg_links = parser.AddArgument("l", "links", false, "Generate html link tags for all the javascript files.  Uses manifest if present.");

            CmdLineArg arg_ignore = parser.AddArgument("i", "ignore", false, "Ignore manifest file.  Ignores the manifest file if present.  THe program will use all the files it discovers.");

            //check for arguments after 
            //the command line args are intialized so they display
            int al = args.Length;

            //if no arguments, display help and exit
            if (al == 0) {
                parser.displayHelp();
                holdConsole();
                return;
            }

            //if it failed to parse the arguments display help and exit
            if (!parser.parseArgs(args)) {
                parser.displayHelp();
                holdConsole();
                return;
            }


            //check for help
            if (arg_help.Present) {
                parser.displayHelp();
            }

            //check for working directory
            if (arg_dir.Present) {
                string w_dir = arg_dir.Option;
                if (!Directory.Exists(w_dir)) {

                    Console.WriteLine("Working directory could not be set, directory not found: ");
                    Console.WriteLine(w_dir);
                    holdConsole();
                    return;
                }

                //set the working directory
                try {
                    Directory.SetCurrentDirectory(w_dir);
                    local_path = w_dir;

                } catch (Exception ex) {

                    Console.WriteLine("An error occured setting the working directory:");
                    Console.WriteLine(w_dir);
                    Console.WriteLine("Exception: " + ex.Message);
                    holdConsole();
                    return;

                }




            }//end if directory argument


            //check for manifest flag - special case taht needs to be handled differenty
            // this is for generating the manifest only - will return early
            if (arg_manifest.Present) {

                //check for invalid flags - warn and exit
                if (arg_compile.Present || arg_compile_min.Present || arg_links.Present || arg_ignore.Present) {
                    Console.WriteLine("Error: invalid flag used with -" + arg_manifest.LongName + ", argument must be used exclusively.");
                    holdConsole();
                    return;
                }

                //create the manifest - use the filtered file list to generate
                // the manifest list - filters out generated files
                string[] m_files = getFilteredFileList();

                if (m_files.Length > 0) {
                    createManifest(local_path, ref m_files);
                } else {
                    Console.WriteLine("No files found to generate manifest.");
                }


                //end manifest creation
                holdConsole();
                return;
            }//end ar_manifest



            //check for -minimum flag
            if (arg_compile_min.Present && !arg_compile.Present) {
                Console.WriteLine("Error: -" + arg_compile_min.LongName + " argument with no -" + arg_compile.LongName + " argument");
                holdConsole();
                return;
            }




            //setup files to work with
            string[] files = null;

            //ignore manifest? - force local files with manifest flag
            if (arg_ignore.Present) {
                //get the local files
                Console.WriteLine("Using local files.");
                files = getFilteredFileList();


            } else {
                //try to get manifest file
                string manifest_fn = FILE_PREFIX + FILE_MANIFEST;
                string[] manifest_files = Directory.GetFiles(".", manifest_fn, SearchOption.TopDirectoryOnly);

                //was the file found
                if (manifest_files.Length == 0) {
                    Console.WriteLine("No Manifest file present.  Using local files scan.");
                    //get the local files if no manifest
                    files = getFilteredFileList();
                } else {

                    //load manifest file
                    string mpath = manifest_files[0];
                    files = getManifestList(mpath);
                    //check files
                    if (files == null) {
                        Console.WriteLine("Failed to load manifest file: " + mpath);
                        holdConsole();
                        return;
                    } else {
                        Console.WriteLine("Using manifest file: " + mpath);

                    }

                }

            }

            //check files
            if (files == null) {
                Console.WriteLine("Failed to get files to process at: " + local_path);
                holdConsole();
                return;
            }

            //number of files
            int fl = files.Length;

            //files to process?
            if (fl == 0) {
                Console.WriteLine("No Javascript files to process, exiting");
                holdConsole();
                return;
            }


            //generate html links?
            if (arg_links.Present) {
                createHTMLLinks(local_path, ref files);
            }


            //compile javascript
            if (arg_compile.Present) {
                if (arg_compile_min.Present) {
                    //minimum compile
                    compileMinimum(local_path, ref files);
                } else {
                    //standard compile
                    compile(local_path, ref files);
                }

            }


            //hold the console before exiting
            holdConsole();



        }//end run

        /// <summary>
        /// Hold the console from exiting
        /// This is just for debugging to hold the console
        /// </summary>
        private void holdConsole() {
#if DEBUG
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
#endif

        }

        /// <summary>
        /// Get a list of all files while filtering out 
        /// ones generated by this utility
        /// </summary>
        /// <returns>List of all javascript files</returns>
        private string[] getFilteredFileList() {

            string[] files = Directory.GetFiles(".", "*.js", SearchOption.AllDirectories);

            List<string> to_filter = new List<string>();

            foreach (string p in files) {

                string s = Path.GetFileName(p);
                if (!s.StartsWith(FILE_PREFIX)) {
                    to_filter.Add(p);
                }

            }

            //push back filtered file list
            files = to_filter.ToArray();

            return files;

        }

        /// <summary>
        /// Create the html script link text
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private void createHTMLLinks(string path, ref string[] files) {

            string fpath = path + Path.DirectorySeparatorChar + FILE_PREFIX + FILE_HTML;

            try {
                using (StreamWriter writer = new StreamWriter(fpath)) {

                    int fc = files.Length;
                    string line = "";

                    for (int i = 0; i < fc; i++) {
                        string tmp_line = files[i];
                        tmp_line = tmp_line.Replace("\\", "/");//force formward slashes

                        line = "<script src='";
                        line += tmp_line; // files[i];
                        line += "'></script>";
                        writer.WriteLine(line);


                    }

                    writer.Flush();
                    writer.Close();

                    Console.WriteLine(">> HTML links created:");
                    Console.WriteLine(fpath);

                }
            } catch (Exception ex) {

                Console.WriteLine("An error occured attempting to generate the HTML links file at:");
                Console.WriteLine(path);
                Console.WriteLine("Error: " + ex.Message);


            }//end try/catch



        }


        /// <summary>
        /// Create a manifest file
        ///This is a list of all the javascript files in the current directory 
        ///and all subdirectories.  THe manifest can be modified to control write order
        ///in compile mode and html link mode
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private void createManifest(string path, ref string[] files) {

            string fpath = path + Path.DirectorySeparatorChar + FILE_PREFIX + FILE_MANIFEST;

            try {
                using (StreamWriter writer = new StreamWriter(fpath)) {

                    int fc = files.Length;

                    for (int i = 0; i < fc; i++) {

                        writer.WriteLine(files[i]);

                    }

                    writer.Flush();
                    writer.Close();

                    Console.WriteLine(">> Manifest file created:");
                    Console.WriteLine(fpath);

                }
            } catch (Exception ex) {

                Console.WriteLine("An error occured attempting to generate the manifest file at:");
                Console.WriteLine(path);
                Console.WriteLine("Error: " + ex.Message);


            }//end try/catch



        }

        /// <summary>
        /// Get the manifest list from the supplied file path
        /// </summary>
        /// <param name="path">Path of the manifest file</param>
        /// <returns>Returns the manifest list - list of locally pathed files</returns>
        private string[] getManifestList(string path) {


            try {
                using (StreamReader reader = new StreamReader(path)) {

                    string file = reader.ReadToEnd();
                    string nl = Environment.NewLine;
                    //not sure why, but there were extra lines....
                    string[] lines = file.Split(nl.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    return lines;

                }
            } catch (Exception ex) {

                Console.WriteLine("An error occured attempting to generate the manifest file at:");
                Console.WriteLine(path);
                Console.WriteLine("Error: " + ex.Message);

                return null;

            }//end try/catch


        }

        /// <summary>
        /// Compile all the javascript files into one file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private void compile(string path, ref string[] files) {
            //build compile file path
            string fpath = path + Path.DirectorySeparatorChar + FILE_PREFIX + FILE_COMPILE;

            try {
                using (StreamWriter writer = new StreamWriter(fpath)) {

                    int fc = files.Length;

                    for (int i = 0; i < fc; i++) {

                        string js_file = files[i];

                        using (StreamReader reader = new StreamReader(js_file)) {

                            string contents = reader.ReadToEnd();

                            writer.Write(contents);

                            reader.Close();

                        }//end using reader


                    }//end for

                    writer.Flush();
                    writer.Close();

                    Console.WriteLine(">> Compiled javascript file created:");
                    Console.WriteLine(fpath);

                }
            } catch (Exception ex) {

                Console.WriteLine("An error occured attempting to generate the compiled file at:");
                Console.WriteLine(path);
                Console.WriteLine("Error: " + ex.Message);


            }//end try/catch



        }


        /// <summary>
        /// Compile all the javascript files into one file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private void compileMinimum(string path, ref string[] files) {
            //build compile minimum file path
            string fpath = path + Path.DirectorySeparatorChar + FILE_PREFIX + FILE_COMPILE_MIN;

            bool search_long_comm = false;
            bool do_write = true;

            try {
                using (StreamWriter writer = new StreamWriter(fpath)) {

                    int fc = files.Length;

                    for (int i = 0; i < fc; i++) {

                        string js_file = files[i];

                        using (StreamReader reader = new StreamReader(js_file)) {


                            //something to process?
                            while (reader.Peek() > 0) {

                                //prepare 
                                do_write = false;
                                //grab the line
                                string line = reader.ReadLine();

                                //check for empty
                                if (!String.IsNullOrWhiteSpace(line)) {


                                    //searching for end of long comment?
                                    if (search_long_comm) {

                                        //check the line for end of long comment
                                        do_write = lineCheckCommentOn(ref line, ref search_long_comm);


                                    } else {

                                        //check line for comments
                                        do_write = lineCheckCommentOff(ref line, ref search_long_comm);


                                    }//end if long comm search

                                    if (do_write) {

                                        // System.Diagnostics.Debug.Print("writing line:" + line);
                                        line = line.Trim();

                                        if (!String.IsNullOrWhiteSpace(line)) {
                                            //make the line safe
                                            makeLineSafe(ref line);
                                            //write to file with no line ending
                                            writer.Write(line);
                                        }

                                    }



                                } //end if null or whitespace


                            }//end while

                            //write a newline for every file
                            writer.Write(Environment.NewLine);

                        }//end using reader


                    }//end for

                    writer.Flush();
                    writer.Close();

                    Console.WriteLine(">> Minimum compiled javascript file created:");
                    Console.WriteLine(fpath);

                }//end using

            } catch (Exception ex) {

                Console.WriteLine("An error occured attempting to generate the minimum compiled file at:");
                Console.WriteLine(path);
                Console.WriteLine("Error: " + ex.Message);


            }//end try/catch



        }//end compile minimum

        /// <summary>
        /// Make sure line is safe to write
        /// Since javascript will let you write statements across multiple lines i.e.:
        /// var 
        /// myString = "this is my string";
        /// Need to check for this condition and add a space to the string so
        /// the statement doesn't get invalidated (merged tokens) when pushed to a single line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private void makeLineSafe(ref string line) {


            //lsat character in line
            char last = line[line.Length - 1];
            //check the character - these are okay without space
            if (last != '{' && last != '}' && last != ';' && last != ',') {
                line += " ";
            }


        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns>True if triggered search for long comment</returns>
        private bool lineCheckCommentOff(ref string line, ref bool search) {

            int cbi = line.IndexOf(COMM_LB); //comment begin
            int cei = line.IndexOf(COMM_LE); //comment end
            int sli = line.IndexOf(COMM_SL); //single line comm

            //flag that a long comment has begun on this line
            bool do_write = true;

            //long comment on one line?'/* comment */'   (Note: /*/ will break this...)
            if (cbi >= 0 && cei >= 0) {
                //single line comment after it? //comment
                if (sli >= 0 && cei < sli) {
                    //remove the comment
                    removeEmbededComment(ref line);
                    //trim off end comment
                    do_write = trimRightOf(ref line, COMM_SL);
                    //single line comment before it?
                } else if (sli >= 0 && sli < cbi) {
                    //it's getting trimmed
                    do_write = trimRightOf(ref line, COMM_SL);

                } else {
                    //remove the comment
                    removeEmbededComment(ref line);

                }


                //long comment begin '/*', no '*/'
            } else if (cbi >= 0 && cei < 0) {
                //single line comment in front of it  'data // comm /* something'
                if (sli >= 0 && sli > cbi) {
                    //trim from  '//'
                    do_write = trimRightOf(ref line, COMM_SL);
                    //long begin in front of single line 'data /* comm // something'
                } else if (sli >= 0 && cbi > sli) {
                    //trim from '/*'
                    do_write = trimRightOf(ref line, COMM_LB);
                    //set flag here for search
                    search = true;
                    //no other symbols, simple comment begin ' data /* comm' 
                } else {
                    //kept seperate from above code for clarity
                    //trim from ''
                    do_write = trimRightOf(ref line, COMM_LB);
                    //set flag here for search
                    search = true;
                }

            } else if (sli >= 0) {
                //trim from '//'
                do_write = trimRightOf(ref line, COMM_SL);
            }


            //return the write flag
            return do_write;


        }//end  lineCheckBegin


        /// <summary>
        /// Check the current line when  flag for long comment '/*' is on.
        /// Need to find the next ''.  This routine will not handle very complicated
        /// scenarios.  Single line comments will be recognized after the end of a long comment
        /// </summary>
        /// <param name="line">Current line to check and clean</param>
        /// <returns>True if a long comment begins</returns>
        private bool lineCheckCommentOn(ref string line, ref bool search) {

            int cei = line.IndexOf(COMM_LE);  //comment end
            int sli = line.IndexOf(COMM_SL); //single line comm

            //comment is active - no writing by default
            bool do_write = false;

            //look for first '*/' to end long comment
            if (cei >= 0) {
                do_write = trimLeftOf(ref line, COMM_LE);
                //found it, turn off search
                search = false;

                //look for comment on end //comment
                sli = line.IndexOf(COMM_SL);
                if (sli >= 0) {
                    do_write = trimRightOf(ref line, COMM_SL);
                }

            }


            return do_write;

        }//end lineCheckCommentOn

        /// <summary>
        /// Removes an inline /* comment */ from a line
        /// </summary>
        /// <param name="line">Line to trim</param>
        private bool removeEmbededComment(ref string line) {
            string a = line; //first part
            string b = line; //second part

            bool tl = trimRightOf(ref a, COMM_LB); //keep left
            bool tr = trimLeftOf(ref b, COMM_LE); //keep right
            //is there text to keep?
            if (tl && tr) {
                line = a + " " + b; //splice with a space for safety so tokens don't get merged
            } else if (tl) {
                line = a;
            } else if (tr) {
                line = b;
            }

            //return write flag - true to write text
            return (tl || tr);

        }



        /// <summary>
        /// Trim text from the string to the right of the symbol (text string).
        /// The symbol is included in the trim (removed from final text).
        /// </summary>
        /// <param name="line"></param>
        /// <param name="symb"></param>
        /// <returns>True if there is text remaining after trim</returns>
        private bool trimRightOf(ref string line, string symb) {

            int idx = line.IndexOf(symb);
            if (idx == 0) {
                //nothing to the left
                return false;

            } else if (idx > 0) {
                line = line.Substring(0, idx);

                if (String.IsNullOrEmpty(line)) {
                    return false;
                } else {
                    return true;
                }

            } else {
                //no text found keep
                return true;
            }

        }



        /// <summary>
        /// Trim text from the string to the left of the symbol (text string).
        /// The symbol is included in the trim (removed from final text).
        /// </summary>
        /// <param name="line">Text line to trim</param>
        /// <param name="symb">Symbol to find</param>
        /// <returns>True if there is text remaining after trim</returns>
        private bool trimLeftOf(ref string line, string symb) {

            int idx = line.IndexOf(symb);
            if (idx >= 0) {

                int sl = symb.Length;
                int len = line.Length;
                //nothing to the right
                if (idx == len - sl) {
                    return false;
                }
                line = line.Substring(idx + sl, len - idx - sl);

                if (String.IsNullOrEmpty(line)) {
                    return false;
                } else {
                    return true;
                }

            } else {
                //no text found keep
                return true;
            }

        }



    }//end class
}
