﻿namespace VirtualNes.Core
{
    public struct MMCSTAT : IStateBufferObject
    {
        public byte[] mmcdata;

        public static MMCSTAT GetDefault()
        {
            return new MMCSTAT() { mmcdata = new byte[256] };
        }

        public readonly uint GetSize()
        {
            return 256;
        }

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(mmcdata);
        }

        public void LoadState(StateReader buffer)
        {
            mmcdata = buffer.Read_bytes(256);
        }
    }
}
