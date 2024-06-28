using UnityEngine;
using AxibugEmuOnline.Client.UNES;
using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client.Sample
{
    public class UNESTest : MonoBehaviour
    {
        public string RomFile;

        public bool PlayerP1;

        public static UNESTest instance;
        public UNESBehaviour UNES { get; set; }

        public void Awake()
        {
            Screen.SetResolution(640, 480,false);

            instance = this;
            AppAxibugEmuOnline.Init();
            AppAxibugEmuOnline.Connect("127.0.0.1", 10492);
        }

        public void Start()
        {
            UNES = GetComponent<UNESBehaviour>();
            if (PlayerP1)
            {
                var data = Resources.Load<TextAsset>(RomFile).bytes;
                UNES.Boot(data);
            }
            else
            {
                UNES.Boot_Obs();
            }
        }

        public void OnDisable()
        {
            AppAxibugEmuOnline.Close();
        }

    }
}
