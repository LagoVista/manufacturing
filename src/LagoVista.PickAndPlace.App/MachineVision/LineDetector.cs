using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class LineDetector
    {
        private readonly ILocatorViewModel _locatorViewModel;
        private readonly List<MVLocatedLine> _foundLines = new List<MVLocatedLine>();

        public LineDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindLines(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;

            if (profile.FindLines)
            {
                var lines = CvInvoke.HoughLinesP(input.Image, profile.HoughLinesRHO, profile.HoughLinesTheta * (Math.PI / 180), profile.HoughLinesThreshold, profile.HoughLinesMinLineLength, profile.HoughLinesMaxLineGap);
                foreach (var line in lines)
                {
                    _foundLines.Add(new MVLocatedLine()
                    {
                        Start = new Point2D<double>(line.P1.X, line.P1.Y),
                        End = new Point2D<double>(line.P2.X, line.P2.Y),
                    });
                }
            }

            _foundLines.Clear();
        }
        public List<MVLocatedLine> FoundLines { get => _foundLines; }

    }
}
