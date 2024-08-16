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
        [SerializeField]
        Transform Root;

        public float SelectScale = 1f;
        public float UnSelectScale = 0.85f;

        public RectTransform Rect => transform as RectTransform;

        public void SetData(MainMenuData data)
        {
            Icon.sprite = data.Icon;
            Txt.text = data.Name;
        }

        public void ControlSelectProgress(float progress)
        {
            var temp = Txt.color;
            temp.a = progress;
            Txt.color = temp;

            Root.localScale = Vector3.one * Mathf.Lerp(UnSelectScale, SelectScale, progress);
        }
    }
}
