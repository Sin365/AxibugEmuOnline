using AxibugEmuOnline.Client.Input;
using MyNes.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class NetKeyMapper : KeyMapper
    {
        public override void Init()
        {
        }

        public override void Update()
        {
        }

        public override bool IsPressing(EnumKeyKind keyKind)
        {
            return false;
        }
    }
}
