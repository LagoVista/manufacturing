using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.LumenSupport;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.GCodeTests
{
    public class PhotonFeederTransactionTests
    {
        private const byte Slot = 4;
        private const string Id = "00112233445566778899AABB";
        private Mock<IMachine> _machine;
        private PhotonFeederViewModel _vm;
        private PhotonProtocolHandler _protocol;
        private EventHandler<string> _handlers;
        private List<FeederCommands> _sent;
        private Action<byte[]> _respond;
        private bool _connected;

        [SetUp]
        public void Setup()
        {
            _connected = true;
            _handlers = null;
            _sent = new List<FeederCommands>();
            _protocol = new PhotonProtocolHandler();
            _machine = new Mock<IMachine>();
            _machine.SetupGet(m => m.Connected).Returns(() => _connected);
            _machine.SetupAdd(m => m.LineReceived += It.IsAny<EventHandler<string>>())
                .Callback<EventHandler<string>>(h => _handlers += h);
            _machine.SetupRemove(m => m.LineReceived -= It.IsAny<EventHandler<string>>())
                .Callback<EventHandler<string>>(h => _handlers -= h);
            _machine.Setup(m => m.SendCommand(It.IsAny<string>())).Callback<string>(gcode =>
            {
                var request = PhotonProtocolHandler.GetByteArray(gcode.Substring(5)).ToArray();
                _sent.Add((FeederCommands)request[5]);
                _respond(request);
            });
            var repo = new Mock<IMachineRepo>();
            repo.SetupGet(r => r.CurrentMachine).Returns(_machine.Object);
            _vm = new PhotonFeederViewModel(repo.Object, _protocol, null, null);
            _respond = RespondNormally;
        }

        private void Emit(byte[] request, byte status, byte[] payload = null, byte? packet = null, byte? from = null)
        {
            payload = payload ?? Array.Empty<byte>();
            var bytes = new List<byte> { 0, from ?? Slot, packet ?? request[2], (byte)(payload.Length + 1), status };
            bytes.AddRange(payload);
            bytes.Insert(4, _protocol.CalcCRC(bytes));
            _handlers?.Invoke(_machine.Object, "rs485-reply: " + string.Concat(bytes.Select(b => b.ToString("X2"))));
        }

        private void RespondNormally(byte[] request)
        {
            switch ((FeederCommands)request[5])
            {
                case FeederCommands.GetId:
                case FeederCommands.Initialize:
                    Emit(request, 0, PhotonProtocolHandler.GetByteArray(Id).ToArray());
                    break;
                case FeederCommands.MoveFeedForward:
                case FeederCommands.MoveFeedBackward:
                    Emit(request, 0, new byte[] { 0, 0 });
                    break;
                case FeederCommands.MoveFeedStatus:
                    Emit(request, 0);
                    break;
            }
        }

        [Test]
        public async Task ImmediateRepliesAndFeedStatusConfirmForwardAndReverse()
        {
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.True);
            Assert.That((await _vm.AdvanceFeed(Slot, 4, Id)).Successful, Is.True);
            Assert.That((await _vm.RetractFeed(Slot, 2, Id)).Successful, Is.True);
            Assert.That(_sent, Is.EqualTo(new[] { FeederCommands.GetId, FeederCommands.Initialize,
                FeederCommands.GetId, FeederCommands.MoveFeedForward, FeederCommands.MoveFeedStatus,
                FeederCommands.GetId, FeederCommands.MoveFeedBackward, FeederCommands.MoveFeedStatus }));
            Assert.That(_handlers, Is.Null);
            Assert.That(_vm.IsBusBusy, Is.False);
        }

        [Test]
        public async Task WrongPacketAndWrongSlotCannotCompleteInitialization()
        {
            _respond = request =>
            {
                if ((FeederCommands)request[5] == FeederCommands.GetId) RespondNormally(request);
                else
                {
                    var uuid = PhotonProtocolHandler.GetByteArray(Id).ToArray();
                    Emit(request, 0, uuid, (byte)(request[2] + 1));
                    Emit(request, 0, uuid, from: (byte)(Slot + 1));
                    Emit(request, 1, uuid);
                }
            };
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.False);
            Assert.That(_vm.IsInitialized(Slot, Id), Is.False);
            Assert.That(_handlers, Is.Null);
        }

        [Test]
        public async Task InitializationTimeoutIsBoundedAndUnsubscribes()
        {
            _respond = request => _handlers?.Invoke(_machine.Object, "rs485-reply: timeout");
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.False);
            Assert.That(_sent, Is.EqualTo(new[] { FeederCommands.GetId, FeederCommands.GetId, FeederCommands.GetId }));
            Assert.That(_handlers, Is.Null);
            Assert.That(_vm.IsBusBusy, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task AmbiguousPhysicalMoveIsNeverAutomaticallyResent(bool reverse)
        {
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.True);
            _respond = request =>
            {
                if ((FeederCommands)request[5] == FeederCommands.GetId) RespondNormally(request);
                else _handlers?.Invoke(_machine.Object, "rs485-reply: timeout");
            };
            var result = reverse ? await _vm.RetractFeed(Slot, 4, Id) : await _vm.AdvanceFeed(Slot, 4, Id);
            Assert.That(result.Successful, Is.False);
            Assert.That(_vm.IsInitialized(Slot, Id), Is.False);
            Assert.That((await _vm.AdvanceFeed(Slot, 4, Id)).Successful, Is.False);
            Assert.That(_sent.Count(c => c == FeederCommands.MoveFeedForward || c == FeederCommands.MoveFeedBackward), Is.EqualTo(1));
            Assert.That(_handlers, Is.Null);
        }

        [Test]
        public async Task NegativeFeedStatusDoesNotConfirmMotion()
        {
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.True);
            _respond = request =>
            {
                if ((FeederCommands)request[5] == FeederCommands.MoveFeedStatus) Emit(request, 5);
                else RespondNormally(request);
            };
            Assert.That((await _vm.AdvanceFeed(Slot, 4, Id)).Successful, Is.False);
            Assert.That(_vm.IsInitialized(Slot, Id), Is.False);
            Assert.That(_handlers, Is.Null);
        }

        [Test]
        public async Task ConcurrentRequestRejectedAndDisconnectCleansUp()
        {
            byte[] pending = null;
            _respond = request => pending = request;
            var first = _vm.InitializeFeederAsync(Slot, Id);
            Assert.That(_vm.IsBusBusy, Is.True);
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.False);
            Assert.That(_sent.Count, Is.EqualTo(1));
            Assert.That(pending, Is.Not.Null);
            _connected = false;
            Assert.That((await first).Successful, Is.False);
            Assert.That(_handlers, Is.Null);
            Assert.That(_vm.IsBusBusy, Is.False);
        }

        [Test]
        public async Task InvalidIdentitySlotAndDistanceNeverSend()
        {
            Assert.That((await _vm.InitializeFeederAsync(0, Id)).Successful, Is.False);
            Assert.That((await _vm.InitializeFeederAsync(255, Id)).Successful, Is.False);
            Assert.That((await _vm.InitializeFeederAsync(Slot, "bad")).Successful, Is.False);
            Assert.That((await _vm.AdvanceFeed(Slot, 0, Id)).Successful, Is.False);
            Assert.That(_sent, Is.Empty);
        }

        [Test]
        public async Task MalformedFrameCannotInitialize()
        {
            _respond = request => _handlers?.Invoke(_machine.Object, "rs485-reply: NOTHEX");
            Assert.That((await _vm.InitializeFeederAsync(Slot, Id)).Successful, Is.False);
            Assert.That(_vm.IsInitialized(Slot, Id), Is.False);
            Assert.That(_handlers, Is.Null);
        }
    }
}
