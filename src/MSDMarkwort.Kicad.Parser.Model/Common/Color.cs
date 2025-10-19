// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e96fcde8e5e9179dfef3aa87c6f1a7067e106651a2b6487ecafad47e1b476234
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Color
    {
        [KicadParameter(0)]
        public int Red { get; set; }

        [KicadParameter(1)]
        public int Green { get; set; }

        [KicadParameter(2)]
        public int Blue { get; set; }

        [KicadParameter(3)]
        public double Alpha { get; set; }

        public string ToString(string layer)
        {
            if (Red > 0 || Green > 0 || Blue > 0)
            {
                if (Alpha == 0)
                    Alpha = 0xFF;

                return $"#{(byte)Alpha:X}{(byte)Red:X}{(byte)Green:X}{(byte)Blue:X}";
            }
            else
            {
                return LayerToColor(layer) ;
            }
        }

        public static string LayerToColor(string layerName)
        {
            switch (layerName)
            {
                case "F.Cu": return "#c83434";
                case "B.Cu": return "#4d7fc4";
                case "F.Silkscreen":
                case "F.SilkS": return "#f2eda1";
                case "B.Silkscreen":
                case "B.SilkS": return "#e8b2a7";
                case "Edge.Cuts": return "";
                case "F.Fab": return "#afafaf";
                case "B.Fab": return "#585d84";
                case "F.Mask": return "#562866";
                case "B.Mask": return "#177c76";
                case "F.Paste": return "#a5938e";
                case "B.Paste": return "#00aeae";
                case "F.CrtYd":
                case "F.Courtyard": return "#ff26e2";
                case "B.CrtYd":
                case "B.Courtyard": return "#26e9ff";
                default: return $"#FFFFFFFF";
            }
        }
    }
}
