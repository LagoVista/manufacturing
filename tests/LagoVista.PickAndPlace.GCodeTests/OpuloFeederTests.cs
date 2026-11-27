// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 37abe04e2ddf0304f7fc1aa6927e509f16726a1d1558718f41ee8078058db4cf
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.LumenSupport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.GCodeTests
{
    public class Tests
    {
        PhotonProtocolHandler _support = new PhotonProtocolHandler();

        private List<string> _commandResponseQUeue ;

        [SetUp]
        public void Setup()
        {
            _commandResponseQUeue = new List<string>();
        }

        [TestCase(0.1, true, 1)]
        [TestCase(4.0, true, 40)]
        [TestCase(25.5, true, 255)]
        [TestCase(0.0, false, 0)]
        [TestCase(-1.0, false, 0)]
        [TestCase(25.6, false, 0)]
        [TestCase(4.01, false, 0)]
        public void DistanceEncoding(double mm, bool valid, int expected)
        {
            Assert.That(PhotonProtocolHandler.TryEncodeDistance(mm, out var encoded), Is.EqualTo(valid));
            Assert.That(encoded, Is.EqualTo(expected));
        }

        [Test]
        public void CorrelationAndValidation()
        {
            var bytes = new List<byte> { 0, 4, 77, 1, 0 };
            bytes.Insert(4, _support.CalcCRC(bytes));
            var frame = string.Concat(bytes.Select(b => b.ToString("X2")));
            var response = _support.ParseResponse(frame);
            Assert.That(PhotonProtocolHandler.MatchesResponse(response, 77, 4), Is.True);
            Assert.That(PhotonProtocolHandler.MatchesResponse(response, 78, 4), Is.False);
            Assert.That(PhotonProtocolHandler.MatchesResponse(response, 77, 5), Is.False);
            response.ToAddress = 1;
            Assert.That(PhotonProtocolHandler.MatchesResponse(response, 77, 4), Is.False);
            Assert.Throws<FormatException>(() => _support.ParseResponse("00044D02"));
            Assert.Throws<FormatException>(() => _support.ParseResponse("GG"));
            Assert.Throws<FormatException>(() => _support.ParseResponse(frame + "00"));
            Assert.Throws<ArgumentException>(() => _support.GenerateGCode(FeederCommands.MoveFeedForward, 4));
        }

        [Test]
        public void Generate()
        {
            _support.packetID = 0x44;
            var pCmd = _support.GenerateGCode(FeederCommands.MoveFeedForward, 1, new byte[1] { 40 });
            Console.WriteLine(pCmd.GCode);
        }

        // Only valid with a powered Opulo feeder reachable on the local
        // 10.1.1.x network. Never run in the canonical headless unit suite.
        [Test]
        [Category("Integration")]
        [Explicit("Requires a live physical Opulo feeder at 10.1.1.246:3000")]
        public async Task Test1()
        {
            var socketClient = new SocketClient();

            await socketClient.ConnectAsync("10.1.1.246", 3000);

            var _cancelSource = new CancellationTokenSource();

            var reader = new StreamReader(socketClient.InputStream);
            var writer = new StreamWriter(socketClient.OutputStream);

            Listen(_cancelSource, reader);

            var pCmd = _support.GenerateGCode(FeederCommands.GetId, 4);
            Console.WriteLine($"--> {pCmd.GCode}");
            await writer.WriteAsync($"{pCmd.GCode}\n");
            await writer.FlushAsync();

            var endAt = DateTime.Now.AddSeconds(1);
            while (DateTime.Now < endAt)
            {
                lock (_commandResponseQUeue)
                {
                    if (_commandResponseQUeue.Any())
                    {
                        foreach (var cmd in _commandResponseQUeue)
                        {
                            var regExp = new Regex("^rs485-reply: (?'payload'[0-9A-F]+)$");
                            var match = regExp.Match(cmd);
                            if (match.Success)
                            {
                                var responsePayload = match.Groups[1].Value;
                                var response = _support.ParseResponse(responsePayload);
                                Console.WriteLine($"<-- +++ {response.TextPayload}");
                            }
                            else
                                Console.WriteLine($"<-- --- {cmd}");
                        }

                        _commandResponseQUeue.Clear();
                    }
                }
                     await Task.Delay(50);                
            }

            _cancelSource.Cancel();
        }

        private async void Listen(CancellationTokenSource token, StreamReader reader)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    var lineTask = reader.ReadLineAsync();

                    while (!lineTask.IsCompleted && !token.IsCancellationRequested)
                    {
                        Task.Delay(50);
                    }

                    if (lineTask.IsCompleted)
                    {
                        if (lineTask.Status != TaskStatus.Faulted)
                        {
                            lock(_commandResponseQUeue)
                                _commandResponseQUeue.Add(lineTask.Result!);
                        }
                    }
                }

            }, token.Token);
        }

        public class SocketClient : ISocketClient
        {
            TcpClient _client;

            public async Task ConnectAsync(string ipAddress, int port)
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ipAddress, port);
                InputStream = _client.GetStream();
                OutputStream = _client.GetStream();
            }

            public Task CloseAsync()
            {
                _client.Close();
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (_client != null)
                    {
                        _client.Dispose();
                    }
                }
            }

            public Stream InputStream { get; private set; }
            public Stream OutputStream { get; private set; }
        }
    }
}