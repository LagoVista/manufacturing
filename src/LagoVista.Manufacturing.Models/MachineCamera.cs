using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using System;
using LagoVista.Core;

namespace LagoVista.Manufacturing.Models
{
    public class MachineCamera : ModelBase
    {
        public MachineCamera()
        {
            Id = Guid.NewGuid().ToId();
        }

        public String Id { get; set; }
        public int CameraIndex { get; set; }
        public String Name { get; set; }

        public Point2D<double> AbsolutePosition { get; set; }

        public double FocusHeight { get; set; }

        private Point2D<double> _tool1Offset;
        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value); }
        }


        public Point2D<double> Tool2Offset { get; set; }
        public Point2D<double> Tool3Offset { get; set; }
    }
}
