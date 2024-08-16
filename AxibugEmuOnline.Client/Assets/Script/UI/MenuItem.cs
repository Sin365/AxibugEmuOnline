using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MenuItem : MonoBehaviour
    {
        [SerializeField]
        Image Icon;
        [SerializeField]
        Text Txt;
        public Transform Root;

        public RectTransform Rect => transform as RectTransform;

        public void SetData(MainMenuData data)
        {
            Icon.sprite = data.Icon;
            Txt.text = data.Name;
        }

        public void ControlSelectProgress(float progress)
        {

        }
    }
}
