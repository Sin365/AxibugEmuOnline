using AxibugProtobuf;

/// <summary>
/// 此类为内部继承, 请勿继承此类
/// </summary>
public abstract class InternalEmuCoreBinder
{
    /// <summary> 所属核心 </summary>
    public abstract RomPlatformType Platform { get; }
    /// <summary> 控制器数量 </summary>
    public abstract int ControllerCount { get; }
}