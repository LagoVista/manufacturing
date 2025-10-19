// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 15dcd7875a649d6ff72d8f8c6b7e93c0cad544b7a09bbe9a27f07260495fab32
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartKicadPcb
{
    public class Net
    {
        [KicadParameter(0)]
        public int Number { get; set; }

        [KicadParameter(1)]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Number}: {Name}";
        }
    }
}
