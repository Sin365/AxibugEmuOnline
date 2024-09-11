using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static AxiReplay.ReplayData;

namespace AxiReplay
{
    public class ReplayReader : IReplayReader
    {
        public ReplayData.ReplayFormat mFormat { get; private set; }
        public Encoding TexEncoding { get; private set; }
        ReplayHandler handler;
        string mTitle;
        string mNote;
        int mAllFrame;
        int mAllTime;
        long mData;
        int mSingleInputLenght;
        int mSingleDataLenght;
        FileStream mStream;
        BinaryReader mBinaryReader;

        int mCurrFrame = -1;
        byte[] mNextOutbytes;
        public ReplayStep currStep;
        public ReplayStep nextStep;
        bool bEnd;

        List<string> dbgList = new List<string>();
        bool bdbg = false;
        string dumpPath;

        public ReplayReader(string path, bool bWithDump = false, string dumppath = null)
        {
            dbgList.Clear();
            bdbg = bWithDump;
            dumpPath = dumppath;
            mStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            mBinaryReader = new BinaryReader(mStream);
            byte[] Outbytes;
            Outbytes = mBinaryReader.ReadBytes(ReplayData.HandlerLenght);
            handler = ReplayData.GetReplayHandlerFromData(Outbytes);
            mFormat = (ReplayFormat)handler.Format;
            switch (mFormat)
            {
                case ReplayData.ReplayFormat.FM32IP64: mSingleInputLenght = sizeof(UInt64); break;
                case ReplayData.ReplayFormat.FM32IP32: mSingleInputLenght = sizeof(UInt32); break;
                case ReplayData.ReplayFormat.FM32IP16: mSingleInputLenght = sizeof(UInt16); break;
                case ReplayData.ReplayFormat.FM32IPBYTE: mSingleInputLenght = sizeof(byte); break;
            }
            //Frame+Lenght
            mSingleDataLenght = (sizeof(UInt32)) + mSingleInputLenght;
            nextStep = new ReplayStep();
            nextStep.FrameStartID = -1;
            bEnd = false;

            dbgList.Add($"Format => {handler.Format}");
            dbgList.Add($"DataOffset => {handler.DataOffset}");
            dbgList.Add($"CreateTime => {handler.CreateTime}");
            dbgList.Add($"AllFrame => {handler.AllFrame}");
            dbgList.Add($"SingleLenght => {handler.SingleLenght}");


            mNextOutbytes = new byte[mSingleDataLenght];

            if (bWithDump)
            {
                int TestFrameIdx = -1;
                while (!bEnd)
                {
                    UpdateNextFrame(TestFrameIdx++);
                }
                File.WriteAllLines(dumppath, dbgList);
            }
            else
            {
                UpdateNextFrame(0);
            }
        }

        
        void UpdateNextFrame(int targetFrame)
        {
            //如果已经超过
            while (targetFrame >= nextStep.FrameStartID)
            {
                if (nextStep.FrameStartID >= handler.AllFrame)
                {
                    bEnd = true;
                    break;
                }
                mBinaryReader.Read(mNextOutbytes, 0, mSingleDataLenght);
                switch (mFormat)
                {
                    case ReplayFormat.FM32IP64:
                        {
                            nextStep.FrameStartID = BitConverter.ToInt32(mNextOutbytes, 0);
                            nextStep.InPut = BitConverter.ToUInt64(mNextOutbytes, sizeof(UInt32));
                        }
                        break;
                    case ReplayFormat.FM32IP32:
                        {
                            nextStep.All64Data = BitConverter.ToUInt64(mNextOutbytes, 0);
                        }
                        break;
                    case ReplayFormat.FM32IP16:
                        {
                            nextStep.All64Data = BitConverter.ToUInt64(mNextOutbytes, 0);
                        }
                        break;
                    case ReplayFormat.FM32IPBYTE:
                        {
                            nextStep.All64Data = BitConverter.ToUInt64(mNextOutbytes, 0);
                        }
                        break;
                }
                dbgList.Add($"{nextStep.FrameStartID} | {nextStep.InPut}");

                targetFrame++;
            }
        }

        int byFrameIdx = 0;

        /// <summary>
        /// 往前推进1帧的Input(返回是否变化）
        /// </summary>
        public bool NextFrame(out ReplayStep data)
        {
            return TakeFrame(1,out data);
        }

        /// <summary>
        /// 往前推进指定帧数量的Input (返回是否变化）
        /// </summary>
        /// <param name="addFrame"></param>
        public bool TakeFrame(int addFrame,out ReplayStep data)
        {
            bool Changed = false;
            mCurrFrame += addFrame;
            if (mCurrFrame >= nextStep.FrameStartID)
            {
                Changed = currStep.InPut != nextStep.InPut;
                currStep = nextStep;
                data = currStep;
                UpdateNextFrame(mCurrFrame);
            }
            else
            {
                data = currStep;
            }
            return Changed;
        }

        /// <summary>
        /// 往前推进帧的,指定帧下标
        /// </summary>
        public bool NextFramebyFrameIdx(int FrameID, out ReplayStep data)
        {
            bool res = TakeFrame(FrameID - byFrameIdx, out data);
            byFrameIdx = FrameID;
            return res;
        }

        public void Dispose()
        {
            mStream.Dispose();
            mBinaryReader.Dispose();
            //TODO
        }

    }
}
