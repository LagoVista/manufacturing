// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e85144b49fd0d49de9af65e4e392886a9e42efa99fb276285d6a137b4fd127e0
// IndexVersion: 0
// --- END CODE INDEX META ---
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class LineDetector : ILineDetector<IInputOutputArray>
    {
        private readonly ILocatorViewModel _locatorViewModel;
        private readonly ObservableCollection<MVLocatedLine> _foundLines = new ObservableCollection<MVLocatedLine>();

        public LineDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindLines(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;

            if (profile.FindLines)
            {
                var lines = CvInvoke.HoughLinesP(input.Image, profile.HoughLinesRHO, profile.HoughLinesTheta * (Math.PI / 180), profile.HoughLinesThreshold, profile.HoughLinesMinLineLength / profile.PixelsPerMM, profile.HoughLinesMaxLineGap);
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

        public void Reset()
        {
            _foundLines.Clear();
        }

        public ObservableCollection<MVLocatedLine> FoundLines { get => _foundLines; }

    }
}
