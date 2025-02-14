using Essgee.EventArguments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Essgee.Emulation.ExtDevices.Nintendo
{
    [Description("Game Boy Printer")]
    //todo Unity [ElementPriority(2)]
    public class GBPrinter : ISerialDevice
    {
        readonly Color[] defaultPalette = new Color[]
        {
            Color.FromArgb(0xF8, 0xF8, 0xF8),
            Color.FromArgb(0x9B, 0x9B, 0x9B),
            Color.FromArgb(0x3E, 0x3E, 0x3E),
            Color.FromArgb(0x1F, 0x1F, 0x1F)
        };

        enum PrinterPacketBytes
        {
            MagicLSB,
            MagicMSB,
            CommandByte,
            CompressionFlag,
            DataLengthLSB,
            DataLengthMSB,
            DataByte,
            ChecksumLSB,
            ChecksumMSB,
            Execute
        }

        enum PrinterCommands : byte
        {
            Initialize = 0x01,
            StartPrinting = 0x02,
            Unknown = 0x03,
            ImageTransfer = 0x04,
            ReadStatus = 0x0F
        }

        [Flags]
        enum PrinterPresenceBits : byte
        {
            Unknown = (1 << 0),
            Present = (1 << 7),
        }

        [Flags]
        enum PrinterStatusBits : byte
        {
            BadChecksum = (1 << 0),
            PrintInProgress = (1 << 1),
            PrintRequested = (1 << 2),
            ReadyToPrint = (1 << 3),
            LowVoltage = (1 << 4),
            Unknown = (1 << 5),
            PaperJam = (1 << 6),
            ThermalProblem = (1 << 7)
        };

        (ushort magic, PrinterCommands command, bool isCompressed, ushort dataLen, byte[] data, ushort checksum) packet;
        PrinterPacketBytes nextPacketByte;
        int dataBytesLeft;

        PrinterStatusBits status;
        PrinterPresenceBits presence;

        List<byte> imageData;
        byte marginBefore, marginAfter, palette, exposure;

        int imageHeight;
        int printDelay;

        byte serialData;

        public event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        protected virtual void OnSaveExtraData(SaveExtraDataEventArgs e) { SaveExtraData?.Invoke(this, e); }

        public GBPrinter()
        {
            imageData = new List<byte>();
        }

        public void Initialize()
        {
            ResetPacket();

            nextPacketByte = PrinterPacketBytes.MagicLSB;
            dataBytesLeft = 0;

            status = 0;
            presence = 0;

            marginBefore = marginAfter = 0;
            palette = exposure = 0;

            imageHeight = 0;
            printDelay = 0;

            serialData = 0;
        }

        public void Shutdown()
        {
            //
        }

        private void ResetPacket()
        {
            packet = (0, 0, false, 0, new byte[0], 0);
        }

        public byte ExchangeBit(int left, byte data)
        {
            var bitToSend = (byte)((serialData >> 7) & 0b1);
            serialData = (byte)((serialData << 1) | (data & 0b1));
            if (left == 0) serialData = ProcessReceivedByte(serialData);
            return bitToSend;
        }

        public byte ProcessReceivedByte(byte data)
        {
            byte ret = 0;

            switch (nextPacketByte)
            {
                case PrinterPacketBytes.MagicLSB:
                    /* Received: First magic byte
					 * Action:   Reset packet
					 * Send:     Nothing
					 */
                    if (data == 0x88)
                    {
                        ResetPacket();
                        packet.magic |= (ushort)(data << 8);
                        nextPacketByte = PrinterPacketBytes.MagicMSB;
                    }
                    break;

                case PrinterPacketBytes.MagicMSB:
                    /* Received: Second magic byte
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    if (data == 0x33)
                    {
                        packet.magic |= data;
                        nextPacketByte = PrinterPacketBytes.CommandByte;
                    }
                    break;

                case PrinterPacketBytes.CommandByte:
                    /* Received: Command byte
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    packet.command = (PrinterCommands)data;
                    nextPacketByte = PrinterPacketBytes.CompressionFlag;
                    break;

                case PrinterPacketBytes.CompressionFlag:
                    /* Received: Compression flag
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    packet.isCompressed = (data & 0x01) != 0;
                    nextPacketByte = PrinterPacketBytes.DataLengthLSB;
                    break;

                case PrinterPacketBytes.DataLengthLSB:
                    /* Received: Data length LSB
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    packet.dataLen |= data;
                    nextPacketByte = PrinterPacketBytes.DataLengthMSB;
                    break;

                case PrinterPacketBytes.DataLengthMSB:
                    /* Received: Data length MSB
					 * Action:   Prepare to receive data
					 * Send:     Nothing
					 */
                    packet.dataLen |= (ushort)(data << 8);
                    packet.data = new byte[packet.dataLen];
                    dataBytesLeft = packet.dataLen;
                    if (dataBytesLeft > 0)
                        nextPacketByte = PrinterPacketBytes.DataByte;
                    else
                        nextPacketByte = PrinterPacketBytes.ChecksumLSB;
                    break;

                case PrinterPacketBytes.DataByte:
                    /* Received: Data byte
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    if (dataBytesLeft > 0)
                    {
                        packet.data[--dataBytesLeft] = data;
                        if (dataBytesLeft == 0)
                            nextPacketByte = PrinterPacketBytes.ChecksumLSB;
                    }
                    break;

                case PrinterPacketBytes.ChecksumLSB:
                    /* Received: Checksum LSB
					 * Action:   Nothing
					 * Send:     Nothing
					 */
                    packet.checksum |= data;
                    nextPacketByte = PrinterPacketBytes.ChecksumMSB;
                    break;

                case PrinterPacketBytes.ChecksumMSB:
                    /* Received: Checksum MSB
					 * Action:   Nothing
					 * Send:     Printer presence
					 */
                    packet.checksum |= (ushort)(data << 8);
                    presence = PrinterPresenceBits.Present | PrinterPresenceBits.Unknown;
                    ret = (byte)presence;
                    nextPacketByte = PrinterPacketBytes.Execute;
                    break;

                case PrinterPacketBytes.Execute:
                    /* Received: Execute command
					 * Action:   Nothing
					 * Send:     Printer status
					 */

                    /* First, we're done with the packet, so check what we need to do now */
                    packet.data = packet.data.Reverse().ToArray();
                    switch (packet.command)
                    {
                        case PrinterCommands.Initialize:
                            /* Reset some data */
                            status = 0;
                            imageData.Clear();
                            imageHeight = 0;
                            printDelay = 0;
                            break;

                        case PrinterCommands.ImageTransfer:
                            /* Copy packet data for drawing, increase image height & tell GB we're ready to print */
                            if (packet.data.Length > 0)
                            {
                                if (packet.isCompressed)
                                {
                                    /* Decompress RLE first! */
                                    //List<byte> decomp = new List<byte>();
                                    List<byte> decomp = ObjectPoolAuto.AcquireList<byte>();

                                    int ofs = 0, numbytes = 0;
                                    while (ofs < packet.dataLen)
                                    {
                                        if ((packet.data[ofs] & 0x80) != 0)
                                        {
                                            /* Compressed */
                                            numbytes = (packet.data[ofs] & 0x7F) + 2;
                                            for (int i = 0; i < numbytes; i++) decomp.Add(packet.data[ofs + 1]);
                                            ofs += 2;
                                        }
                                        else
                                        {
                                            /* Uncompressed */
                                            numbytes = (packet.data[ofs] & 0x7F) + 1;
                                            for (int i = 0; i < numbytes; i++) decomp.Add(packet.data[ofs + 1 + i]);
                                            ofs += (numbytes + 1);
                                        }
                                    }
                                    packet.data = decomp.ToArray();
                                    packet.dataLen = (ushort)decomp.Count;

                                    ObjectPoolAuto.Release(decomp);
                                }

                                imageData.AddRange(packet.data);
                                imageHeight += (packet.data.Length / 0x28);

                                status |= PrinterStatusBits.ReadyToPrint;
                            }
                            break;

                        case PrinterCommands.StartPrinting:
                            /* Fetch parameters from packet, tell GB that we're about to print & perform printing */
                            marginBefore = (byte)((packet.data[1] >> 4) & 0x0F);
                            marginAfter = (byte)(packet.data[1] & 0x0F);
                            palette = packet.data[2];
                            exposure = (byte)(packet.data[3] & 0x7F);

                            status &= ~PrinterStatusBits.ReadyToPrint;
                            status |= PrinterStatusBits.PrintRequested;
                            PerformPrint();
                            break;

                        case PrinterCommands.ReadStatus:
                            if ((status & PrinterStatusBits.PrintRequested) != 0)
                            {
                                /* If we said printing has been requested, tell the GB it's in progress now */
                                status &= ~PrinterStatusBits.PrintRequested;
                                status |= PrinterStatusBits.PrintInProgress;
                            }
                            else if ((status & PrinterStatusBits.PrintInProgress) != 0)
                            {
                                /* Delay the process a bit... */
                                printDelay++;
                                if (printDelay >= 16)   // TODO: figure out actual print duration/timing?
                                {
                                    /* If we said printing is in progress, tell the GB we're finished with it */
                                    status &= ~PrinterStatusBits.PrintInProgress;
                                    printDelay = 0;
                                }
                            }
                            break;
                    }

                    /* End of packet */
                    DumpPacket();

                    ret = (byte)status;

                    nextPacketByte = PrinterPacketBytes.MagicLSB;
                    break;
            }

            return ret;
        }

        public byte DoMasterTransfer(byte data)
        {
            /* Not used */
            return 0xFF;
        }

        private void PerformPrint()
        {
            if (imageHeight == 0) return;

            //TODO 需要实现



            ///* Create bitmap for "printing" */
            //using (var image = new Bitmap(160, imageHeight))
            //{
            //	/* Convert image tiles to pixels */
            //	for (var y = 0; y < image.Height; y += 8)
            //	{
            //		for (var x = 0; x < image.Width; x += 8)
            //		{
            //			var tileAddress = ((y / 8) * 0x140) + ((x / 8) * 0x10);

            //			for (var py = 0; py < 8; py++)
            //			{
            //				for (var px = 0; px < 8; px++)
            //				{
            //					var ba = (imageData[tileAddress + 0] >> (7 - (px % 8))) & 0b1;
            //					var bb = (imageData[tileAddress + 1] >> (7 - (px % 8))) & 0b1;
            //					var c = (byte)((bb << 1) | ba);
            //					image.SetPixel(x + px, y + py, defaultPalette[(byte)((palette >> (c << 1)) & 0x03)]);
            //				}

            //				tileAddress += 2;
            //			}
            //		}
            //	}

            //	/* Apply approximate exposure (i.e. mess with the brightness a bit) */
            //	using (var adjustedImage = new Bitmap(image.Width, image.Height))
            //	{
            //		using (var g = System.Drawing.Graphics.FromImage(adjustedImage))
            //		{
            //			var scale = ((128 - exposure) / 128.0f) + 0.5f;
            //			var matrix = new float[][]
            //			{
            //				new float[] { scale, 0.0f, 0.0f, 0.0f, 0.0f },
            //				new float[] { 0.0f, scale, 0.0f, 0.0f, 0.0f },
            //				new float[] { 0.0f, 0.0f, scale, 0.0f, 0.0f },
            //				new float[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
            //				new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
            //			};

            //			var imageAttribs = new ImageAttributes();
            //			imageAttribs.ClearColorMatrix();
            //			imageAttribs.SetColorMatrix(new ColorMatrix(matrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            //			g.DrawImage(image, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttribs);

            //			/* Save the image */
            //			OnSaveExtraData(new SaveExtraDataEventArgs(ExtraDataTypes.Image, ExtraDataOptions.IncludeDateTime, "Printout", adjustedImage));
            //		}
            //	}
            //}


        }

        private void DumpPacket()
        {
            if (AppEnvironment.EnableLogger)
            {
                EssgeeLogger.WriteLine("[Received GB Printer Packet]");
                EssgeeLogger.WriteLine("- Magic bytes: 0x" + packet.magic.ToString("X4"));
                EssgeeLogger.WriteLine("- Command: " + packet.command.ToString());
                EssgeeLogger.WriteLine("- Is data compressed? " + packet.isCompressed.ToString());
                EssgeeLogger.WriteLine("- Data length: 0x" + packet.dataLen.ToString("X4"));
                if (packet.dataLen != 0)
                {
                    EssgeeLogger.WriteLine("- Data (UNCOMPRESSED):");
                    for (int line = 0; line < ((packet.dataLen / 16) == 0 ? 1 : (packet.dataLen / 16)); line++)
                    {
                        string msg = "";
                        msg += (" - 0x" + (line * 16).ToString("X4") + ": ");
                        for (int byteno = 0; byteno < ((packet.dataLen % 16) == 0 ? 0x10 : (packet.dataLen % 16)); byteno++)
                            msg += (packet.data[(line * 16) + byteno].ToString("X2") + " ");
                        EssgeeLogger.WriteLine(msg);
                        EssgeeLogger.WriteLine();
                    }
                }
                EssgeeLogger.WriteLine("- Checksum: 0x" + packet.checksum.ToString("X4"));
                EssgeeLogger.WriteLine("[Status Returned]");
                EssgeeLogger.WriteLine("- Presence: " + presence.ToString());
                EssgeeLogger.WriteLine("- Status: " + status.ToString());
                EssgeeLogger.WriteLine();
            }
        }
    }
}
