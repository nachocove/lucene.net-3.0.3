using System;

namespace Lucene.Cli
{
    public class Log
    {
        public static bool DebugEnabled = false;

        private static void _Log (string fmt, params object[] args)
        {
            Console.WriteLine (fmt, args);
        }

        public static void Info (string fmt, params object[] args)
        {
            _Log ("[INFO] " + fmt, args);
        }

        public static void Debug (string fmt, params object[] args)
        {
            if (DebugEnabled) {
                _Log ("[DEBUG] " + fmt, args);
            }
        }

        public static void Warn (string fmt, params object[] args)
        {
            _Log ("[WARN] " + fmt, args);
        }

        public static void Error (string fmt, params object[] args)
        {
            _Log ("[ERROR] " + fmt, args);
        }
    }
}

