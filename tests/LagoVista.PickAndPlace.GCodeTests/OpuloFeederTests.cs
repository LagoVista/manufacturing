using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.LumenSupport;
using System.Net.Sockets;
using System.Text.RegularExpressions;

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

        [Test]
        public void Generate()
        {
            _support.packetID = 0x44;
            var pCmd = _support.GenerateGCode(FeederCommands.MoveFeedForward, 1, new byte[1] { 40 });
            Console.WriteLine(pCmd.GCode);
        }

        [Test]
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