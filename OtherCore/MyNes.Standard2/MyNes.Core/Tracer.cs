#define TRACE
using System;

namespace MyNes.Core
{
    public sealed class Tracer
    {
        public static event EventHandler<TracerEventArgs> EventRaised;

        public static void WriteLine(string message)
        {
            Tracer.EventRaised?.Invoke(null, new TracerEventArgs(message, TracerStatus.Normal));
            //Debug.Log(message);
        }

        public static void WriteLine(string message, string category)
        {
            Tracer.EventRaised?.Invoke(null, new TracerEventArgs($"{category}: {message}", TracerStatus.Normal));
            //Debug.Log(message);
        }

        public static void WriteLine(string message, TracerStatus status)
        {
            Tracer.EventRaised?.Invoke(null, new TracerEventArgs(message, status));
            switch (status)
            {
                case TracerStatus.Error: WriteLine(message); break;
                case TracerStatus.Infromation:
                case TracerStatus.Normal:
                    WriteLine(message);
                    break;
                case TracerStatus.Warning:
                    WriteLine(message);
                    break;
            }

        }

        public static void WriteLine(string message, string category, TracerStatus status)
        {
            Tracer.EventRaised?.Invoke(null, new TracerEventArgs($"{category}: {message}", status));
            WriteLine($"{category}:{message}", status);
        }

        public static void WriteError(string message)
        {
            WriteLine(message, TracerStatus.Error);
        }

        public static void WriteError(string message, string category)
        {
            WriteLine(message, category, TracerStatus.Error);
        }

        public static void WriteWarning(string message)
        {
            WriteLine(message, TracerStatus.Warning);
        }

        public static void WriteWarning(string message, string category)
        {
            WriteLine(message, category, TracerStatus.Warning);
        }

        public static void WriteInformation(string message)
        {
            WriteLine(message, TracerStatus.Infromation);
        }

        public static void WriteInformation(string message, string category)
        {
            WriteLine(message, category, TracerStatus.Infromation);
        }
    }
}
