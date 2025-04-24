
namespace MAME.Core
{
    public class DAC
    {
        public struct dac_info
        {
            public sound_stream channel;
            public short output;
            public short[] UnsignedVolTable;
            public short[] SignedVolTable;
        };
        public static dac_info dac1;
        public unsafe static void DAC_update(int offset, int length)
        {
            ;
            short out1 = dac1.output;
            int i;
            for (i = 0; i < length; i++)
            {
                Sound.dacstream.streamoutput_Ptrs[0][offset + i] = out1;
            }
        }
        public static void dac_signed_data_w(int num, byte data)
        {
            short out1 = dac1.SignedVolTable[data];
            if (dac1.output != out1)
            {
                Sound.dacstream.stream_update();
                dac1.output = out1;
            }
        }
        public static void dac_signed_data_16_w(int num, ushort data)
        {
            short out1 = (short)((uint)data - (uint)0x08000);
            if (dac1.output != out1)
            {
                Sound.dacstream.stream_update();
                dac1.output = out1;
            }
        }
        public static void DAC_build_voltable()
        {
            int i;
            for (i = 0; i < 256; i++)
            {
                dac1.UnsignedVolTable[i] = (short)(i * 0x101 / 2);
                dac1.SignedVolTable[i] = (short)(i * 0x101 - 0x8000);
            }
        }
        public static void dac_start()
        {
            dac1 = new dac_info();
            dac1.UnsignedVolTable = new short[256];
            dac1.SignedVolTable = new short[256];
            DAC_build_voltable();
            dac1.output = 0;
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            writer.Write(DAC.dac1.output);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            dac1.output = reader.ReadInt16();
        }
    }
}
