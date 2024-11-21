using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        private void Awake()
        {
            App.Init();
        }
    }
}
