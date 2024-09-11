using VirtualNes.Core.Debug;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;
namespace VirtualNes.Core
{
    public class _Mapper : Mapper
    {

        public _Mapper(NES parent) : base(parent) { }

        public override void Reset()
        {
        }
    }
}
