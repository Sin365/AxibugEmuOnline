using System;

namespace MyNes.Core;

public class TracerEventArgs : EventArgs
{
	public string Message { get; private set; }

	public TracerStatus Status { get; private set; }

	public TracerEventArgs(string message, TracerStatus status)
	{
		Message = message;
		Status = status;
	}
}
