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
        private readonly EntityHeader _org = EntityHeader.Create("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", "Organization");
        private readonly EntityHeader _user = EntityHeader.Create("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", "User");
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
            Add("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "0001");
            var target = Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", null);
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
            var response = await _manager.AssignStripFeederIdAsync("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
            _repo.Verify(r => r.GetOwnedStripFeedersAsync(_org.Id), Times.Once);
        }

        [Test]
        public async Task RefusesConflictingExistingIdentity()
        {
            Add("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "00AF");
            var target = Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "00af");
            var response = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task LockContentionNeverWrites()
        {
            var target = Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", null);
            _cache.Setup(c => c.AttemptAcquireLockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>())).ReturnsAsync(false);
            var response = await _manager.AssignStripFeederIdAsync(target.Id, _org, _user);
            Assert.That(response.Successful, Is.False);
            Assert.That(target.FeederId, Is.Null);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task NormalUpdateCannotChangeAnAssignedIdentity()
        {
            Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "1234");
            var update = new StripFeeder { Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Name = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", FeederId = "5678", OwnerOrganization = _org, IsDraft = true };
            var response = await _manager.UpdateStripFeederAsync(update, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public async Task NormalUpdateCannotIntroduceDuplicateId()
        {
            Add("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "00AB");
            Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", null);
            var update = new StripFeeder { Id = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Name = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", FeederId = "00ab", OwnerOrganization = _org, IsDraft = true };
            var response = await _manager.UpdateStripFeederAsync(update, _org, _user);
            Assert.That(response.Successful, Is.False);
            _repo.Verify(r => r.UpdateStripFeederAsync(It.IsAny<StripFeeder>()), Times.Never);
        }

        [Test]
        public void SummaryCarriesIdentityAndOrientation()
        {
            var target = Add("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "000A");
            target.Orientation = EntityHeader<FeederOrientations>.Create(FeederOrientations.Vertical);
            var summary = target.CreateSummary();
            Assert.That(summary.FeederId, Is.EqualTo("000A"));
            Assert.That(summary.Orientation, Is.EqualTo("vertical"));
        }
    }
}
