using System;
using System.ComponentModel;
using System.Linq;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    /* Image processing, etc. based on https://github.com/AntonioND/gbcam-rev-engineer/blob/master/doc/gb_camera_doc_v1_1_1.pdf */

    public class GBCameraCartridge : IGameBoyCartridge
    {
        const int camSensorExtraLines = 8;
        const int camSensorWidth = 128;
        const int camSensorHeight = 112 + camSensorExtraLines;

        const int camWidth = 128;
        const int camHeight = 112;

        static readonly float[] edgeRatioLookUpTable = new float[] { 0.50f, 0.75f, 1.00f, 1.25f, 2.00f, 3.00f, 4.00f, 5.00f };

        public enum ImageSources
        {
            [Description("Random Noise")]
            Noise,
            [Description("Image File")]
            File
        }

        ImageSources imageSourceType;
        //Bitmap scaledImage;

        readonly int[,] webcamOutput, camRetinaOutput;
        readonly byte[,,] tileBuffer;

        byte[] romData, ramData;
        bool hasBattery;

        byte romBank, ramBank;
        bool ramEnable;

        readonly byte[] camRegisters;
        bool camSelected;

        int cameraCycles, camClocksLeft;

        public GBCameraCartridge(int romSize, int ramSize)
        {
            imageSourceType = ImageSources.Noise;
            //scaledImage = new Bitmap(camSensorWidth, camSensorHeight);

            webcamOutput = new int[camSensorWidth, camSensorHeight];
            camRetinaOutput = new int[camSensorWidth, camSensorHeight];
            tileBuffer = new byte[14, 16, 16];

            romData = new byte[romSize];
            ramData = new byte[ramSize];

            romBank = 1;
            ramBank = 0;

            ramEnable = false;

            camRegisters = new byte[0x80];  // 0x36 used
            camSelected = false;

            hasBattery = false;
        }


        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            return data;
        }
        #endregion

        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));
        }

        public void LoadRam(byte[] data)
        {
            Buffer.BlockCopy(data, 0, ramData, 0, Math.Min(data.Length, ramData.Length));
        }

        public byte[] GetRomData()
        {
            return romData;
        }

        public byte[] GetRamData()
        {
            return ramData;
        }

        public bool IsRamSaveNeeded()
        {
            return hasBattery;
        }

        public ushort GetLowerBound()
        {
            return 0x0000;
        }

        public ushort GetUpperBound()
        {
            return 0x7FFF;
        }

        public void SetCartridgeConfig(bool battery, bool rtc, bool rumble)
        {
            hasBattery = battery;
        }

        public void SetImageSource(ImageSources source, string filename)
        {
            imageSourceType = source;

            if (imageSourceType == ImageSources.File)
            {
                //TODO  imageSourceType == ImageSources.File

                //using (var tempImage = new Bitmap(filename))
                //{
                //	using (var g = System.Drawing.Graphics.FromImage(scaledImage))
                //	{
                //		var ratio = Math.Min(tempImage.Width / (float)camSensorWidth, tempImage.Height / (float)camSensorHeight);
                //		var srcWidth = (int)(camSensorWidth * ratio);
                //		var srcHeight = (int)(camSensorHeight * ratio);
                //		var srcX = (tempImage.Width - srcWidth) / 2;
                //		var srcY = (tempImage.Height - srcHeight) / 2;
                //		var scaledRect = new Rectangle(0, 0, camSensorWidth, camSensorHeight);

                //		g.FillRectangle(Brushes.White, scaledRect);
                //		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                //		g.DrawImage(tempImage, scaledRect, new Rectangle(srcX, srcY, srcWidth, srcHeight), GraphicsUnit.Pixel);
                //	}
                //}

                //for (var x = 0; x < camSensorWidth; x++)
                //	for (var y = 0; y < camSensorHeight; y++)
                //		webcamOutput[x, y] = (int)(scaledImage.GetPixel(x, y).GetBrightness() * 255);
            }
        }

        public void Step(int clockCyclesInStep)
        {
            cameraCycles += clockCyclesInStep;
            if (cameraCycles >= camClocksLeft)
            {
                camRegisters[0x00] &= 0xFE;
                cameraCycles = 0;
            }
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF)
            {
                return romData[address & 0x3FFF];
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                return romData[(romBank << 14) | (address & 0x3FFF)];
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (!camSelected)
                {
                    if ((camRegisters[0x00] & 0b1) == 0)
                        return ramData[(ramBank << 13) | (address & 0x1FFF)];
                    else
                        return 0xFF;
                }
                else
                {
                    var reg = (byte)(address & 0x7F);
                    if (reg == 0x00)
                        return (byte)(camRegisters[reg] & 0x07);
                    else
                        return 0xFF;
                }
            }
            else
                return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                romBank = (byte)((romBank & 0xC0) | (value & 0x3F));
                romBank &= (byte)((romData.Length >> 14) - 1);
                if ((romBank & 0x3F) == 0x00) romBank |= 0x01;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if ((value & 0x10) != 0)
                {
                    camSelected = true;
                }
                else
                {
                    camSelected = false;
                    ramBank = (byte)(value & 0x0F);
                }
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (!camSelected)
                {
                    if (ramEnable)
                        ramData[(ramBank << 13) | (address & 0x1FFF)] = value;
                }
                else
                {
                    var reg = (byte)(address & 0x7F);
                    if (reg == 0x00 && (value & 0b1) != 0)
                        GenerateImage();

                    camRegisters[reg] = value;
                }
            }
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }

        private void GenerateImage()
        {
            /* Get configuration -- register 0 */
            var pBits = 0;
            var mBits = 0;

            switch ((camRegisters[0x00] >> 1) & 0b11)
            {
                case 0: pBits = 0x00; mBits = 0x01; break;
                case 1: pBits = 0x01; mBits = 0x00; break;
                case 2:
                case 3: pBits = 0x01; mBits = 0x02; break;
            }

            /* Register 1 */
            var nBit = ((camRegisters[0x01] >> 7) & 0b1) != 0;
            var vhBits = (camRegisters[0x01] >> 5) & 0b11;

            /* Registers 2 and 3 */
            var exposureBits = camRegisters[0x02] << 8 | camRegisters[0x03];

            /* Register 4 */
            var edgeAlpha = edgeRatioLookUpTable[(camRegisters[0x04] >> 4) & 0b111];
            var e3Bit = ((camRegisters[0x04] >> 7) & 0b1) != 0;
            var iBit = ((camRegisters[0x04] >> 3) & 0b1) != 0;

            /* Calculate timings */
            camClocksLeft = 4 * (32446 + (nBit ? 0 : 512) + 16 * exposureBits);

            /* Clear tile buffer */
            for (var j = 0; j < 14; j++)
                for (var i = 0; i < 16; i++)
                    for (var k = 0; k < 16; k++)
                        tileBuffer[j, i, k] = 0x00;

            /* Sensor handling */
            /* Copy webcam buffer to sensor buffer, apply color correction & exposure time */
            for (var i = 0; i < camSensorWidth; i++)
            {
                for (var j = 0; j < camSensorHeight; j++)
                {
                    var value = 0;
                    switch (imageSourceType)
                    {
                        case ImageSources.File: value = webcamOutput[i, j]; break;
                        case ImageSources.Noise: value = EmuStandInfo.Random.Next(255); break;
                    }

                    value = (value * exposureBits) / 0x0300;
                    value = 128 + (((value - 128) * 1) / 8);
                    camRetinaOutput[i, j] = Clamp(value, 0, 255);

                    /* Invert */
                    if (iBit)
                        camRetinaOutput[i, j] = 255 - camRetinaOutput[i, j];

                    /* Make signed */
                    camRetinaOutput[i, j] = camRetinaOutput[i, j] - 128;
                }
            }

            var tempBuffer = new int[camSensorWidth, camSensorHeight];
            var filteringMode = (nBit ? 8 : 0) | (vhBits << 1) | (e3Bit ? 1 : 0);
            switch (filteringMode)
            {
                case 0x00:
                    /* 1-D filtering */
                    for (var i = 0; i < camSensorWidth; i++)
                        for (var j = 0; j < camSensorHeight; j++)
                            tempBuffer[i, j] = camRetinaOutput[i, j];

                    for (var i = 0; i < camSensorWidth; i++)
                    {
                        for (var j = 0; j < camSensorHeight; j++)
                        {
                            var ms = tempBuffer[i, Math.Min(j + 1, camSensorHeight - 1)];
                            var px = tempBuffer[i, j];

                            var value = 0;
                            if ((pBits & 0b01) != 0) value += px;
                            if ((pBits & 0b10) != 0) value += ms;
                            if ((mBits & 0b01) != 0) value -= px;
                            if ((mBits & 0b10) != 0) value -= ms;

                            camRetinaOutput[i, j] = Clamp(value, -128, 127);
                        }
                    }
                    break;

                case 0x02:
                    /* 1-D filtering + Horiz. enhancement : P + {2P-(MW+ME)} * alpha */
                    for (var i = 0; i < camSensorWidth; i++)
                    {
                        for (var j = 0; j < camSensorHeight; j++)
                        {
                            var mw = camRetinaOutput[Math.Max(0, i - 1), j];
                            var me = camRetinaOutput[Math.Min(i + 1, camSensorWidth - 1), j];
                            var px = camRetinaOutput[i, j];

                            tempBuffer[i, j] = Clamp((int)(px + ((2 * px - mw - me) * edgeAlpha)), 0, 255);
                        }
                    }

                    for (var i = 0; i < camSensorWidth; i++)
                    {
                        for (var j = 0; j < camSensorHeight; j++)
                        {
                            var ms = tempBuffer[i, Math.Min(j + 1, camSensorHeight - 1)];
                            var px = tempBuffer[i, j];

                            var value = 0;
                            if ((pBits & 0b01) != 0) value += px;
                            if ((pBits & 0b10) != 0) value += ms;
                            if ((mBits & 0b01) != 0) value -= px;
                            if ((mBits & 0b10) != 0) value -= ms;

                            camRetinaOutput[i, j] = Clamp(value, -128, 127);
                        }
                    }
                    break;

                case 0x0E:
                    /* 2D enhancement : P + {4P-(MN+MS+ME+MW)} * alpha */
                    for (var i = 0; i < camSensorWidth; i++)
                    {
                        for (var j = 0; j < camSensorHeight; j++)
                        {
                            var ms = camRetinaOutput[i, Math.Min(j + 1, camSensorHeight - 1)];
                            var mn = camRetinaOutput[i, Math.Max(0, j - 1)];
                            var mw = camRetinaOutput[Math.Max(0, i - 1), j];
                            var me = camRetinaOutput[Math.Min(i + 1, camSensorWidth - 1), j];
                            var px = camRetinaOutput[i, j];

                            tempBuffer[i, j] = Clamp((int)(px + ((4 * px - mw - me - mn - ms) * edgeAlpha)), -128, 127);
                        }
                    }

                    for (var i = 0; i < camSensorWidth; i++)
                        for (var j = 0; j < camSensorHeight; j++)
                            camRetinaOutput[i, j] = tempBuffer[i, j];
                    break;

                case 0x01:
                    /* Unknown, always same color; sensor datasheet does not document this, maybe a bug? */
                    for (var i = 0; i < camSensorWidth; i++)
                        for (var j = 0; j < camSensorHeight; j++)
                            camRetinaOutput[i, j] = 0;
                    break;

                default:
                    /* Unknown; write to log if enabled */
                    if (AppEnvironment.EnableLogger)
                    {
                        EssgeeLogger.WriteLine($"Unsupported GB Camera mode 0x{filteringMode:X2}");
                        EssgeeLogger.WriteLine(string.Join(" ", camRegisters.Take(6).Select(x => $"0x{x:X2}")));
                    }
                    break;
            }

            /* Make unsigned */
            for (var i = 0; i < camSensorWidth; i++)
                for (var j = 0; j < camSensorHeight; j++)
                    camRetinaOutput[i, j] = camRetinaOutput[i, j] + 128;

            /* Convert output to GB tiles */
            for (var i = 0; i < camWidth; i++)
            {
                for (var j = 0; j < camHeight; j++)
                {
                    var sensorValue = camRetinaOutput[i, j + (camSensorExtraLines / 2)];
                    var matrixOffset = 0x06 + ((j % 4) * 12) + ((i % 4) * 3);

                    var c = (byte)0;
                    if (sensorValue < camRegisters[matrixOffset + 0]) c = 3;
                    else if (sensorValue < camRegisters[matrixOffset + 1]) c = 2;
                    else if (sensorValue < camRegisters[matrixOffset + 2]) c = 1;
                    else c = 0;

                    if ((c & 1) != 0) tileBuffer[j >> 3, i >> 3, ((j & 7) * 2) + 0] |= (byte)(1 << (7 - (7 & i)));
                    if ((c & 2) != 0) tileBuffer[j >> 3, i >> 3, ((j & 7) * 2) + 1] |= (byte)(1 << (7 - (7 & i)));
                }
            }

            /* Copy tiles to cartridge RAM */
            int outputOffset = 0x100;
            for (var j = 0; j < 14; j++)
                for (var i = 0; i < 16; i++)
                    for (var k = 0; k < 16; k++)
                        ramData[outputOffset++] = tileBuffer[j, i, k];
        }
    }
}
