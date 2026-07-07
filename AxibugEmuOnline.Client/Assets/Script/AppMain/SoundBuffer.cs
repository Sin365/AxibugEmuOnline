using VirtualNes.Core;

public class SoundBuffer : RingBuffer<byte>, ISoundDataBuffer
{
    public SoundBuffer(int capacity) : base(capacity) { }

    public void WriteByte(byte value)
    {
        Write(value);
    }

    public void WriteByte(byte[] input, int offset, int requested)
    {
        Write(input, offset, requested);
    }
}
