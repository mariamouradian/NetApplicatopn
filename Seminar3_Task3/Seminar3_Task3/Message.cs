using System;
using System.Text.Json;

namespace Seminar3_Task3
{
    public class Message
    {
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Time { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static Message? FromJson(string somemessage) => JsonSerializer.Deserialize<Message>(somemessage);

        public Message (string nikname, string text)
        {
            this.Name = nikname;
            this.Text = text;
            this.Time = DateTime.Now;
        }

        public Message() { }

        public override string ToString() =>
            $"Получено сообщение от {Name} ({Time.ToShortTimeString()}): \n{Text}";
    }
}