using System.Collections.Generic;

namespace AxiReplay
{
    public class NetReplay
    {
        int MaxInFrame = 0;
        public int mCurrPlayFrame = -1;
        Queue<ReplayStep> mQueueReplay;
        ReplayStep mNextReplay;
        ReplayStep mCurrReplay;
        int byFrameIdx = 0;
        /// <summary>
        /// 服务器远端当前帧
        /// </summary>
        public int remoteFrameIdx { get; private set; }
        /// <summary>
        /// 当前帧和服务器帧相差数量
        /// </summary>
        public int remoteFrameDiff => remoteFrameIdx - mCurrPlayFrame;
        public NetReplay()
        {
            mQueueReplay = new Queue<ReplayStep>();
        }
        public void InData(ReplayStep inputData,int ServerFrameIdx)
        {
            mQueueReplay.Enqueue(inputData);
            MaxInFrame = inputData.FrameStartID;
            remoteFrameIdx = ServerFrameIdx;
        }
        public bool NextFrame(out ReplayStep data, out int FrameDiff)
        {
            return TakeFrame(0, out data, out FrameDiff);
        }
        /// <summary>
        /// 往前推进帧的,指定帧下标
        /// </summary>
        public bool NextFramebyFrameIdx(int FrameID, out ReplayStep data, out int FrameDiff)
        {
            bool res = TakeFrame(FrameID - byFrameIdx, out data, out FrameDiff);
            byFrameIdx = FrameID;
            return res;
        }
        public bool TakeFrame(int addFrame, out ReplayStep data, out int FrameDiff)
        {
            bool Changed = false;
            mCurrPlayFrame += addFrame;
            if (mCurrPlayFrame >= mNextReplay.FrameStartID)
            {
                Changed = mCurrReplay.InPut != mNextReplay.InPut;
                mCurrReplay = mNextReplay;
                data = mCurrReplay;
                UpdateNextFrame(mCurrPlayFrame, out FrameDiff);
            }
            else
            {
                data = mCurrReplay;
                FrameDiff = MaxInFrame - mCurrPlayFrame;
            }
            return Changed;
        }
        void UpdateNextFrame(int targetFrame,out int FrameDiff)
        {
            FrameDiff = MaxInFrame - targetFrame;
            //如果已经超过
            while (targetFrame > mNextReplay.FrameStartID)
            {
                if (mNextReplay.FrameStartID >= MaxInFrame)
                {
                    //TODO
                    //bEnd = true;
                    break;
                }

                if (mQueueReplay.Count > 0)
                {
                    mNextReplay = mQueueReplay.Dequeue();
                }
                targetFrame++;
            }
        }
    }
}
