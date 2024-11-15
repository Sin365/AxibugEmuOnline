using System;

namespace VirtualNes.Core
{
    public class ArrayRef<T>
    {
        public T[] RawArray => m_rawArray;
        private T[] m_rawArray;
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

        public ArrayRef() { }
        public ArrayRef(T[] array, int offset, int length)
        {
            SetArray(array, offset, length);
        }

        public ArrayRef(T[] array) : this(array, 0, array.Length) { }
        public ArrayRef(T[] array, int offset) : this(array, offset, array.Length - offset) { }

        public void SetArray(T[] array, int offset, int length)
        {
            m_rawArray = array;
            m_offset = offset;
            m_length = length;
        }

        public void SetArray(T[] array, int offset)
        {
            m_rawArray = array;
            m_offset = offset;
            m_length = array.Length - offset;
        }

        public T this[int index]
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

        public void WriteTo(T[] source, int start, int length)
        {
            Array.Copy(source, 0, m_rawArray, Offset + start, length);
        }

        public Span<T> Span(int start, int length)
        {
            return new Span<T>(m_rawArray, start + Offset, length);
        }

        public static implicit operator ArrayRef<T>(T[] array)
        {
            return new ArrayRef<T>(array);
        }

        public static implicit operator Span<T>(ArrayRef<T> array)
        {
            return new Span<T>(array.m_rawArray, array.Offset, array.m_length);
        }
    }
}
