using System;
using System.Text;
using System.Text.Json;

namespace Chat
{
    internal class Message
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static Message? FromJson(string json) => JsonSerializer.Deserialize<Message>(json);

        public Message(string nikname, string text)
        {
            Name = nikname;
            Text = text;
            Time = DateTime.Now;
        }

        public Message() { }

        public override string ToString() =>
            $"Получено сообщение от {Name} ({Time.ToShortTimeString()}): \n{Text}";
    }
}