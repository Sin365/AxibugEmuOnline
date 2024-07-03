namespace MyNes.Core
{
    public interface IVideoProvider
    {
    	string Name { get; }

    	string ID { get; }

    	void WriteErrorNotification(string message, bool instant);

    	void WriteInfoNotification(string message, bool instant);

    	void WriteWarningNotification(string message, bool instant);

    	void TakeSnapshotAs(string path, string format);

    	void TakeSnapshot();

    	void Initialize();

    	void ShutDown();

    	void SignalToggle(bool started);

    	void SubmitFrame(ref int[] buffer);

    	void ResizeBegin();

    	void ResizeEnd();

    	void ApplyRegionChanges();

    	void Resume();

    	void ToggleAspectRatio(bool keep_aspect);

    	void ToggleFPS(bool show_fps);

    	void ApplyFilter();
    }
}
