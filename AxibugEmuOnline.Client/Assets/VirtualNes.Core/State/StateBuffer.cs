using System;
using System.Collections.Generic;
using System.IO;
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
        private MemoryStream m_dataStream;
        public StateReader(byte[] bytes)
        {
            m_dataStream = new MemoryStream(bytes);
        }

        public void Skip(uint count)
        {
            m_dataStream.Seek(count, SeekOrigin.Current);
        }
        public void Skip(long count)
        {
            m_dataStream.Seek(count, SeekOrigin.Current);
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
                int byte1 = m_dataStream.ReadByte();
                int byte2 = m_dataStream.ReadByte();

                result[i] = (ushort)(byte1 << 8 | byte2);
            }

            return result;
        }
        public int[] Read_ints(int length)
        {
            int[] result = new int[length];
            for (int i = 0; i < length; i++)
            {
                int byte1 = m_dataStream.ReadByte();
                int byte2 = m_dataStream.ReadByte();
                int byte3 = m_dataStream.ReadByte();
                int byte4 = m_dataStream.ReadByte();

                result[i] = byte1 << 24 | byte2 << 16 | byte3 << 8 | byte4;
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
        public ushort Read_ushort()
        {
            var b1 = Read_byte();
            var b2 = Read_byte();
            return (ushort)(b1 << 8 | b2);
        }

        public int Read_int()
        {
            var b1 = Read_byte();
            var b2 = Read_byte();
            var b3 = Read_byte();
            var b4 = Read_byte();

            return b1 << 24 | b2 << 16 | b3 << 8 | b4;
        }

        public sbyte Read_sbyte(sbyte value)
        {
            return (sbyte)m_dataStream.ReadByte();
        }
        public long Read_long()
        {
            var b1 = Read_byte();
            var b2 = Read_byte();
            var b3 = Read_byte();
            var b4 = Read_byte();
            var b5 = Read_byte();
            var b6 = Read_byte();
            var b7 = Read_byte();
            var b8 = Read_byte();

            return b1 << 56 | b2 << 48 | b3 << 40 | b4 << 32 | b5 << 24 | b6 << 16 | b7 << 8 | b8;
        }

        public uint Read_uint()
        {
            return (uint)Read_int();
        }
    }

    public interface IStateBufferObject
    {
        uint GetSize();
        void SaveState(StateBuffer buffer);
        void LoadState(StateReader buffer);
    }
}
