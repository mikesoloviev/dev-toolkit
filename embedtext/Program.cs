using System;
using System.Linq;
using System.IO;
using System.Text;

// Setup: 
// Add to Visual Studio External Tools like this:
// Open: Tools > External Tools
// Click: Add
// Set Title to: Embed Text
// Set Command to the path to embedtext.exe
// Set Arguments to: $(ItemPath)
// Set Initial directory to: $(ProjectDir)
// Click: Apply and OK

// Usage: 
// Create the file with embed directive(s) (see format below)
// Select the file with embed directive in editor
// Run: Tools > Embed Text

// Embed directive format
/* <InPath> => <OutPath> :: <Namespace>.<Class>.<Field> */

// Note: Paths are relative to the project folder

// For example:
// Markease/Stylesheet.css => Markease/Stylesheet.cs :: Markease.Stylesheet.Style

namespace EmbedText {

    class Program {

        static void Main(string[] args) {
            if (args.Length < 1) return;
            var directivePath = args[0];
            if (!File.Exists(directivePath)) {
                Console.WriteLine($"Directive file not found: {directivePath}");
                return;
            }
            foreach (var directive in File.ReadAllLines(directivePath)) {
                Embed(directive.Trim());
            }
        }

        static void Embed(string directive) {
            // parse directive
            if (directive.Length == 0) return;
            var tokens = directive.Replace("=>", "|").Replace("::", "|").Split('|');
            if (tokens.Length < 3) {
                Console.WriteLine($"Invalid directive: {directive}");
                return;
            }
            // input
            var projectDirectory = Environment.CurrentDirectory;
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
                Console.WriteLine($"Invalid C# namespace.class.field description: {tokens[2]}");
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
            File.WriteAllText(outPath, content.ToString());
        }
    }
}
