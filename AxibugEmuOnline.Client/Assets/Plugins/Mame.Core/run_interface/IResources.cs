namespace MAME.Core
{
    public interface IResources
    {
        byte[] mcu { get; }
        byte[] sfix { get; }
        byte[] _000_lo { get; }
        byte[] sm1 { get; }
        byte[] mainbios { get; }
        byte[] pgmmainbios { get; }
        byte[] pgmvideobios { get; }
        byte[] pgmaudiobios { get; }
        byte[] _1 { get; }
        byte[] readme { get; }
        string mame { get; }
    }
}
