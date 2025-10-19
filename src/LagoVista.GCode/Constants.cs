// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4fbc589bff3312890b4d0b847c67758a0b624e9a915ec1ed4b8568bc45f51f5e
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode
{ 
    public enum ParseDistanceMode
    {
        Absolute,
        Relative
    }

    public enum ParseUnit
    {
        Metric,
        Imperial
    }

    public enum ArcPlane
    {
        XY = 0,
        YZ = 1,
        ZX = 2
    }

    public enum ArcDirection
    {
        CW,
        CCW
    }
}
