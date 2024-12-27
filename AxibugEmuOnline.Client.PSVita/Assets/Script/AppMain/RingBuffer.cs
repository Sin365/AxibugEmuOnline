using System.Threading;

public class RingBuffer<T>
{
    private readonly T[] buffer;
    private readonly int capacity;
    private int writePos;
    private int readPos;
    private int count;

    public RingBuffer(int capacity)
    {
        this.capacity = capacity;
        this.buffer = new T[capacity];
        this.writePos = 0;
        this.readPos = 0;
        this.count = 0;
    }

    public void Write(T item)
    {
        int localWritePos;
        int localReadPos;

        do
        {
            localWritePos = Volatile.Read(ref writePos);
            localReadPos = Volatile.Read(ref readPos);

            int nextWritePos = (localWritePos + 1) % capacity;

            if (nextWritePos == localReadPos)
            {
                // 缓冲区已满，覆盖最旧的未读数据
                Interlocked.CompareExchange(ref readPos, (localReadPos + 1) % capacity, localReadPos);
            }
        }
        while (Interlocked.CompareExchange(ref writePos, (localWritePos + 1) % capacity, localWritePos) != localWritePos);

        buffer[localWritePos] = item;
        Interlocked.Increment(ref count);
    }

    public bool TryRead(out T item)
    {
        item = default(T);

        int localReadPos;
        int localWritePos;

        do
        {
            localReadPos = Volatile.Read(ref readPos);
            localWritePos = Volatile.Read(ref writePos);

            if (localReadPos == localWritePos)
            {
                return false; // 缓冲区为空
            }
        }
        while (Interlocked.CompareExchange(ref readPos, (localReadPos + 1) % capacity, localReadPos) != localReadPos);

        item = buffer[localReadPos];
        Interlocked.Decrement(ref count);
        return true;
    }

    public int Available()
    {
        return Volatile.Read(ref count);
    }
}
