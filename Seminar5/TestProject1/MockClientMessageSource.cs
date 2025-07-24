using Seminar5;
using Seminar5.Abstraction;
using Seminar5.Models;
using System.Net;
using System.Net.Sockets;

namespace TestProject1
{
    public class MockClientMessageSource : IMessageSource
    {
        public Queue<MessageUdp> ExpectedResponses { get; } = new();
        public List<MessageUdp> SentMessages { get; } = new();
        public IPEndPoint LastEndPoint { get; private set; }

        public UdpClient UdpClient => throw new NotImplementedException();

        public MessageUdp ReceiveMessage(ref IPEndPoint ep)
        {
            if (ExpectedResponses.Count == 0)
                return null;

            LastEndPoint = ep;
            return ExpectedResponses.Dequeue();
        }

        public void SendMessage(MessageUdp message, IPEndPoint ep)
        {
            SentMessages.Add(message);
            LastEndPoint = ep;
        }
    }

    [TestFixture]
    public class ClientTests
    {
        private MockClientMessageSource mockSource;
        private IPEndPoint serverEndPoint;
        private Client client;

        [SetUp]
        public void Setup()
        {
            mockSource = new MockClientMessageSource();
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);
            client = new Client(mockSource, serverEndPoint, "TestUser");
        }

        [Test]
        public void Register_SendsCorrectRegistrationMessage()
        {
            // Act
            try
            {
                var clientThread = new Thread(client.Run);
                clientThread.Start();
                Thread.Sleep(200);
                clientThread.Interrupt();
            }
            catch { }

            // Assert
            Assert.That(mockSource.SentMessages.Count, Is.EqualTo(1));
            var msg = mockSource.SentMessages[0];
            Assert.That(msg.Command, Is.EqualTo(Command.Register));
            Assert.That(msg.FromName, Is.EqualTo("TestUser"));
        }

        [Test]
        public void Sender_SendsCorrectMessageStructure()
        {
            // Arrange
            var input = new StringReader("Привет\nЮля");
            Console.SetIn(input);

            // Act
            client.Sender();

            // Assert
            Assert.That(mockSource.SentMessages.Count, Is.EqualTo(1));
            var msg = mockSource.SentMessages[0];
            Assert.That(msg.Text, Is.EqualTo("Привет"));
            Assert.That(msg.ToName, Is.EqualTo("Юля"));
        }

        [Test]
        public void Listener_ProcessesIncomingMessages()
        {
            // Arrange
            mockSource.ExpectedResponses.Enqueue(new MessageUdp
            {
                FromName = "Юля",
                Text = "Привет!",
                Command = Command.Message
            });

            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            client.Listener();

            // Assert
            var consoleOutput = output.ToString();
            Assert.That(consoleOutput, Does.Contain("Привет!"));
            Assert.That(consoleOutput, Does.Contain("Юля"));
        }
    }
}