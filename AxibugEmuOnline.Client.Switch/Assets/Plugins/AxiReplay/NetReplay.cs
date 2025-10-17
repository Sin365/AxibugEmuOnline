﻿using System.Collections.Generic;
using UnityEngine;

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
        public Queue<ReplayStep> mNetReplayQueue { get; private set; } = new Queue<ReplayStep>();
        /// <summary>
        /// 当前数据
        /// </summary>
        ReplayStep mCurrReplay;
        /// <summary>
        /// 下一个数据数据
        /// </summary>
        ReplayStep mNextReplay;

        FrameProfiler frameProfiler = new FrameProfiler();

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

            frameProfiler.Reset();
        }
        public void InData(ReplayStep inputData, int ServerFrameIdx, uint ServerForwardCount)
        {
            mRemoteForwardCount = (int)ServerForwardCount;
            mNetReplayQueue.Enqueue(inputData);
            //Debug.Log($"InData=>{inputData.FrameStartID} QCount = >{mNetReplayQueue.Count}");
            mRemoteFrameIdx = inputData.FrameStartID;
            if (!bNetInit)
            {
                bNetInit = true;
                mNextReplay = mNetReplayQueue.Dequeue();
            }

            frameProfiler.InputHead(inputData.FrameStartID);
        }

        public bool TryGetNextFrame(int targetFrame, bool indirectGet, out ReplayStep data, out int frameDiff, out bool inputDiff)
        {
            if (!bNetInit)
            {
                data = default(ReplayStep);
                frameDiff = default(int);
                inputDiff = false;
                return false;
            }
            return TakeFrameToTargetFrame(targetFrame, indirectGet, out data, out frameDiff, out inputDiff);
        }

        bool checkCanGetFrame(int targetFrame, bool indirectGet)
        {
            if (indirectGet)
            {
                return targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx;
            }
            else
            {
                return targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx && mNetReplayQueue.Count >= frameProfiler.TempFrameCount(mRemoteForwardCount);
            }
        }

        bool TakeFrameToTargetFrame(int targetFrame, bool indirectGet, out ReplayStep data, out int bFrameDiff, out bool inputDiff)
        {
            bool result;
            inputDiff = false;

            if (checkCanGetFrame(targetFrame, indirectGet))
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
            if (!bNetInit)
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
            else if (moreNum <= mRemoteForwardCount) skip = 0;
            else if (moreNum <= mRemoteForwardCount + 2) skip = 0;
            else if (moreNum <= mRemoteForwardCount + 5) skip = 1;
            else if (moreNum <= mRemoteForwardCount + 6) skip = 2;
            else if (moreNum <= mRemoteForwardCount + 20) skip = moreNum / 2; //20帧以内，平滑跳帧数
            else skip = moreNum;//完全追上
            return skip;

            //int skip = 0;
            //if (mDiffFrameCount > short.MaxValue) skip = 0;
            //else if (moreNum <= 1) skip = 0;
            //else if (moreNum <= 3) skip = 2;
            //else if (moreNum <= 6) skip = 2;
            //else if (moreNum <= 20) skip = moreNum / 2; //20帧以内，平滑跳帧数
            //else skip = moreNum;//完全追上
            //return skip;

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
