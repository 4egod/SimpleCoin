using System;
using System.Diagnostics;

namespace XSC
{
    using XSC.Core;

    public static class Logger
    {
        public const string Extension = ".log";

        public static string Path { get; private set; }

        public static bool IsStarted { get; private set; }

        public static void Start(string address)
        {
            Debug.Listeners.Add(new ConsoleTraceListener(true));

            Path = Config.LogsDirectory + address + Extension;
            Debug.Listeners.Add(new TextWriterTraceListener(Path));
            Debug.AutoFlush = true;

            IsStarted = true;
        }

        public static void WriteLine(object message)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("MM.dd-HH:mm:ss")} {message}");
        }

        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("MM.dd-HH:mm:ss")} {string.Format(format, args)}");
        }
    }
}
