namespace VirtualNes.Core
{
    public class ByteArrayRef
    {
        private byte[] m_rawArray;
        private int m_offset;
        private int m_length;

        public ByteArrayRef(byte[] array, int offset, int length)
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
