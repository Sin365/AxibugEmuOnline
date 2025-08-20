using StoicGooseUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SGKeyboard : MonoBehaviour
{
    Dictionary<KeyCode, StoicGooseKey> dictKey2SGKey = new Dictionary<KeyCode, StoicGooseKey>();
    KeyCode[] checkKeys;
    long currInput;
    private void Awake()
    {
        SetVerticalOrientation(false);
    }


    internal void PollInput(ref long buttonsHeld)
    {
        buttonsHeld = currInput;
    }

    internal void SetVerticalOrientation(bool isVerticalOrientation)
    {
        dictKey2SGKey[KeyCode.Return] = StoicGooseKey.Start;
        dictKey2SGKey[KeyCode.W] = StoicGooseKey.X1;
        dictKey2SGKey[KeyCode.S] = StoicGooseKey.X2;
        dictKey2SGKey[KeyCode.A] = StoicGooseKey.X3;
        dictKey2SGKey[KeyCode.D] = StoicGooseKey.X4;
        dictKey2SGKey[KeyCode.G] = StoicGooseKey.Y1;
        dictKey2SGKey[KeyCode.V] = StoicGooseKey.Y2;
        dictKey2SGKey[KeyCode.C] = StoicGooseKey.Y3;
        dictKey2SGKey[KeyCode.B] = StoicGooseKey.Y4;
        dictKey2SGKey[KeyCode.Return] = StoicGooseKey.Start;
        dictKey2SGKey[KeyCode.J] = StoicGooseKey.B;
        dictKey2SGKey[KeyCode.K] = StoicGooseKey.A;
        checkKeys = dictKey2SGKey.Keys.ToArray();
    }

    public void Update_InputData()
    {
        currInput = 0;
        for (int i = 0; i < checkKeys.Length; i++)
        {
            KeyCode key = checkKeys[i];
            if (Input.GetKey(key))
            {
                currInput |= (long)dictKey2SGKey[key];
            }
        }
    }
}