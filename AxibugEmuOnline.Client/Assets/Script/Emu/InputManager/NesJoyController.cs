using MyNes.Core;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace AxibugEmuOnline.Client
{
    public class NesJoyController : IJoypadConnecter
    {
        private EnumJoyIndex m_joyIndex;
        private bool turbo;

        public NesJoyController(EnumJoyIndex joyIndex)
        {
            m_joyIndex = joyIndex;
        }
        public override void Update()
        {
            turbo = !turbo;
            DATA = 0;
            var state = MyNesMain.Supporter;
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.A))
            {
                DATA |= 1;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.B))
            {
                DATA |= 2;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.TurboA) && turbo)
            {
                DATA |= 1;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.TurboB) && turbo)
            {
                DATA |= 2;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Select))
            {
                DATA |= 4;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Start))
            {
                DATA |= 8;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Up))
            {
                DATA |= 16;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Down))
            {
                DATA |= 32;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Left))
            {
                DATA |= 64;
            }
            if (state.IsKeyPressing(m_joyIndex, EnumKeyKind.Right))
            {
                DATA |= 128;
            }
        }
    }
}
