using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Seminar4_HW.Common.Models
{
    public class Message
    {
        public string FromName { get; set; }
        public string ToName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public IPEndPoint SenderEndpoint { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static Message FromJson(string json) => JsonSerializer.Deserialize<Message>(json);

        public override string ToString() =>
            $"[{Time.ToShortTimeString()}] {FromName} -> {ToName}: {Text}";
    }
}
