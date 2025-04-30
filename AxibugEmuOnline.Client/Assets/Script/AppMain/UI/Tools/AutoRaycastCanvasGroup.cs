using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 自动根据canvasgroup的alpha控制blocksRaycasts的开启状态
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class AutoRaycastCanvasGroup : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private void Update()
        {
            if (canvasGroup == null) canvasGroup = gameObject.GetComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = canvasGroup.alpha == 0 ? false : true;
        }
    }
}
