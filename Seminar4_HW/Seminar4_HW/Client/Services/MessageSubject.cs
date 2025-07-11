using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Client.Services
{
    public class MessageSubject : IChatSubject<Message>
    {
        private readonly List<IChatObserver<Message>> _observers = new();

        public void RegisterObserver(IChatObserver<Message> observer) => _observers.Add(observer);
        public void RemoveObserver(IChatObserver<Message> observer) => _observers.Remove(observer);
        public void NotifyObservers(Message message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(message);
            }
        }
    }
}
