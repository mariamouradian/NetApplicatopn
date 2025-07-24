using NUnit.Framework;
using Npgsql;
using Seminar5;
using Seminar5.Abstraction;
using Seminar5.Models;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    public class MockMessageSource : IMessageSource, IDisposable
    {
        private readonly Queue<MessageUdp> _messages = new();
        private Server? _server;
        private readonly IPEndPoint _endPoint = new(IPAddress.Any, 0);
        private bool _isStopped = false;
        private readonly UdpClient _mockUdpClient;
        private readonly object _lock = new();

        public UdpClient UdpClient => _mockUdpClient;

        public MockMessageSource()
        {
            _mockUdpClient = new UdpClient(0);
            EnqueueMessage(new MessageUdp { Command = Command.Register, FromName = "Вася" });
            EnqueueMessage(new MessageUdp { Command = Command.Register, FromName = "Юля" });
            EnqueueMessage(new MessageUdp { Command = Command.Message, FromName = "Юля", ToName = "Вася", Text = "От Юли" });
            EnqueueMessage(new MessageUdp { Command = Command.Message, FromName = "Вася", ToName = "Юля", Text = "От Васи" });
        }

        public void EnqueueMessage(MessageUdp message)
        {
            lock (_lock) { _messages.Enqueue(message); }
        }

        public void AddServer(Server server) => _server = server;

        public MessageUdp? ReceiveMessage(ref IPEndPoint? ep)
        {
            if (_isStopped) return null;

            lock (_lock)
            {
                if (_messages.TryDequeue(out var message))
                {
                    ep = _endPoint;
                    return message;
                }
                return null;
            }
        }

        public void SendMessage(MessageUdp message, IPEndPoint ep)
        {
            if (!_isStopped && _server != null && message.Command == Command.Message)
            {
                Task.Run(() => _server.ProcessMessage(message, ep));
            }
        }

        public void Stop() => _isStopped = true;

        public void Dispose()
        {
            Stop();
            _mockUdpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    [TestFixture]
    public class ServerTests
    {
        [SetUp]
        public void Setup()
        {
            try
            {
                CleanDatabase();
            }
            catch (NpgsqlException ex)
            {
                Assert.Inconclusive($"Не удалось подключиться к базе данных: {ex.Message}");
            }
        }

        private static void CleanDatabase()
        {
            using var ctx = CreateContext();
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
        }

        private static Context CreateContext()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseNpgsql("Host=localhost;Port=5432;Database=NetAppSem5;Username=postgres;Password=yourpassword")
                .Options;

            return new Context(options);
        }

        [Test]
        public void TestMessageProcessing()
        {
            try
            {
                using var mock = new MockMessageSource();
                using var srv = new Server(mock);
                mock.AddServer(srv);

                IPEndPoint? ep = new(IPAddress.Any, 0);
                while (mock.ReceiveMessage(ref ep) != null) { }

                VerifyDatabaseState();
            }
            catch (NpgsqlException ex)
            {
                Assert.Inconclusive($"Тест пропущен из-за ошибки базы данных: {ex.Message}");
            }
        }

        private static void VerifyDatabaseState()
        {
            using var ctx = CreateContext();
            var users = ctx.Users
                .Include(u => u.FromMessages)
                .Include(u => u.ToMessages)
                .ToList();

            Assert.That(users, Has.Count.EqualTo(2), "Должно быть 2 пользователя");

            var vasya = users.Find(x => x.Name == "Вася");
            var yulya = users.Find(x => x.Name == "Юля");

            Assert.Multiple(() =>
            {
                Assert.That(vasya, Is.Not.Null, "Пользователь Вася не найден");
                Assert.That(yulya, Is.Not.Null, "Пользователь Юля не найден");

                var vasyaMessages = vasya!.FromMessages.ToList();
                var yulyaMessages = yulya!.FromMessages.ToList();

                Assert.That(vasyaMessages, Has.Count.EqualTo(1), "У Васи должно быть 1 исходящее сообщение");
                Assert.That(yulyaMessages, Has.Count.EqualTo(1), "У Юли должно быть 1 исходящее сообщение");

                Assert.That(vasyaMessages.First().Text, Is.EqualTo("От Васи"), "Текст сообщения от Васи не совпадает");
                Assert.That(yulyaMessages.First().Text, Is.EqualTo("От Юли"), "Текст сообщения от Юли не совпадает");
            });
        }
    }
}