using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Common.Models
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public IPEndPoint Endpoint { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
