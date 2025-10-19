// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2a94a76a4e0e8800d237db4973bac21199b4b973161333a29f10b428c57b3b7b
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public enum HeightMapProbePointStatus
    {
        NotProbed,
        Probed
    }
    public class HeightMapProbePoint
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public LagoVista.Core.Models.Drawing.Vector3 Point { get; set; }

        public HeightMapProbePointStatus Status { get; set; }
    }
}
