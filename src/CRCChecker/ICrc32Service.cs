using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRCChecker
{
    public interface ICrc32Service
    {
        void Append(System.IO.Stream stream);
        byte[] GetHashAndReset();
        void Reset();
    }
}