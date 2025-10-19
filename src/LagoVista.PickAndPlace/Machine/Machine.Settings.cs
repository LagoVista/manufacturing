// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d8b5ca02305cabe0ca2ca853aa84369750bf83d805c1ab5cb394b7f7e82f6fcc
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.GCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        private ParseDistanceMode _distanceMode = ParseDistanceMode.Absolute;
        public ParseDistanceMode DistanceMode
        {
            get { return _distanceMode; }
            set
            {
                if (_distanceMode == value)
                    return;

                _distanceMode = value;
        
                RaisePropertyChanged();
            }
        }

        private ParseUnit _unit = ParseUnit.Metric;
        public ParseUnit Unit
        {
            get { return _unit; }
            private set
            {
                if (_unit == value)
                    return;
                _unit = value;

                RaisePropertyChanged();
            }
        }

        private ArcPlane _plane = ArcPlane.XY;
        public ArcPlane Plane
        {
            get { return _plane; }
            private set
            {
                if (_plane == value)
                    return;
                _plane = value;

                RaisePropertyChanged();
            }
        }

    }
}
