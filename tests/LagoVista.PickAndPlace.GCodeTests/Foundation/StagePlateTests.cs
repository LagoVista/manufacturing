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
        StagingPlateUtils _utils;
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

            _utils = new StagingPlateUtils(_plate);
        }

        [Test]
        public void ResolveRow()
        {
            var y = _utils.RowToY(EntityHeader.Create("G", "G", "G"));
            Console.WriteLine(y);
            Assert.AreEqual((7 * 30 / 2), y);

            y = _utils.RowToY(EntityHeader.Create("A", "A", "A"));
            Console.WriteLine(y);
            Assert.AreEqual((1 * 30 / 2), y);
        }

        [Test]
        public void ResolveCols()
        {
            var x = _utils.ColToX(EntityHeader.Create("1", "1", "1"));
            Console.WriteLine(x);
            Assert.AreEqual((1 * 30 / 2), x);

            x = _utils.ColToX(EntityHeader.Create("10", "10", "10"));
            Console.WriteLine(x);
            Assert.AreEqual((10 * 30 / 2), x);
        }


        [Test]
        public void ResolveXY() {
            var location = _utils.ResolveStagePlateLocation(EntityHeader.Create("10", "10", "10"), EntityHeader.Create("B", "B", "B"));
            Console.WriteLine(location); 
            Assert.AreEqual(150, location.X);
            Assert.AreEqual(30, location.Y);
        }

        [Test]
        public void ResolveWorkSpaceLocation()
        {
            var col = EntityHeader.Create("10", "10", "10");
            var row = EntityHeader.Create("B", "B", "B");
            
            Console.WriteLine("Reference Hole: " +  _plate.ReferenceHoleLocation1);
            Console.WriteLine("Target Point  : " + _utils.ResolveStagePlateLocation(col, row));            
            Console.WriteLine("Reference On Plate:  " + _utils.ResolveStagePlateLocation(_plate.ReferenceHoleColumn1, _plate.ReferenceHoleRow1));

            var location = _utils.ResolveStagePlateWorkSpakeLocation(col, row);
            Console.WriteLine("Final Location: " + location);
        }
    }
}
