using NUnit.Framework;
using Seminar5.Abstraction;
using Seminar5.Models;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Seminar5;

namespace TestProject1
{
    public class MockClientMessageSource : IMessageSource, IDisposable
    {
        private readonly Queue<MessageUdp> _expectedResponses = new();
        private readonly List<MessageUdp> _sentMessages = new();
        private IPEndPoint? _lastEndPoint;
        private readonly UdpClient _udpClient;
        private bool _disposed;

        public UdpClient UdpClient => _udpClient;
        public List<MessageUdp> SentMessages => _sentMessages;
        public Queue<MessageUdp> ExpectedResponses => _expectedResponses;

        public MockClientMessageSource()
        {
            _udpClient = new UdpClient(0);
        }

        public void EnqueueResponse(MessageUdp message)
        {
            _expectedResponses.Enqueue(message);
        }

        public MessageUdp? ReceiveMessage(ref IPEndPoint? ep)
        {
            if (_disposed || _expectedResponses.Count == 0)
                return null;

            _lastEndPoint = ep;
            return _expectedResponses.Dequeue();
        }

        public void SendMessage(MessageUdp message, IPEndPoint ep)
        {
            if (!_disposed)
            {
                lock (_sentMessages)
                {
                    _sentMessages.Add(message);
                }
                _lastEndPoint = ep;
                Console.WriteLine($"Mock: Сообщение '{message.Text}' для {message.ToName} отправлено");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _udpClient.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }

    [TestFixture]
    public class ClientTests : IDisposable
    {
        private MockClientMessageSource _mockSource = null!;
        private IPEndPoint _serverEndPoint = null!;
        private Client _client = null!;
        private readonly TextWriter _originalOutput = Console.Out;
        private readonly TextReader _originalInput = Console.In;

        [SetUp]
        public void Setup()
        {
            _mockSource = new MockClientMessageSource();
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);
            _client = new Client(_mockSource, _serverEndPoint, "TestUser");
        }

        [TearDown]
        public void Cleanup()
        {
            _client.Stop();
        }

        public void Dispose()
        {
            _client.Dispose();
            _mockSource.Dispose();
            Console.SetOut(_originalOutput);
            Console.SetIn(_originalInput);
            GC.SuppressFinalize(this);
        }

        [Test]
        public void Register_SendsCorrectRegistrationMessage()
        {
            using var client = new Client(_mockSource, _serverEndPoint, "TestUser");
            var clientThread = new Thread(client.Run);
            clientThread.Start();

            Thread.Sleep(500);
            client.Stop();
            clientThread.Join(500);

            Assert.That(_mockSource.SentMessages, Has.Count.EqualTo(1));
            var msg = _mockSource.SentMessages[0];
            Assert.Multiple(() =>
            {
                Assert.That(msg.Command, Is.EqualTo(Command.Register));
                Assert.That(msg.FromName, Is.EqualTo("TestUser"));
            });
        }

        [Test]
        [Timeout(5000)]
        public void Sender_SendsCorrectMessageStructure()
        {
            // Arrange
            using var input = new StringReader("Привет\nЮля\n");
            Console.SetIn(input);

            // Запускаем клиент для регистрации
            var clientThread = new Thread(_client.Run);
            clientThread.Start();
            Thread.Sleep(500); // Даем время на регистрацию

            // Act
            var senderThread = new Thread(() => {
                try
                {
                    _client.Sender();
                }
                catch (OperationCanceledException)
                {
                    // Игнорируем ожидаемое исключение при остановке
                }
            });
            senderThread.Start();

            // Даем время на обработку ввода
            Thread.Sleep(1000);
            _client.Stop();
            senderThread.Join(1000);
            clientThread.Join(1000);

            // Debug output
            Console.WriteLine($"Отправлено сообщений: {_mockSource.SentMessages.Count}");
            foreach (var msg in _mockSource.SentMessages)
            {
                Console.WriteLine($"Сообщение: {msg.Text}, Кому: {msg.ToName}");
            }

            // Assert
            Assert.That(_mockSource.SentMessages, Has.Count.EqualTo(2),
                $"Ожидалось 2 сообщения (регистрация + текст), но получено {_mockSource.SentMessages.Count}");

            // Первое сообщение - регистрация
            var registerMessage = _mockSource.SentMessages[0];
            Assert.Multiple(() =>
            {
                Assert.That(registerMessage.Command, Is.EqualTo(Command.Register));
                Assert.That(registerMessage.FromName, Is.EqualTo("TestUser"));
            });

            // Второе сообщение - текст
            var textMessage = _mockSource.SentMessages[1];
            Assert.Multiple(() =>
            {
                Assert.That(textMessage.Text, Is.EqualTo("Привет"));
                Assert.That(textMessage.ToName, Is.EqualTo("Юля"));
            });
        }

        [Test]
        [Timeout(3000)]
        public void Listener_ProcessesIncomingMessages()
        {
            _mockSource.ExpectedResponses.Enqueue(new MessageUdp
            {
                FromName = "Юля",
                Text = "Привет!",
                Command = Command.Message
            });

            using var output = new StringWriter();
            Console.SetOut(output);

            var listenerThread = new Thread(_client.Listener);
            listenerThread.Start();

            try
            {
                bool received = SpinWait.SpinUntil(() =>
                    output.ToString().Contains("Привет!"),
                    TimeSpan.FromSeconds(2));

                Assert.IsTrue(received, "Сообщение не было получено");
                Assert.That(output.ToString(), Does.Contain("Юля"));
            }
            finally
            {
                _client.Stop();
                listenerThread.Join(1000);
            }
        }
    }
}