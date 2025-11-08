// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 970e584cbdd8ce2c9cdd97d4d4d975fe1581993eab793d0a2057aefc8fefc060
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode
{
    public class Word
    {
        public char Command { get; set; }
        public double Parameter { get; set; }
        public string FullWord { get; set; }
    }
}
