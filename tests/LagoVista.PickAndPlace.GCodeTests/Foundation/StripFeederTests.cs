// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8b8302832607377096935fe178fe31f53c13f94d6bf2a44aba3d4697f183d2d2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StripFeederTests
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

            StagingPlateUtils.ResolveStagePlateLocation(_plate, "B", "3");
        }

        [Test]
        public void StripFeeder()
        {
            
        }
    }
}
