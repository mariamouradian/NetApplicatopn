using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Common.Interfaces
{
    public interface IChatSubject<T>
    {
        void RegisterObserver(IChatObserver<T> observer);
        void RemoveObserver(IChatObserver<T> observer);
        void NotifyObservers(T item);
    }
}
