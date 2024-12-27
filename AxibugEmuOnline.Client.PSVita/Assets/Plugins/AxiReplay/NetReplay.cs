using System.Collections.Generic;

namespace AxiReplay
{
    public class NetReplay
    {
        /// <summary>
        /// 客户端当前帧
        /// </summary>
        public int mCurrClientFrameIdx = 0;
        /// <summary>
        /// 服务器远端当前帧
        /// </summary>
        public int mRemoteFrameIdx { get; private set; }
        /// <summary>
        /// 服务器远端当前提前量
        /// </summary>
        public int mRemoteForwardCount { get; private set; }
        /// <summary>
        /// Remote 2 Client Frame Gap
        /// </summary>
        public int mDiffFrameCount => mRemoteFrameIdx - mCurrClientFrameIdx;
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

        bool bNetInit = false;
        public NetReplay()
        {
            ResetData();
        }
        public void ResetData()
        {
            mNetReplayQueue.Clear();
            mCurrReplay = default(ReplayStep);
            mCurrReplay.FrameStartID = int.MinValue;
            bNetInit = false;
        }
        public void InData(ReplayStep inputData, int ServerFrameIdx)
        {
            mNetReplayQueue.Enqueue(inputData);
            mRemoteFrameIdx = inputData.FrameStartID;
            if (!bNetInit)
            {
                bNetInit = true;
                mNextReplay = mNetReplayQueue.Dequeue();
            }
        }
        public bool TryGetNextFrame(out ReplayStep data, out int frameDiff, out bool inputDiff)
        {
            if (!bNetInit)
            {
                data = default(ReplayStep);
                frameDiff = default(int);
                inputDiff = false;
                return false;
            }
            TakeFrame(1, out data, out frameDiff, out inputDiff);
            return frameDiff > 0;
        }

        public bool TryGetNextFrame(int targetFrame, out ReplayStep data, out int frameDiff, out bool inputDiff)
        {
            if (!bNetInit)
            {
                data = default(ReplayStep);
                frameDiff = default(int);
                inputDiff = false;
                return false;
            }
            return TakeFrameToTargetFrame(targetFrame, out data, out frameDiff, out inputDiff);
        }

        void TakeFrame(int addFrame, out ReplayStep data, out int bFrameDiff, out bool inputDiff)
        {
            int targetFrame = mCurrClientFrameIdx + addFrame;
            TakeFrameToTargetFrame(targetFrame, out data, out bFrameDiff, out inputDiff);
        }

        bool TakeFrameToTargetFrame(int targetFrame, out ReplayStep data, out int bFrameDiff, out bool inputDiff)
        {
            bool result;
            inputDiff = false;
            if (targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx && mNetReplayQueue.Count > 0)
            {
                //当前帧追加
                mCurrClientFrameIdx = targetFrame;
                ulong oldInput = mCurrReplay.InPut;
                mCurrReplay = mNextReplay;
                if (oldInput != mCurrReplay.InPut)
                    inputDiff = true;
                mNextReplay = mNetReplayQueue.Dequeue();
                result = true;
            }
            else
                result = false;

            bFrameDiff = mRemoteFrameIdx - mCurrClientFrameIdx;
            data = mCurrReplay;

            return result;
        }

        public int GetSkipFrameCount()
        {
            if(!bNetInit)
                return 0;
            //本地队列差异高于服务器提前量的值
            int moreNum = mDiffFrameCount - mRemoteForwardCount;
            //if (mDiffFrameCount < 0 || mDiffFrameCount > 10000)
            //    return 0;

            ////游戏刚开始的一小段时间，直接追满
            //if (mCurrClientFrameIdx < 60)
            //    return moreNum;

            int skip = 0;
            if (mDiffFrameCount > short.MaxValue) skip = 0;
            else if (moreNum <= 1) skip = 0;
            else if (moreNum <= 3) skip = 2;
            else if (moreNum <= 6) skip = 2;
            else if (moreNum <= 20) skip = moreNum / 2; //20帧以内，平滑跳帧数
            else skip = moreNum;//完全追上
            return skip;

            //var frameGap = mDiffFrameCount;
            //if (frameGap > 10000) return 0;
            //if (frameGap <= 2) skip = 0;
            //if (frameGap > 2 && frameGap < 6) skip = 1 + 1;
            //else if (frameGap > 7 && frameGap < 12) skip = 2 + 1;
            //else if (frameGap > 13 && frameGap < 20) skip = 3 + 1;
            //else skip = frameGap - 2;


            //return skip;
        }
    }
}
