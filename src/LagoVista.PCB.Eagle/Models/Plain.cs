// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0a9c293d4f36e13014bb9e1e00d84726f536321e9d3ee73f46635835430282ce
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Plain
    {
        public List<PcbLine> Wires { get; set; }
        public List<Text> Texts { get; set; }

        public static Plain Create(XElement element)
        {
            return new Plain()
            {
                Wires = (from childWires in element.Descendants("wire") select PcbLine.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
            };
        }
    }
}
