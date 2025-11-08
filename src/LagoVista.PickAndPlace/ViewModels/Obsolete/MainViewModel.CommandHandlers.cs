// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 040913439a94141210c2adbafb9df85b787729248d9fae4c73ea3536fd505fc1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PickAndPlace.Models;
using System;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MainViewModel
    {

        public void SetAbsolutePositionMode()
        {
            AssertInManualMode(() => Machine.SendCommand("G90"));
        }

        public void SetIncrementalPositionMode()
        {
            AssertInManualMode(() => Machine.SendCommand("G91"));
        }

        private void ButtonArcPlane_Click()
        {
            if (Machine.Mode != OperatingMode.Manual)
                return;

            if (Machine.Plane != ArcPlane.XY)
                Machine.SendCommand("G17");
        }

        //http://www.cnccookbook.com/CCCNCGCodeG20G21MetricImperialUnitConversion.htm
        public void SetImperialUnits()
        {
            AssertInManualMode(() => Machine.SendCommand("G20"));
        }

        public void SetMetricUnits()
        {
            AssertInManualMode(() => Machine.SendCommand("G21"));
        }



        public void CloseEagleBoardFile()
        {

        }



        public void ProbeHeightMap()
        {

        }


        public void SaveModifiedGCode()
        {

        }
    }
}
