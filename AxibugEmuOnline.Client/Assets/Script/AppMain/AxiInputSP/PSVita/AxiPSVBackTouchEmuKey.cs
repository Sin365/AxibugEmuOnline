/// 
/// Axibug PSVita 背面触摸板模拟触摸（by 皓月）
/// 
using UnityEngine;
using System.Collections.Generic;

namespace AxiInputSP
{
    public enum AxiPSVBackTouchType : byte
    {
        LeftHalf,//左半触摸板
        RigthHalf,//右半触摸板
        LeftTop,//左上部分触摸板
        LeftBotton,//左下部分触摸板
        RightTop,//右上部分触摸板
        RightBotton,//右下部分触摸板
    }
    public class AxiPSVBackTouchEmuKey : MonoBehaviour
    {
        public static bool GetKey(AxiPSVBackTouchType btnType) { return _instance.m_TouckState[btnType].GetKey(); }
        public static bool GetKeyUp(AxiPSVBackTouchType btnType) { return _instance.m_TouckState[btnType].GetKeyUp(); }
        public static bool GetKeyDown(AxiPSVBackTouchType btnType) { return _instance.m_TouckState[btnType].GetKeyDown(); }

        #region 静态管理
        static AxiPSVBackTouchEmuKey instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("[AxiPSVBackTouch]").AddComponent<AxiPSVBackTouchEmuKey>();
                    GameObject.DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
        static AxiPSVBackTouchEmuKey _instance;
        const int PSVScreenWidth = 960;
        const int PSVScreenHeight = 544;
        static Dictionary<AxiPSVBackTouchType, System.ValueTuple<Vector2, Vector2>> dictType2Range = new Dictionary<AxiPSVBackTouchType, (Vector2, Vector2)>()
        {
            { AxiPSVBackTouchType.LeftHalf,new System.ValueTuple<Vector2,Vector2>(Vector2.zero,new Vector2(PSVScreenWidth*0.5f,PSVScreenHeight* 1f))},
            { AxiPSVBackTouchType.RigthHalf,new System.ValueTuple<Vector2,Vector2>(new Vector2(PSVScreenWidth*0.5f,0),new Vector2(PSVScreenWidth*1f,PSVScreenHeight * 1f))},
            { AxiPSVBackTouchType.LeftTop,new System.ValueTuple<Vector2,Vector2>(new Vector2(0,PSVScreenHeight*0.5f),new Vector2(PSVScreenWidth*0.5f,PSVScreenHeight * 1f))},
            { AxiPSVBackTouchType.LeftBotton,new System.ValueTuple<Vector2,Vector2>(new Vector2(0,PSVScreenHeight*0f),new Vector2(PSVScreenWidth*0.5f,PSVScreenHeight* 0.5f))},
            { AxiPSVBackTouchType.RightTop,new System.ValueTuple<Vector2,Vector2>(new Vector2(PSVScreenWidth*0.5f,PSVScreenHeight*0.5f),new Vector2(PSVScreenWidth*1f,PSVScreenHeight* 1f))},
            { AxiPSVBackTouchType.RightBotton,new System.ValueTuple<Vector2,Vector2>(new Vector2(0,PSVScreenHeight*0.5f),new Vector2(PSVScreenWidth*1f,PSVScreenHeight* 0.5f))},
        };
        #endregion


        #region Mono驱动
        Dictionary<AxiPSVBackTouchType, AxiPSVBackTouchState> m_TouckState = new Dictionary<AxiPSVBackTouchType, AxiPSVBackTouchState>();
        List<Vector2> m_PSVTouchPosList = new List<Vector2>();
        private void Awake()
        {
            m_TouckState[AxiPSVBackTouchType.LeftHalf] = new AxiPSVBackTouchState(AxiPSVBackTouchType.LeftHalf);
            m_TouckState[AxiPSVBackTouchType.RigthHalf] = new AxiPSVBackTouchState(AxiPSVBackTouchType.RigthHalf);
            m_TouckState[AxiPSVBackTouchType.LeftTop] = new AxiPSVBackTouchState(AxiPSVBackTouchType.LeftTop);
            m_TouckState[AxiPSVBackTouchType.LeftBotton] = new AxiPSVBackTouchState(AxiPSVBackTouchType.LeftBotton);
            m_TouckState[AxiPSVBackTouchType.RightTop] = new AxiPSVBackTouchState(AxiPSVBackTouchType.RightTop);
            m_TouckState[AxiPSVBackTouchType.RightBotton] = new AxiPSVBackTouchState(AxiPSVBackTouchType.RightBotton);
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            _instance = null;
        }

        private void Update()
        {
            m_PSVTouchPosList.Clear();

#if UNITY_PSP2 && !UNITY_EDITOR
            for (int i = 0; i < PSVitaInput.touchesSecondary.Length; i++)
            {
                Touch touch = PSVitaInput.touchesSecondary[i];
                m_PSVTouchPosList.Add(touch.position);
            }
#endif
            //收集按下的区域类型
            foreach (var pos in m_PSVTouchPosList)
            {
                foreach (var rangeKV in dictType2Range)
                {
                    bool bIsIn = (rangeKV.Value.Item2.x < pos.x && pos.x < rangeKV.Value.Item2.x
                        &&
                        rangeKV.Value.Item1.x < pos.y && pos.y < rangeKV.Value.Item1.y);

                    if (bIsIn)
                        m_TouckState[rangeKV.Key].UpdateStateForIn();
                    else
                        m_TouckState[rangeKV.Key].UpdateStateForOut();
                }
            }

        }
        #endregion
    }
}
