using System;

namespace VirtualNes.Core
{
    public class ByteArrayRef
    {
        private byte[] m_rawArray;
        private int m_offset;
        private int m_length;

        public int Offset
        {
            get => m_offset;
            set
            {
                var gap = value - m_offset;
                m_length -= gap;
            }
        }

        public ByteArrayRef() { }
        public ByteArrayRef(byte[] array, int offset, int length)
        {
            SetArray(array, offset, length);
        }

        public ByteArrayRef(byte[] array) : this(array, 0, array.Length) { }
        public ByteArrayRef(byte[] array, int offset) : this(array, offset, array.Length - offset) { }

        public void SetArray(byte[] array, int offset, int length)
        {
            m_rawArray = array;
            m_offset = offset;
            m_length = length;
        }

        public byte this[int index]
        {
            get
            {
                return m_rawArray[m_offset + index];
            }
            set
            {
                m_rawArray[(m_offset + index)] = value;
            }
        }

        public static implicit operator ByteArrayRef(byte[] array)
        {
            return new ByteArrayRef(array);
        }

        public static implicit operator Span<byte>(ByteArrayRef array)
        {
            return new Span<byte>(array.m_rawArray, array.Offset, array.m_length);
        }
    }
}
