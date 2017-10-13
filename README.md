## Synopsis

This is a utility for the Windows Command line to help manage larger JavaScript projects.
It was created to help with a my first large javascript project, and grew out wanting something a little nicer than shell scripts...  This has only been tested on an ES6 style project.


It serves two major functions:
1) Generating HTML link tags from a directory of javascript files (sub-directories included).
2) Compiling into a minimized format for release.

The utility can work on all discovered files in a directory structure or a manifest file can be generated to control link and compilation order.

## Recommended Usage

Add the program to your path in windows.
Navigate to your javascript directory root.
Generate a manifest file.
Edit the file order in the manifest if need be.
Remove files from the manifest to exclude.
Generate HTML <script> links if continuing to work with multiple files.
Compile minimum javascript file.


## Usage

From the root of your javascript file tree:
Create a manifest file for editing:
    jsfmu -m

Create html links:
    jsfmu -l

Compile (minimize) a set of files:
    jsfmu -c -min

For further help:
    jsfmu -help



## Links

Creates text links of javascript files for pasting into an HTML document.  This allows for re-organization without having to recreate all the links during development.

## Minimize

Each javascript file is turned into a single line of text and written to a single file.
Currently it does not do a very aggressive minimization, but handles all typical commenting cases: multiline, inline, and single line comments.


## License

MIT (see License.txt)
