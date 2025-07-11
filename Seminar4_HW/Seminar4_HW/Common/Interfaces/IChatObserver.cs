using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Common.Interfaces
{
    public interface IChatObserver<T>
    {
        void Update(T item);
    }
}
