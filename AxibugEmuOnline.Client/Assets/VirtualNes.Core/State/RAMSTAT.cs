
namespace VirtualNes.Core
{
    public struct RAMSTAT : IStateBufferObject
    {
        /// <summary> Internal NES RAM </summary>
        public byte[] RAM;
        /// <summary> BG Palette </summary>
        public byte[] BGPAL;
        /// <summary> SP Palette </summary>
        public byte[] SPPAL;
        /// <summary> Sprite RAM </summary>
        public byte[] SPRAM;

        public static RAMSTAT GetDefault()
        {
            var res = new RAMSTAT();
            res.RAM = new byte[2 * 1024];
            res.BGPAL = new byte[16];
            res.SPPAL = new byte[16];
            res.SPRAM = new byte[256];
            return res;
        }

        public readonly uint GetSize()
        {
            return (uint)(RAM.Length + BGPAL.Length + SPPAL.Length + SPRAM.Length);
        }

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(RAM);
            buffer.Write(BGPAL);
            buffer.Write(SPPAL);
            buffer.Write(SPRAM);
        }

        public void LoadState(StateReader buffer)
        {
            RAM = buffer.Read_bytes(2 * 1024);
            BGPAL = buffer.Read_bytes(16);
            SPPAL = buffer.Read_bytes(16);
            SPRAM = buffer.Read_bytes(256);
        }
    }
}
