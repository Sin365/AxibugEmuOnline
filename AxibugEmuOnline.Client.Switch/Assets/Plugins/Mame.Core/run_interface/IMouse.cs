namespace MAME.Core
{
    public interface IMouse
    {
        void MouseXY(out int X, out int Y, out byte[] MouseButtons);
    }
}
