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
    }
}
