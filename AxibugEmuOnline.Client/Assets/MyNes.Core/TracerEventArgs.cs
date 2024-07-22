using System;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
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
}
