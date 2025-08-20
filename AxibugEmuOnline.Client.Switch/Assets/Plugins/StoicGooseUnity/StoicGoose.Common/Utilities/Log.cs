using System.Collections.Generic;

namespace StoicGoose.Common.Utilities
{
    public enum LogSeverity { Verbose, Debug, Information, Warning, Error, Fatal }
    public enum LogType { Debug, Warning, Error }

    public interface IStoicGooseLogger
    {
        public void Log(LogType logtype, string message);
        public void Debug(string message);
        public void Warning(string message);
        public void Err(string message);
    }

    public static class Log
	{
		const string defaultTemplate = "{Message}{NewLine}{Exception}";

		readonly static Dictionary<LogSeverity, LogType> severityToEventLevelMapping = new()
		{
			{ LogSeverity.Verbose, LogType.Debug },
			{ LogSeverity.Debug, LogType.Debug },
			{ LogSeverity.Information, LogType.Debug },
			{ LogSeverity.Warning, LogType.Warning },
			{ LogSeverity.Error, LogType.Error },
			{ LogSeverity.Fatal, LogType.Error }
		};

		readonly static Dictionary<LogSeverity, string> logSeverityAnsiColors = new()
		{
			{ LogSeverity.Verbose, Ansi.White },
			{ LogSeverity.Debug, Ansi.Cyan },
			{ LogSeverity.Information, Ansi.Green },
			{ LogSeverity.Warning, Ansi.Yellow },
			{ LogSeverity.Error, Ansi.Magenta },
			{ LogSeverity.Fatal, Ansi.Red }
		};

		static IStoicGooseLogger mainLogger;

		public static void Initialize(IStoicGooseLogger logger)
		{
			mainLogger = logger;
		}


		public static void WriteLine(string message) => mainLogger.Debug(message);
		//public static void WriteFatal(string message) => Write(LogEventLevel.Fatal, message);

		//private static void Write(LogEventLevel logEventLevel, string message)
		//{
		//	mainLogger?.Write(logEventLevel, message);
		//	fileLogger?.Write(logEventLevel, message.RemoveAnsi());
		//}

		public static void WriteEvent(LogSeverity severity, object source, string message)
		{
			var eventLevel = severityToEventLevelMapping.ContainsKey(severity) ? severityToEventLevelMapping[severity] : LogType.Debug;
			var logMessage = $"{logSeverityAnsiColors[severity]}[{source?.GetType().Name ?? string.Empty}]{Ansi.Reset}: {message}";
			mainLogger.Log(eventLevel, logMessage);
		}
	}

	//class TextWriterSink : ILogEventSink
	//{
	//	readonly TextWriter textWriter = default;
	//	readonly ITextFormatter textFormatter = default;

	//	readonly object syncRoot = new();

	//	//public TextWriterSink(TextWriter writer, ITextFormatter formatter)
	//	//{
	//	//	textWriter = writer;
	//	//	textFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
	//	//}

	//	public void Emit(LogEvent logEvent)
	//	{
	//		lock (syncRoot)
	//		{
	//			textFormatter.Format(logEvent ?? throw new ArgumentNullException(nameof(logEvent)), textWriter);
	//			textWriter.Flush();
	//		}
	//	}
	//}
}
