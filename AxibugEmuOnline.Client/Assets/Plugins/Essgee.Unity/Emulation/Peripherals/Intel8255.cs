using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Linq;

namespace Essgee.Emulation.Peripherals
{
    public class Intel8255 : IPeripheral
    {
        [StateRequired]
        public byte PortAInput { get; set; }
        [StateRequired]
        public byte PortBInput { get; set; }
        [StateRequired]
        public byte PortCInput { get; set; }
        [StateRequired]
        public byte PortAOutput { get; set; }
        [StateRequired]
        public byte PortBOutput { get; set; }
        [StateRequired]
        public byte PortCOutput { get; set; }

        [StateRequired]
        byte configByte, setResetControlByte;

        int operatingModeGroupA => ((configByte >> 5) & 0x03);
        bool isPortAInput => ((configByte & 0x10) == 0x10);
        bool isPortCUInput => ((configByte & 0x08) == 0x08);
        int operatingModeGroupB => ((configByte >> 2) & 0x01);
        bool isPortBInput => ((configByte & 0x02) == 0x02);
        bool isPortCLInput => ((configByte & 0x01) == 0x01);
        int bitToChange => ((setResetControlByte >> 1) & 0x07);
        bool isSetBitOperation => ((setResetControlByte & 0x01) == 0x01);

        public Intel8255() { }


        #region AxiState

        public virtual void LoadAxiStatus(AxiEssgssStatusData data)
        {
            PortAInput = data.MemberData[nameof(PortAInput)].First();
            PortBInput = data.MemberData[nameof(PortBInput)].First();
            PortCInput = data.MemberData[nameof(PortCInput)].First();
            PortAOutput = data.MemberData[nameof(PortAOutput)].First();
            PortBOutput = data.MemberData[nameof(PortBOutput)].First();
            PortCOutput = data.MemberData[nameof(PortCOutput)].First();
            configByte = data.MemberData[nameof(configByte)].First();
            setResetControlByte = data.MemberData[nameof(setResetControlByte)].First();
        }

        public virtual AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            PortAInput = data.MemberData[nameof(PortAInput)].First();
            PortBInput = data.MemberData[nameof(PortBInput)].First();
            PortCInput = data.MemberData[nameof(PortCInput)].First();
            data.MemberData[nameof(PortAInput)] = BitConverter.GetBytes(PortAInput);
            data.MemberData[nameof(PortBInput)] = BitConverter.GetBytes(PortBInput);
            data.MemberData[nameof(PortCInput)] = BitConverter.GetBytes(PortCInput);
            data.MemberData[nameof(PortAOutput)] = BitConverter.GetBytes(PortAOutput);
            data.MemberData[nameof(PortBOutput)] = BitConverter.GetBytes(PortBOutput);
            data.MemberData[nameof(PortCOutput)] = BitConverter.GetBytes(PortCOutput);
            data.MemberData[nameof(configByte)] = BitConverter.GetBytes(configByte);
            data.MemberData[nameof(setResetControlByte)] = BitConverter.GetBytes(setResetControlByte);
            return data;
        }
        #endregion

        public void Startup()
        {
            //
        }

        public void Shutdown()
        {
            //
        }

        public void Reset()
        {
            PortAInput = PortAOutput = 0x00;
            PortBInput = PortBOutput = 0x00;
            PortCInput = PortCOutput = 0x00;

            WritePort(0x03, 0x9B);
        }

        public byte ReadPort(byte port)
        {
            switch (port & 0x03)
            {
                case 0x00: return (isPortAInput ? PortAInput : PortAOutput);
                case 0x01: return (isPortBInput ? PortBInput : PortBOutput);
                case 0x02: return (byte)(((isPortCUInput ? PortCInput : PortCOutput) & 0xF0) | (isPortCLInput ? PortCInput : PortCOutput) & 0x0F);
                case 0x03: return 0xFF; /* Cannot read control port */

                default: throw new EmulationException(string.Format("i8255: Unsupported read from port 0x{0:X2}", port));
            }
        }

        public void WritePort(byte port, byte value)
        {
            switch (port & 0x03)
            {
                case 0x00: PortAOutput = value; break;
                case 0x01: PortBOutput = value; break;
                case 0x02: PortCOutput = value; break;

                case 0x03:
                    /* Control port */
                    if ((value & 0x80) == 0x80)
                    {
                        configByte = value;
                    }
                    else
                    {
                        setResetControlByte = value;

                        byte mask = (byte)(1 << bitToChange);
                        if (isSetBitOperation) PortCOutput |= mask;
                        else PortCOutput &= (byte)~mask;
                    }
                    break;

                default: throw new EmulationException(string.Format("i8255: Unsupported write to port 0x{0:X2}, value 0x{1:X2}", port, value));
            }
        }
    }
}
