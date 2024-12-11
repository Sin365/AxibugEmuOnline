using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class PopTipsUI : MonoBehaviour
    {
        [SerializeField] PopTipsItem m_itemTemplate;

        List<PopTipsItem> m_runtimeItems = new List<PopTipsItem>();

        const float StartY = -108f;
        const float StepY = -120;

        private void Awake()
        {
            m_itemTemplate.gameObject.SetActiveEx(false);
            m_runtimeItems.Add(m_itemTemplate);
        }

        public void Pop(string msg)
        {
            PopTipsItem item = GetPopItem();
            item.Popout(msg);
        }

        private PopTipsItem GetPopItem()
        {
            for (int i = 0; i < m_runtimeItems.Count; i++)
            {
                var target = m_runtimeItems[i];
                if (!target.gameObject.activeSelf)
                {
                    return target;
                }
            }

            var @new = Instantiate(m_itemTemplate.gameObject, m_itemTemplate.transform.parent).GetComponent<PopTipsItem>();
            m_runtimeItems.Add(@new);
            @new.RectTransform.anchoredPosition = new Vector2(0, StartY + (m_runtimeItems.Count - 1) * StepY);

            return @new;
        }
    }
}
