namespace Essgee.Emulation.Cartridges.Nintendo
{
    public interface IGameBoyCartridge : ICartridge
    {
        void SetCartridgeConfig(bool battery, bool rtc, bool rumble);
    }
}
