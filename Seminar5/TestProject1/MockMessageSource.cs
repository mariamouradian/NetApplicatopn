using Seminar5;
using Seminar5.Abstraction;
using Seminar5.Models;
using System.Net;
using System.Net.Sockets;

namespace TestProject1
{
    public class MockMessageSource : IMessageSource
    {
        private Queue<MessageUdp> messages = new();
        private Server server;
        public IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        private bool isStopped = false;

        public UdpClient UdpClient => throw new NotImplementedException();

        public MockMessageSource()
        {
            messages.Enqueue(new MessageUdp
            {
                Command = Command.Register,
                FromName = "Вася"
            });

            messages.Enqueue(new MessageUdp
            {
                Command = Command.Register,
                FromName = "Юля"
            });

            messages.Enqueue(new MessageUdp
            {
                Command = Command.Message,
                FromName = "Юля",
                ToName = "Вася",
                Text = "От Юли"
            });

            messages.Enqueue(new MessageUdp
            {
                Command = Command.Message,
                FromName = "Вася",
                ToName = "Юля",
                Text = "От Васи"
            });
        }

        public void AddServer(Server srv)
        {
            server = srv;
        }

        public MessageUdp ReceiveMessage(ref IPEndPoint ep)
        {
            if (isStopped || messages.Count == 0)
            {
                return null;
            }

            ep = endPoint;
            return messages.Dequeue();
        }

        public void SendMessage(MessageUdp message, IPEndPoint ep)
        {
            Console.WriteLine($"Mock отправка: {message.Command} от {message.FromName}");
        }

        public void Stop()
        {
            isStopped = true;
        }
    }

    [TestFixture]
    public class ServerTests
    {
        [SetUp]
        public void Setup()
        {
            using (var ctx = new Context())
            {
                ctx.Messages.RemoveRange(ctx.Messages);
                ctx.Users.RemoveRange(ctx.Users);
                ctx.SaveChanges();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            using (var ctx = new Context())
            {
                ctx.Messages.RemoveRange(ctx.Messages);
                ctx.Users.RemoveRange(ctx.Users);
                ctx.SaveChanges();
            }
        }

        [Test]
        public void TestMessageProcessing()
        {
            var mock = new MockMessageSource();
            var srv = new Server(mock);
            mock.AddServer(srv);

            // Запускаем обработку сообщений
            while (mock.ReceiveMessage(ref mock.endPoint) != null) { }

            using (var ctx = new Context())
            {
                // Проверка пользователей
                Assert.That(ctx.Users.Count(), Is.EqualTo(2), "Должно быть 2 пользователя");
                var user1 = ctx.Users.FirstOrDefault(x => x.Name == "Вася");
                var user2 = ctx.Users.FirstOrDefault(x => x.Name == "Юля");
                Assert.IsNotNull(user1, "Пользователь Вася не найден");
                Assert.IsNotNull(user2, "Пользователь Юля не найден");

                // Проверка сообщений
                Assert.That(user1.FromMessages.Count, Is.EqualTo(1), "У Васи должно быть 1 исходящее");
                Assert.That(user2.FromMessages.Count, Is.EqualTo(1), "У Юли должно быть 1 исходящее");
                Assert.That(user1.ToMessages.Count, Is.EqualTo(1), "У Васи должно быть 1 входящее");
                Assert.That(user2.ToMessages.Count, Is.EqualTo(1), "У Юли должно быть 1 входящее");

                // Проверка текста сообщений
                var msgFromVasya = ctx.Messages.FirstOrDefault(x => x.FromUser == user1 && x.ToUser == user2);
                var msgFromYulya = ctx.Messages.FirstOrDefault(x => x.FromUser == user2 && x.ToUser == user1);

                Assert.That(msgFromYulya.Text, Is.EqualTo("От Юли"), "Текст сообщения от Юли не совпадает");
                Assert.That(msgFromVasya.Text, Is.EqualTo("От Васи"), "Текст сообщения от Васи не совпадает");
            }
        }
    }
}