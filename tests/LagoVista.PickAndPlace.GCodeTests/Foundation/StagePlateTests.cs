using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagePlateTests
    {
        MachineStagingPlate _plate;

        [SetUp]
        public void Setup()
        {
            _plate = new Manufacturing.Models.MachineStagingPlate()
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("B", "B", "B"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(100, 100),
            };
        }

        [Test]
        public void ResolveRow()
        {
            var y = StagingPlateUtils.RowToY(_plate, "G");
            Console.WriteLine(y);
            Assert.That(y.Equals(7 * 30 / 2));

            y = StagingPlateUtils.RowToY(_plate, "A");
            Console.WriteLine(y);
            Assert.That(y.Equals(1 * 30 / 2));
        }

        [Test]
        public void ResolveCols()
        {
            var x = StagingPlateUtils.RowToY(_plate, "1");
            Console.WriteLine(x);
            Assert.That(x.Equals(1 * 30 / 2));

            x = StagingPlateUtils.RowToY(_plate, "10");
            Console.WriteLine(x);
            Assert.That(x.Equals(10 * 30 / 2));
        }


        [Test]
        public void ResolveXY() {
            var location = StagingPlateUtils.ResolveStagePlateLocation(_plate, EntityHeader.Create("10", "10", "10"), EntityHeader.Create("B", "B", "B"));
            Console.WriteLine(location); 
            Assert.That(location.X.Equals(150));
            Assert.That(location.Y.Equals(30));
        }

        [Test]
        public void ResolveWorkSpaceLocation()
        {
            var col = EntityHeader.Create("10", "10", "10");
            var row = EntityHeader.Create("B", "B", "B");
            
            Console.WriteLine("Reference Hole: " +  _plate.ReferenceHoleLocation1);
            Console.WriteLine("Target Point  : " + StagingPlateUtils.ResolveStagePlateLocation(_plate, col, row));            
            Console.WriteLine("Reference On Plate:  " + StagingPlateUtils.ResolveStagePlateLocation(_plate, _plate.ReferenceHoleColumn1, _plate.ReferenceHoleRow1));

            var location = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(_plate, col, row);
            Console.WriteLine("Final Location: " + location);
        }
    }
}
