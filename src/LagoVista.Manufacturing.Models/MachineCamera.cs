using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using System;
using LagoVista.Core;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartLayers;

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

        private bool _mirrorYAxis;
        public bool MirrorYAxis 
        { 
            get { return _mirrorYAxis; }
            set { Set(ref _mirrorYAxis, value); }
        }


        private bool _mirrorXAxis;
        public bool MirrorXAxis 
        {
            get => _mirrorXAxis;
            set {  Set(ref _mirrorXAxis, value); }
        }


        private Point2D<double> _tool2Offset;
        public Point2D<double> Tool2Offset
        {
            get { return _tool2Offset; }
            set { Set(ref _tool2Offset, value); }
        }

        public Point2D<double> Tool3Offset { get; set; }
    }
}
