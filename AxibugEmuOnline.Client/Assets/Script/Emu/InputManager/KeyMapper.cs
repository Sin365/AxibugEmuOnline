using MyNes.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.Input
{
    public abstract class KeyMapper
    {
        public abstract void Init();
        public abstract void Update();
        public abstract bool IsPressing(EnumKeyKind keyKind);

    }
}
