using System.Collections.Generic;
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
#if UNITY_EDITOR
            //Debug.Log($"InData=>{inputData.FrameStartID} QCount = >{mNetReplayQueue.Count}");
#endif
            mRemoteFrameIdx = inputData.FrameStartID;
            if (!bNetInit)
            {
                bNetInit = true;
                mNextReplay = mNetReplayQueue.Dequeue();
            }

            frameProfiler.InputHead(inputData.FrameStartID);
        }

        /// <summary>
        /// 尝试往前推进帧
        /// </summary>
        /// <param name="targetFrame"></param>
        /// <param name="indirectGet"></param>
        /// <param name="data"></param>
        /// <param name="frameDiff"></param>
        /// <param name="inputDiff"></param>
        /// <returns></returns>
        public bool TryGetNextFrame(int targetFrame, bool indirectGet, out ReplayStep data, out int frameDiff, out bool inputDiff)
        {
            if (!bNetInit)//单机模式无条件推进
            {
                data = default(ReplayStep);
                frameDiff = default(int);
                inputDiff = false;
                return false;
            }
            return TakeFrameToTargetFrame(targetFrame, indirectGet, out data, out frameDiff, out inputDiff);
        }

        bool TakeFrameToTargetFrame(int targetFrame, bool indirectGet, out ReplayStep data, out int bFrameDiff, out bool inputDiff)
        {
            bool result;
            inputDiff = false;

            if (CheckCanDoNextFrame(targetFrame, indirectGet))
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


        /// <summary>
        /// 激进取队列，连续行为计数
        /// </summary>
        uint radicalUseCounter;
        /// <summary>
        /// 激进取队列，连续行为最大
        /// </summary>
        byte radicalMaxCount = 6;

        /// <summary>
        /// 检查是否可以推进帧
        /// </summary>
        /// <param name="targetFrame">目标帧</param>
        /// <param name="indirectGet">（废弃参数）</param>
        /// <returns></returns>
        bool CheckCanDoNextFrame(int targetFrame, bool indirectGet)
        {

            int queueLeftCount = mNetReplayQueue.Count;
            //至少还有可用数据的基本判断
            bool mustblag = targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx && queueLeftCount > 0;

            if (!mustblag)
            {
#if UNITY_EDITOR
                Debug.Log("推帧规则怪谈|无");
#endif
                radicalUseCounter = 0;
                return false;
            }

            //队列还有余量
            if (queueLeftCount > 1)
            {
#if UNITY_EDITOR
                Debug.Log("推帧规则怪谈|冗余使用");
#endif
                radicalUseCounter = 0;
                return true;
            }
            else
            {
                //超过连续激进使用的频次
                if (radicalUseCounter > radicalMaxCount)
                {
#if UNITY_EDITOR
                    Debug.Log("推帧规则怪谈|当前停止激进");
#endif
                    radicalUseCounter = 0;
                    return false;
                }
                else//上一帧
                {
                    radicalUseCounter++;
#if UNITY_EDITOR
                    Debug.Log($"推帧规则怪谈|激进使用{radicalUseCounter}");
#endif
                    return true;
                }
            }


            if (indirectGet)
            {
                //指定帧不大于远端帧数
                return targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx && mNetReplayQueue.Count >= 0;
            }
            else
            {
                //指定帧不大于远端帧数 且 满足 网络队列数 不小于 最小队列阈值
                return targetFrame == mNextReplay.FrameStartID && targetFrame <= mRemoteFrameIdx && mNetReplayQueue.Count >= frameProfiler.TempFrameCount(mRemoteForwardCount);
            }
        }

        /// <summary>
        /// //指定帧不大于远端帧数
        /// </summary>
        /// <returns></returns>
        public int GetSkipFrameCount()
        {
            if (!bNetInit)
                return 0;
            //本地队列差异高于服务器提前量的值
            int moreNum = mDiffFrameCount - mRemoteForwardCount;
            ////游戏刚开始的一小段时间，直接追满
            //if (mCurrClientFrameIdx < 60)
            //    return moreNum;

            int skip = 0;
            if (mDiffFrameCount > short.MaxValue) skip = 0;
            else if (moreNum <= mRemoteForwardCount) skip = 0;
            else if (moreNum <= mRemoteForwardCount + 1) skip = 0;
            else if (moreNum <= mRemoteForwardCount + 2) skip = 1;
            else if (moreNum <= mRemoteForwardCount + 3) skip = 2;
            else if (moreNum <= mRemoteForwardCount + 10) skip = moreNum / 2; //10帧以内，平滑跳帧数
            else skip = moreNum;//完全追上
            return skip;
        }
    }
}
