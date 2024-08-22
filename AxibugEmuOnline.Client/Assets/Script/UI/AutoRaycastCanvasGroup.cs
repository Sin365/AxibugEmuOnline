using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
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
