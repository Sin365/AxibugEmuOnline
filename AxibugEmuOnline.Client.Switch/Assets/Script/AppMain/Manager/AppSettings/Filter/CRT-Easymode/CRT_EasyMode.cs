using System.Collections.Generic;

namespace AxibugEmuOnline.Client.Filters
{
    public class CRT_EasyMode : FilterChainEffect
    {
        protected override void DefinePasses(ref List<PassDefine> passes)
        {
            passes.Add(PassDefine.Create(
                "Filter/crt-easymode", scaleModeX: EnumScaleMode.Viewport, scaleModeY: EnumScaleMode.Viewport
                ));
        }

        public override string Name => nameof(CRT_EasyMode);
    }
}