// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 631321107ef3ccf43e9878cb49b89d41de0ab88860e8fe7dba8c012ea24baa5b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.GCode.Commands;
using LagoVista.GCode.Parser;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode
{
    public partial class GCodeFile
    {
        public static GCodeFile Load(string path, ILogger logger = null)
        {
            var parser = new GCodeParser(logger == null ? Services.Logger : logger);
            parser.Reset();
            parser.ParseFile(path);

            return new GCodeFile(parser.Commands) { FileName = path.Substring(path.LastIndexOf('\\') + 1) };
        }

        public static GCodeFile FromList(IEnumerable<string> file, ILogger logger = null)
        {
            var parser = new GCodeParser(logger == null ? Services.Logger : logger);
            parser.Reset();
            parser.Parse(file);

            return new GCodeFile(parser.Commands) { FileName = "output.nc" };
        }

        public static GCodeFile FromString(String contents, ILogger logger = null)
        {
            var commands = contents.Split('\n');
            return FromList(commands, logger);
        }

        public static GCodeFile FromCommands(List<GCodeCommand> commands)
        {
            return new GCodeFile(commands) { FileName = "output.nc" };
        }

        public static GCodeFile GetEmpty()
        {
            return new GCodeFile(new List<GCodeCommand>());
        }
    }
}
