using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AxiReplay
{
    public class ReplayWriter : IReplayWriter
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
        MemoryStream mStream;
        BinaryWriter mBinaryWriter;

        int mCurrFrame;
        UInt64 mCurrInput;
        ReplayStep wirteStep;

        List<string> dbgList = new List<string>();

        public ReplayWriter(string Title, string Note, ReplayData.ReplayFormat format, Encoding encoding)
        {
            mTitle = Title;
            mNote = Note;
            TexEncoding = encoding;
            mFormat = format;
            switch (mFormat)
            {
                case ReplayData.ReplayFormat.FM32IP64: mSingleInputLenght = sizeof(UInt64); break;
                case ReplayData.ReplayFormat.FM32IP32: mSingleInputLenght = sizeof(UInt32); break;
                case ReplayData.ReplayFormat.FM32IP16: mSingleInputLenght = sizeof(UInt16); break;
                case ReplayData.ReplayFormat.FM32IPBYTE: mSingleInputLenght = sizeof(byte); break;
            }
            mSingleDataLenght = (sizeof(UInt32)) + mSingleInputLenght;

            mStream = new MemoryStream();
            mBinaryWriter = new BinaryWriter(mStream);

            mCurrFrame = -1;
            mCurrInput = int.MaxValue;
            wirteStep = new ReplayStep();

            dbgList.Clear();

        }

        int byFrameIdx = 0;
        /// <summary>
        /// 往前推进帧的,指定帧下标
        /// </summary>
        /// <param name="frameInput"></param>
        public void NextFramebyFrameIdx(int FrameID,UInt64 frameInput)
        {
            TakeFrame(FrameID - byFrameIdx, frameInput);
            byFrameIdx = FrameID;
        }

        /// <summary>
        /// 往前推进1帧的Input
        /// </summary>
        /// <param name="frameInput"></param>
        public void NextFrame(UInt64 frameInput)
        {
            TakeFrame(1, frameInput);
        }

        /// <summary>
        /// 往前推进指定帧数量的Input
        /// </summary>
        /// <param name="frameInput"></param>
        public void TakeFrame(int addFrame, UInt64 frameInput)
        {
            if (addFrame < 0)
            {
                
            }
            mCurrFrame += addFrame;
            if (mCurrInput == frameInput)
                return;
            mCurrInput = frameInput;

            wirteStep.FrameStartID = mCurrFrame;
            wirteStep.InPut = mCurrInput;
            dbgList.Add($"{mCurrFrame} | {mCurrInput}");

            switch (mFormat)
            {
                case ReplayData.ReplayFormat.FM32IP64:
                    mBinaryWriter.Write(wirteStep.FrameStartID);
                    mBinaryWriter.Write(wirteStep.InPut);
                    break;
                case ReplayData.ReplayFormat.FM32IP32:
                    mBinaryWriter.Write(BitConverter.GetBytes(wirteStep.All64Data), 0, 4 + 4);
                    break;
                case ReplayData.ReplayFormat.FM32IP16:
                    mBinaryWriter.Write(BitConverter.GetBytes(wirteStep.All64Data), 0, 4 + 2);
                    break;
                case ReplayData.ReplayFormat.FM32IPBYTE:
                    mBinaryWriter.Write(BitConverter.GetBytes(wirteStep.All64Data), 0, 4 + 1);
                    break;
            }
        }

        public void SaveData(string path, bool bWithDump = false, string dumppath = null)
        {
            ReplayData.GetStringByteData(mTitle, out byte[] titleData, out int titleLenghtWithEnd, TexEncoding);
            ReplayData.GetStringByteData(mNote, out byte[] noteData, out int noteLenghtWithEnd, TexEncoding);

            ReplayHandler handler = new ReplayHandler();
            handler.Format = (int)this.mFormat;
            handler.DataOffset = ReplayData.HandlerLenght;
            handler.CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            handler.AllFrame = wirteStep.FrameStartID;
            handler.SingleLenght = mSingleDataLenght;

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    //写入Handler
                    bw.Write(ReplayData.GetHandlerData(handler));
                    //写入Data
                    bw.Write(mStream.ToArray());
                }
            }

            if (bWithDump)
            {
                List<string> temp = new List<string>();
                temp.Add($"Format => {handler.Format}");
                temp.Add($"DataOffset => {handler.DataOffset}");
                temp.Add($"CreateTime => {handler.CreateTime}");
                temp.Add($"AllFrame => {handler.AllFrame}");
                temp.Add($"SingleLenght => {handler.SingleLenght}");
                dbgList.InsertRange(0,temp);
                File.WriteAllLines(dumppath, dbgList);
            }
        }

        public void Dispose()
        {
            mStream.Dispose();
            mBinaryWriter.Dispose();
            //TODO
        }

    }
}
