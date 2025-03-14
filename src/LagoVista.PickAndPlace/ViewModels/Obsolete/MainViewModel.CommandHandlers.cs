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
