using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IPasswordHash
    {
        string Hash(string pass);
        bool Verify(string pass,string passwordHash);
    }
}
