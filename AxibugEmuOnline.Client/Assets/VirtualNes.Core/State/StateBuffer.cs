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
            foreach(var value in sbytes)
            {
                Write(value);
            }
        }
        public void Write(byte[] bytes, int length)
        {
            Data.AddRange(bytes);
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
    }

    public interface IStateBufferObject
    {
        uint GetSize();
        void SaveState(StateBuffer buffer);
    }
}
