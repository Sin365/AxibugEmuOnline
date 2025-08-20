namespace MAME.Core
{
    public interface IVideoPlayer
    {
        void SubmitVideo(int[] data,long frame_number);
    }
}
