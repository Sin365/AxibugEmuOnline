using System;

namespace Essgee.Utilities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MachineIndexAttribute : Attribute
    {
        public int Index { get; private set; }

        public MachineIndexAttribute(int index)
        {
            Index = index;
        }
    }
}
