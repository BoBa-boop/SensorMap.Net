using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IAuthorization
    {
        bool IsSuccessAuth { get;}
        string MessageState { get;}
        void ChangePassword(string password);
        void Authorization(string password);
    }

}
