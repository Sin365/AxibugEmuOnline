namespace MyNes.Core
{
    public interface IAudioProvider
    {
    	string Name { get; }

    	string ID { get; }

    	bool AllowBufferChange { get; }

    	bool AllowFrequencyChange { get; }

    	void SubmitSamples(ref short[] buffer, ref int samples_added);

    	void TogglePause(bool paused);

    	void GetIsPlaying(out bool playing);

    	void Initialize();

    	void ShutDown();

    	void Reset();

    	void SignalToggle(bool started);

    	void SetVolume(int Vol);

        void Update();
    }
}
