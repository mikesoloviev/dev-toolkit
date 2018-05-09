using System;
using System.Linq;
using System.IO;
using System.Text;

// Setup: 
// Add to Visual Studio External Tools like this:
// Open: Tools > External Tools
// Click: Add
// Set Title to: File Injector
// Set Command to the path to fileinjector.exe
// Set Arguments to the name of fileinjector task file -- optionally, deafult is fileinjector.task
// Set Initial directory to: $(ProjectDir)
// Check: Use output window
// Click: Apply and OK

// Usage: 
// Create the file 'fileinjector.task' with directive(s) (see formats below) in the project directory
// Run: Tools > File Injector

// Directive formats
// <InPath> -> <OutPath> :: <Namespace>.<Class>.<Field>
// <InPathAndFileFilter> => <OutPath> 

// Notes: 
// (1) Paths are relative to the project folder
// (2) Directives can be commented out with '--' prefix

// Examples:
// Submark/Stylesheet.css -> Submark/Stylesheet.cs :: Submark.Stylesheet.Style
// ../Submark/Submark/*.* => Submark

namespace FileInjector {

    class Program {

        static string directiveFile = "fileinjector.task";
        static string projectDirectory = Environment.CurrentDirectory;

        static void Main(string[] args) {
            if (args.Length > 0) directiveFile = args[0];
            var directivePath = Path.Combine(projectDirectory, directiveFile);
            if (!File.Exists(directivePath)) {
                Console.WriteLine($"Directive file not found: {directivePath}");
                return;
            }
            foreach (var directive in File.ReadAllLines(directivePath)) {
                if (directive.Trim().Length == 0 || directive.StartsWith("--")) {
                    // skip
                }
                else if (directive.Contains("->")) {
                    InjectText(directive.Trim());
                }
                else if (directive.Contains("=>")) {
                    InjectFiles(directive.Trim());
                }
            }
        }

        static void InjectText(string directive) {
            // parse directive
            var tokens = directive.Replace("->", "|").Replace("::", "|").Split('|');
            if (tokens.Length < 3) {
                Console.WriteLine($"Invalid directive: {directive}");
                return;
            }
            // input
            var inPath = Path.Combine(projectDirectory, tokens[0].Trim().Replace("/", @"\"));
            if (!File.Exists(inPath)) {
                Console.WriteLine($"Input file not found: {inPath}");
                return;
            }
            var text = File.ReadAllText(inPath);
            // output
            var outPath = Path.Combine(projectDirectory, tokens[1].Trim().Replace("/", @"\"));
            var parts = tokens[2].Split('.');
            if (parts.Length < 3) {
                Console.WriteLine($"Invalid ':: namespace.class.field' description: {tokens[2]}");
                return;
            }
            var field = parts[parts.Length - 1];
            var type = parts[parts.Length - 2];
            var space = string.Join(".", parts.Take(parts.Length - 2));
            var content = new StringBuilder();
            content.AppendLine($"namespace {space} " + " {");
            content.AppendLine($"    public class {type}" + " {");
            content.AppendLine($"        public static string {field} = @\"");
            content.AppendLine(text);
            content.AppendLine("\";");
            content.AppendLine("    }");
            content.AppendLine("}");
            try {
                File.WriteAllText(outPath, content.ToString());
            }
            catch (Exception error) {
                Console.WriteLine($"File write error: {error.Message}");
            }
        }

        static void InjectFiles(string directive) {
            // TODO
        }

    }
}
