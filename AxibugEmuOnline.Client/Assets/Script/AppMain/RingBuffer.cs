using System;
using System.Buffers;
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
        if (capacity <= 0) throw new ArgumentException("容量必须大于0", nameof(capacity));

        this.capacity = capacity;
        this.buffer = new T[capacity];
        this.writePos = 0;
        this.readPos = 0;
        this.count = 0;
    }

    #region 单条操作

    public void Write(T item)
    {
        int localWrite;
        do
        {
            localWrite = Volatile.Read(ref writePos);
            int localRead = Volatile.Read(ref readPos);

            if ((localWrite + 1) % capacity == localRead)
            {
                Interlocked.CompareExchange(ref readPos, (localRead + 1) % capacity, localRead);
            }
        }
        while (Interlocked.CompareExchange(ref writePos, (localWrite + 1) % capacity, localWrite) != localWrite);

        buffer[localWrite] = item;
        Interlocked.Increment(ref count);
    }

    public bool TryRead(out T item)
    {
        item = default;
        int localRead;
        do
        {
            localRead = Volatile.Read(ref readPos);
            if (localRead == Volatile.Read(ref writePos))
                return false;
        }
        while (Interlocked.CompareExchange(ref readPos, (localRead + 1) % capacity, localRead) != localRead);

        item = buffer[localRead];
        Interlocked.Decrement(ref count);
        return true;
    }

    #endregion

    #region 新增：获取最后写入的值（不移除）

    /// <summary>
    /// 获取当前环形缓冲区中最后写入的值（不移除数据）
    /// 返回是否成功（缓冲区为空时返回 false）
    /// </summary>
    public bool TryGetLast(out T last)
    {
        last = default;

        int currentCount = Volatile.Read(ref count);
        if (currentCount == 0)
            return false;

        int localWrite = Volatile.Read(ref writePos);
        int lastIndex = (localWrite - 1 + capacity) % capacity;

        last = buffer[lastIndex];
        return true;
    }

    #endregion

    #region 批量操作

    public int Read(T[] output, int offset, int requested)
    {
        if (requested <= 0) return 0;
        int toRead = Math.Min(requested, Volatile.Read(ref count));
        if (toRead == 0) return 0;

        int localRead = Volatile.Read(ref readPos);
        CopyFromBuffer(output, offset, localRead, toRead);

        int newRead = (localRead + toRead) % capacity;
        Interlocked.CompareExchange(ref readPos, newRead, localRead);
        Interlocked.Add(ref count, -toRead);
        return toRead;
    }

    public int Write(T[] input, int offset, int requested)
    {
        if (requested <= 0) return 0;

        int localWrite = Volatile.Read(ref writePos);
        int localRead = Volatile.Read(ref readPos);
        int free = capacity - Volatile.Read(ref count);

        int toWrite = requested;
        if (toWrite > free)
        {
            int overflow = toWrite - free;
            Interlocked.CompareExchange(ref readPos, (localRead + overflow) % capacity, localRead);
        }

        CopyToBuffer(input, offset, localWrite, toWrite);

        int newWrite = (localWrite + toWrite) % capacity;
        Interlocked.CompareExchange(ref writePos, newWrite, localWrite);

        Interlocked.Add(ref count, toWrite);
        if (Volatile.Read(ref count) > capacity)
            Interlocked.Exchange(ref count, capacity);

        return toWrite;
    }

    #endregion

    #region RingBuffer 到 RingBuffer 拷贝

    public int CopyTo(RingBuffer<T> destination, int maxCount = int.MaxValue)
    {
        if (destination == null || maxCount <= 0 || ReferenceEquals(this, destination))
            return 0;

        int srcAvailable = Volatile.Read(ref count);
        int dstFree = destination.capacity - Volatile.Read(ref destination.count);
        int toCopy = Math.Min(maxCount, Math.Min(srcAvailable, dstFree));
        if (toCopy == 0) return 0;

        T[] temp = ArrayPool<T>.Shared.Rent(toCopy);
        try
        {
            int localRead = Volatile.Read(ref readPos);
            CopyFromBuffer(temp, 0, localRead, toCopy);

            int newRead = (localRead + toCopy) % capacity;
            if (Interlocked.CompareExchange(ref readPos, newRead, localRead) != localRead)
                return 0;

            Interlocked.Add(ref count, -toCopy);
            destination.WriteInternal(temp, 0, toCopy);
            return toCopy;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(temp);
        }
    }

    public int PeekCopyTo(RingBuffer<T> destination, int maxCount = int.MaxValue)
    {
        if (destination == null || maxCount <= 0 || ReferenceEquals(this, destination))
            return 0;

        int srcAvailable = Volatile.Read(ref count);
        int dstFree = destination.capacity - Volatile.Read(ref destination.count);
        int toCopy = Math.Min(maxCount, Math.Min(srcAvailable, dstFree));
        if (toCopy == 0) return 0;

        T[] temp = ArrayPool<T>.Shared.Rent(toCopy);
        try
        {
            int localRead = Volatile.Read(ref readPos);
            CopyFromBuffer(temp, 0, localRead, toCopy);
            destination.WriteInternal(temp, 0, toCopy);
            return toCopy;
        }
        finally
        {
            ArrayPool<T>.Shared.Return(temp);
        }
    }

    #endregion

    #region 内部辅助

    private void CopyFromBuffer(T[] dest, int destOffset, int start, int length)
    {
        int first = Math.Min(length, capacity - start);
        Array.Copy(buffer, start, dest, destOffset, first);
        if (length > first)
            Array.Copy(buffer, 0, dest, destOffset + first, length - first);
    }

    private void CopyToBuffer(T[] src, int srcOffset, int start, int length)
    {
        int first = Math.Min(length, capacity - start);
        Array.Copy(src, srcOffset, buffer, start, first);
        if (length > first)
            Array.Copy(src, srcOffset + first, buffer, 0, length - first);
    }

    internal void WriteInternal(T[] src, int srcOffset, int count)
    {
        int localWrite = Volatile.Read(ref writePos);
        int localRead = Volatile.Read(ref readPos);
        int free = capacity - Volatile.Read(ref this.count);

        if (count > free)
        {
            int overflow = count - free;
            Interlocked.CompareExchange(ref readPos, (localRead + overflow) % capacity, localRead);
        }

        int first = Math.Min(count, capacity - localWrite);
        Array.Copy(src, srcOffset, buffer, localWrite, first);
        if (count > first)
            Array.Copy(src, srcOffset + first, buffer, 0, count - first);

        int newWrite = (localWrite + count) % capacity;
        Interlocked.CompareExchange(ref writePos, newWrite, localWrite);

        Interlocked.Add(ref this.count, count);
        if (Volatile.Read(ref this.count) > capacity)
            Interlocked.Exchange(ref this.count, capacity);
    }

    #endregion

    public int Available() => Volatile.Read(ref count);
    public int Free() => capacity - Volatile.Read(ref count);
    public int Capacity => capacity;
}