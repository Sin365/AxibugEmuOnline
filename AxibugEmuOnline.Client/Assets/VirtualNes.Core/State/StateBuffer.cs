using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualNes.Core
{
    public class StateBuffer
    {
        public List<byte> Data = new List<byte>();

        public long Position
        {
            get => Data.Count - 1;
            set
            {
                var gap = value - Position;
                if (gap > 0)
                {
                    Data.AddRange(new byte[gap]);
                }
                else
                {
                    Data.RemoveRange((int)Position, (int)gap);
                }
            }
        }

        public void Write(byte[] bytes)
        {
            Data.AddRange(bytes);
        }
        public void Write(sbyte[] sbytes)
        {
            foreach (var value in sbytes)
            {
                Write(value);
            }
        }
        public void Write(byte value)
        {
            Data.Add(value);
        }
        public void Write(ushort[] values)
        {
            foreach (var value in values)
                Write(value);
        }
        public void Write(int[] values)
        {
            foreach (var value in values)
                Write(value);
        }
        public void Write(string value)
        {
            if (value == null) return;

            Write(Encoding.ASCII.GetBytes(value));
        }
        public void Write(double value)
        {
            Write(BitConverter.GetBytes(value));
        }
        public void Write(ushort value)
        {
            Write(BitConverter.GetBytes(value));
        }
        public void Write(int value)
        {
            Write(BitConverter.GetBytes(value));
        }
        public void Write(sbyte value)
        {
            Write(value);
        }
        public void Write(long value)
        {
            Write(BitConverter.GetBytes(value));
        }
        public void Write(uint value)
        {
            Write(BitConverter.GetBytes(value));
        }
    }
    public class StateReader
    {
        private System.IO.MemoryStream m_dataStream;

        public long Remain => m_dataStream.Length - 1 - m_dataStream.Position;

        public StateReader(byte[] bytes)
        {
            m_dataStream = new System.IO.MemoryStream(bytes);
        }

        public void Skip(uint count)
        {
            m_dataStream.Seek(count, System.IO.SeekOrigin.Current);
        }
        public void Skip(long count)
        {
            m_dataStream.Seek(count, System.IO.SeekOrigin.Current);
        }

        public byte[] Read_bytes(int length)
        {
            var result = new byte[length];
            m_dataStream.Read(result, 0, length);
            return result;
        }
        public sbyte[] Read_sbytes(int length)
        {
            var result = new sbyte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (sbyte)m_dataStream.ReadByte();
            }

            return result;
        }

        public byte Read_byte()
        {
            return (byte)m_dataStream.ReadByte();
        }
        public ushort[] Read_ushorts(int length)
        {
            ushort[] result = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                TEMP[0] = (byte)m_dataStream.ReadByte();
                TEMP[1] = (byte)m_dataStream.ReadByte();

                result[i] = BitConverter.ToUInt16(TEMP, 0);
            }

            return result;
        }
        public int[] Read_ints(int length)
        {
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                TEMP[0] = (byte)m_dataStream.ReadByte();
                TEMP[1] = (byte)m_dataStream.ReadByte();
                TEMP[2] = (byte)m_dataStream.ReadByte();
                TEMP[3] = (byte)m_dataStream.ReadByte();

                result[i] = BitConverter.ToInt32(TEMP, 0);
            }

            return result;
        }


        public string Read_string(int length)
        {
            var result = Read_bytes(length);
            return Encoding.ASCII.GetString(result);
        }
        public double Read_double()
        {
            var result = Read_bytes(4);
            return BitConverter.ToDouble(result, 0);
        }

        byte[] TEMP = new byte[8];

        public ushort Read_ushort()
        {
            TEMP[0] = Read_byte();
            TEMP[1] = Read_byte();
            return BitConverter.ToUInt16(TEMP, 0);
        }

        public int Read_int()
        {
            TEMP[0] = Read_byte();
            TEMP[1] = Read_byte();
            TEMP[2] = Read_byte();
            TEMP[3] = Read_byte();
            return BitConverter.ToInt32(TEMP, 0);
        }

        public sbyte Read_sbyte(sbyte value)
        {
            return (sbyte)m_dataStream.ReadByte();
        }
        public long Read_long()
        {
            TEMP[0] = Read_byte();
            TEMP[1] = Read_byte();
            TEMP[2] = Read_byte();
            TEMP[3] = Read_byte();
            TEMP[4] = Read_byte();
            TEMP[5] = Read_byte();
            TEMP[6] = Read_byte();
            TEMP[7] = Read_byte();

            return BitConverter.ToInt64(TEMP, 0);
        }

        public uint Read_uint()
        {
            return (uint)Read_int();
        }

        public uint[] Read_uints(int length)
        {
            uint[] ret = new uint[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = Read_uint();
            }
            return ret;
        }
    }

    public interface IStateBufferObject
    {
        uint GetSize();
        void SaveState(StateBuffer buffer);
        void LoadState(StateReader buffer);
    }
}
