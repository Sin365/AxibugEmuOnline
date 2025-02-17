namespace Essgee.Emulation.Cartridges.Nintendo
{
    internal interface IGameBoyCartridge : ICartridge
    {
        void SetCartridgeConfig(bool battery, bool rtc, bool rumble);
    }
}
