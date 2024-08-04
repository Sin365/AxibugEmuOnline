using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;

namespace VirtualNes.Core
{
    public class _MapName : Mapper
    {
		public _MapName(NES parent) : base(parent)
        {
        }

        public override void Reset()
		{
		}

	}
}
