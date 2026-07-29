using System;
using System.Collections.Generic;
using System.IO;

public class PipeStream : Stream
{
    private readonly Queue<byte> _buffer = new Queue<byte>();
    private long _maxBufferLength = 8192*4;

    public long MaxBufferLength
    {
        get { return _maxBufferLength; }
        set { _maxBufferLength = value; }
    }

    public new void Dispose()
    {
        _buffer.Clear();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (offset != 0)
            throw new NotImplementedException("Offsets with value of non-zero are not supported");
        if (buffer == null)
            throw new ArgumentException("Buffer is null");
        if (offset + count > buffer.Length)
            throw new ArgumentException("The sum of offset and count is greater than the buffer length. ");
        if (offset < 0 || count < 0)
            throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");

        if (count == 0)
            return 0;

        int readLength = 0;

        lock (_buffer)
        {
            // fill the read buffer
            for (; readLength < count && Length > 0; readLength++)
            {
                buffer[readLength] = _buffer.Dequeue();
            }
        }

        return readLength;
    }

    private bool ReadAvailable(int count)
    {
        return (Length >= count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
            throw new ArgumentException("Buffer is null");
        if (offset + count > buffer.Length)
            throw new ArgumentException("The sum of offset and count is greater than the buffer length. ");
        if (offset < 0 || count < 0)
            throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
        if (count == 0)
            return;

        lock (_buffer)
        {
            while (Length >= _maxBufferLength)
                return;

            // queue up the buffer data
            foreach (byte b in buffer)
            {
                _buffer.Enqueue(b);
            }
        }
    }

    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanSeek
    {
        get { return false; }
    }

    public override bool CanWrite
    {
        get { return true; }
    }

    public override long Length
    {
        get { return _buffer.Count; }
    }

    public override long Position
    {
        get { return 0; }
        set { throw new NotImplementedException(); }
    }
}
