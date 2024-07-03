namespace MyNes.Core
{
    internal delegate void MemReadAccess(ref ushort addr, out byte value);
}
