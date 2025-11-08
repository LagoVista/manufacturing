// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2ec886ee3da5aac1738e639a69e4bce627e2b6eb277b4ea1218adf67a7a69665
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Parser
{
    public interface IGCodeParser
    {
        GCodeCommand ParseLine(string line, int index);
    }
}
