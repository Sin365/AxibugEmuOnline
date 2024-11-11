using System.Collections.Generic;

namespace AxiReplay
{
    public class NetReplay
    {
        /// <summary>
        /// 客户端当前帧
        /// </summary>
        public int mCurrClientFrameIdx => mCurrReplay.FrameStartID;
        /// <summary>
        /// 服务器远端当前帧
        /// </summary>
        public int mRemoteFrameIdx { get; private set; } = int.MinValue;
        /// <summary>
        /// 网络数据队列
        /// </summary>
        Queue<ReplayStep> mNetReplayQueue = new Queue<ReplayStep>();
        /// <summary>
        /// 当前数据
        /// </summary>
        ReplayStep mCurrReplay;
        /// <summary>
        /// 下一个数据数据
        /// </summary>
        ReplayStep mNextReplay;
        public NetReplay()
        {
            ResetData();
        }
        public void ResetData()
        {
            mNetReplayQueue.Clear();
            mRemoteFrameIdx = 0;
            mCurrReplay = default(ReplayStep);
            mCurrReplay.FrameStartID = int.MinValue;
            mNextReplay = default(ReplayStep);
            mNextReplay.FrameStartID = 0;
        }
        public void InData(ReplayStep inputData, int ServerFrameIdx)
        {
            mNetReplayQueue.Enqueue(inputData);
            mRemoteFrameIdx = inputData.FrameStartID;
        }
        public bool TryGetNextFrame(out ReplayStep data, out int frameDiff, out bool inputDiff)
        {
            TakeFrame(1, out data, out frameDiff, out inputDiff);
            return frameDiff > 0;
        }
        void TakeFrame(int addFrame, out ReplayStep data, out int bFrameDiff, out bool inputDiff)
        {
            inputDiff = false;
            int targetFrame = mCurrClientFrameIdx + addFrame;
            if (targetFrame <= mNextReplay.FrameStartID + 1 && targetFrame <= mRemoteFrameIdx && mNetReplayQueue.Count > 0)
            {
                //当前帧追加
                ulong oldInput = mCurrReplay.InPut;
                mCurrReplay = mNextReplay;
                if (oldInput != mCurrReplay.InPut)
                    inputDiff = true;
                mNextReplay = mNetReplayQueue.Dequeue();
            }

            bFrameDiff = mRemoteFrameIdx - mCurrClientFrameIdx;
            data = mCurrReplay;
        }
    }
}
