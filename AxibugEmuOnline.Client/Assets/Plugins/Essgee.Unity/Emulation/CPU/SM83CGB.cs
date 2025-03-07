using Essgee.Emulation.Cartridges;
using Essgee.Emulation.Video;
using System;
using UnityEngine.UIElements;

namespace Essgee.Emulation.CPU
{
    public class SM83CGB : SM83
    {
        // TODO: better way of implementing this?
        public bool IsDoubleSpeed { get; private set; }

        public SM83CGB(MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite) : base(memoryRead, memoryWrite) { }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            base.LoadAxiStatus(data);
            IsDoubleSpeed = BitConverter.ToBoolean(data.MemberData[nameof(IsDoubleSpeed)]);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = base.SaveAxiStatus();
            data.MemberData[nameof(IsDoubleSpeed)] = BitConverter.GetBytes(IsDoubleSpeed);
            return data;
        }
        #endregion
        protected override void EnterHaltState()
        {
            if (ime)
            {
                halt = true;
                pc--;
            }
            else
            {
                if ((memoryReadDelegate(0xFF0F) & memoryReadDelegate(0xFFFF) & 0x1F) != 0)
                    currentCycles += 8;
                else
                    halt = true;
            }
        }

        protected override void Stop()
        {
            pc++;

            // Perform speed switch; get IO register value
            var key1 = memoryReadDelegate(0xFF4D);

            // Is speed switch pending?
            if ((key1 & 0b1) != 0)
            {
                // Clear pending flag
                key1 &= 0xFE;

                if (((key1 >> 7) & 0b1) != 0)
                {
                    // Was double speed, now normal speed
                    key1 &= 0x7F;
                    IsDoubleSpeed = false;
                }
                else
                {
                    // Was normal speed, now double speed
                    key1 |= 0x80;
                    IsDoubleSpeed = true;
                }

                // Write register value
                memoryWriteDelegate(0xFF4D, key1);
            }
        }
    }
}
