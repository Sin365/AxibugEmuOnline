using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;

namespace VirtualNes.Core
{
	public class _Mapper : Mapper
	{
		public _Mapper(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
		}

	}
}
