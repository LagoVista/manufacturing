using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Managers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfg.ServicesTests
{
    public class FeederTests
    {
        StripFeederManager _mgr;
        Mock<IStripFeederTemplateRepo> _templateRepo = new Mock<IStripFeederTemplateRepo>();

        private EntityHeader _user = EntityHeader.Create("123456789", "User");
        private EntityHeader _org = EntityHeader.Create("8934234", "Org");

        [SetUp]
        public void Setup()
        {
            _mgr = new StripFeederManager(new Mock<IStripFeederRepo>().Object, _templateRepo.Object, new Mock<IComponentManager>().Object, new Mock<IComponentPackageRepo>().Object, new Mock<IAdminLogger>().Object,
                    new Mock<IAppConfig>().Object, new Mock<IDependencyManager>().Object, new Mock<ISecurity>().Object);
        }

        [Test]
        public async Task CreateFeederFromTemplate()
        {
            _templateRepo.Setup(tr => tr.GetStripFeederTemplateAsync(It.IsAny<string>())).ReturnsAsync(new LagoVista.Manufacturing.Models.StripFeederTemplate
            {
                Name = "Template",
                Key = "key123",
                RowCount = 3,
                RowWidth = 15,
                Width = 40,
                Length = 120,
                Height = 12,
                TapeSize = EntityHeader<LagoVista.Manufacturing.Models.TapeSizes>.Create(LagoVista.Manufacturing.Models.TapeSizes.EightMM)
            });


            var result = await _mgr.CreateFromTemplateAsync("don'tcare", _user, _org);
            Console.WriteLine(result.MountingHoleOffset);
            foreach (var row in result.Rows)
            {
                Console.WriteLine("FIRST: " + row.FirstTapeHoleOffset);
                Console.WriteLine("Last: " + row.LastTapeHoleOffset);
            }


         }

    }
}
