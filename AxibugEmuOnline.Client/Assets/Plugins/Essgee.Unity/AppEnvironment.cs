public static class AppEnvironment
{
#if DEBUG
    public static readonly bool DebugMode = true;
#else
			public static readonly bool DebugMode = false;
#endif
    public static readonly bool EnableCustomUnhandledExceptionHandler = true;
    public static readonly bool TemporaryDisableCustomExceptionForm = false;

    public static readonly bool EnableLogger = false;
    public static readonly bool EnableSuperSlowCPULogger = false;

    public static readonly bool EnableOpenGLDebug = false;
}