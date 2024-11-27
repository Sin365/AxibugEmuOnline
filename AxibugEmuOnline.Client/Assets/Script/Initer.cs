using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        [SerializeField]
        PostProcessVolume m_filterVolume;

        private void Awake()
        {
            App.Init(m_filterVolume);
        }
    }
}
