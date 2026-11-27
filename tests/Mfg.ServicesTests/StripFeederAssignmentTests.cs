using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Managers;
using LagoVista.Manufacturing.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mfg.ServicesTests
{
    public class StripFeederAssignmentTests
    {
        private readonly EntityHeader _org = EntityHeader.Create("ORG1", "Organization");
        private readonly EntityHeader _user = EntityHeader.Create("USER1", "User");
        private Mock<IStripFeederRepo> _repo;
        private Mock<ICacheProvider> _cache;
        private StripFeederManager _manager;
        private List<StripFeeder> _feeders;

        [SetUp]
        public void Setup()
        {
            _feeders = new List<StripFeeder>();
            _repo = new Mock<IStripFeederRepo>();
            _repo.Setup(r => r.GetOwnedStripFeedersAsync(_org.Id)).ReturnsAsync(_feeders);
            _repo.Setup(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>())).Returns(Task.CompletedTask);
            _cache = new Mock<ICacheProvider>();
            _cache.Setup(c => c.AttemptAcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>())).ReturnsAsync(true);
            _cache.Setup(c => c.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _manager = new StripFeederManager(_repo.Object, new Mock<IStripFeederTemplateRepo>().Object,
                new Mock<IComponentManager>().Object, new Mock<IComponentPackageRepo>().Object,
                new Mock<IAdminLogger>().Object, new Mock<IAppConfig>().Object,
                new Mock<IDependencyManager>().Object, new Mock<ISecurity>().Object, _cache.Object);
        }

        private StripFeeder Add(string id, string feederId)
        {
            var feeder = new StripFeeder { Id = id, FeederId = feederId, Name = id, OwnerOrganization = _org, IsDraft = true };
            _feeders.Add(feeder);
            return feeder;
        }

        [Test]
        public async Task AssignsNextIdAndReassignmentIsIdempotent()
        {
            Add("other", "0001");
            var target = Add("target", null);
            var first = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            var second = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            Assert.That(first.Successful && second.Successful, Is.True);
            Assert.That(target.FeederId, Is.EqualTo("0002"));
            Assert.That(second.Result.FeederId, Is.EqualTo("0002"));
            _repo.Verify(r => r.UpdateStripFeederAsync(target), Times.Once);
            _cache.Verify(c => c.ReleaseLockAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task RefusesAnIdOutsideTheOwnedFeederList()
        {
            var response = await _manager.AssignStripFeederIdAsync("other-org", _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
            _repo.Verify(r => r.GetOwnedStripFeedersAsync(_org.Id), Times.Once);
        }

        [Test]
        public async Task RefusesConflictingExistingIdentity()
        {
            Add("other", "00AF");
            var target = Add("target", "00af");
            var response = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task LockContentionNeverWrites()
        {
            var target = Add("target", null);
            _cache.Setup(c => c.AttemptAcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>())).ReturnsAsync(false);
            var response = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            Assert.That(response.Successful, Is.False);
            Assert.That(target.FeederId, Is.Null);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task NormalUpdateCannotChangeAnAssignedIdentity()
        {
            Add("target", "1234");
            var update = new StripFeeder { Id = "target", Name = "target", FeederId = "5678", OwnerOrganization = _org, IsDraft = true };
            var response = await _manager.UpdateStripFeederAsync(update, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task NormalUpdateCannotIntroduceDuplicateId()
        {
            Add("other", "00AB");
            Add("target", null);
            var update = new StripFeeder { Id = "target", Name = "target", FeederId = "00ab", OwnerOrganization = _org, IsDraft = true };
            var response = await _manager.UpdateStripFeederAsync(update, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public void SummaryCarriesIdentityAndOrientation()
        {
            var target = Add("target", "000A");
            target.Orientation = EntityHeader<FeederOrientations>.Create(FeederOrientations.Vertical);
            var summary = target.CreateSummary();
            Assert.That(summary.FeederId, Is.EqualTo("000A"));
            Assert.That(summary.Orientation, Is.EqualTo("vertical"));
        }
    }
}
