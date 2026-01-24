using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IAuthorization
    {
        string MessageState { get;}
        void ChangePassword(string password);
        bool Authorization(string password);
    }

}
