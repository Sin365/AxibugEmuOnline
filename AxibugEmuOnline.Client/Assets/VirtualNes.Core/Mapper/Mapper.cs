using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes.Core
{
    public abstract class Mapper
    {
        internal virtual void Clock(int cycles) { }
    }
}
