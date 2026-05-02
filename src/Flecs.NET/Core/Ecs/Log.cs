using System;
using System.IO;
using System.Runtime.CompilerServices;
using static Flecs.NET.Bindings.flecs;

namespace Flecs.NET.Core;

public static unsafe partial class Ecs
{
    /// <summary>
    ///     Static class for flecs logging functions.
    /// </summary>
    public static class Log
    {
        private static readonly object Lock = new();
        private static int _level = -1;
        private static int _indent;
        private static bool _timestamp;
        private static bool _timeDelta;
        private static long _lastTimestamp;

        private static void Message(int level, string message, string file, int line)
        {
            if (level > _level)
                return;

            if (Os.Context.Log != default)
            {
                if (Os.Context.Log.Delegate.IsAllocated)
                {
                    ((LogCallback)Os.Context.Log.Delegate.Target!)(level, file, line, message);
                    return;
                }

                ((delegate*<int, string, int, string, void>)Os.Context.Log.Pointer)(level, file, line, message);
                return;
            }

            Write(level, message, file, line);
        }

        private static void Write(int level, string message, string file, int line)
        {
            TextWriter writer = Console.Out;

            if (_timeDelta)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long delta = _lastTimestamp == 0 ? 0 : now - _lastTimestamp;
                _lastTimestamp = now;
                writer.Write(delta == 0 ? "     " : $"+{delta,3} ");
            }

            if (_timestamp)
                writer.Write($"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()} ");

            writer.Write(GetLabel(level));
            writer.Write(": ");

            if (level >= 0 && _indent > 0)
            {
                int indent = Math.Min(_indent, 15);
                for (int i = 0; i < indent; i++)
                    writer.Write("| ");
            }

            if (level < 0)
            {
                if (!string.IsNullOrEmpty(file))
                {
                    writer.Write(Path.GetFileName(file));
                    writer.Write(": ");
                }

                if (line != 0)
                {
                    writer.Write(line);
                    writer.Write(": ");
                }
            }

            writer.WriteLine(message);
        }

        private static string GetLabel(int level)
        {
            if (level >= 4)
                return "jrnl";
            if (level >= 0)
                return "info";
            return level switch
            {
                -2 => "warning",
                -3 => "error",
                -4 => "fatal",
                _ => "info"
            };
        }

        /// <summary>
        ///     Set log level.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLevel(int level)
        {
            lock (Lock)
            {
                _level = level;
                _ = ecs_log_set_level(Math.Min(level, -1));
            }
        }

        /// <summary>
        ///     Get log level.
        /// </summary>
        /// <returns></returns>
        public static int GetLevel()
        {
            lock (Lock)
                return _level;
        }

        /// <summary>
        ///     Enable colors in logging.
        /// </summary>
        /// <param name="enabled"></param>
        public static void EnableColors(bool enabled = true)
        {
            lock (Lock)
                ecs_log_enable_colors(enabled);
        }

        /// <summary>
        ///     Enable timestamps in logging.
        /// </summary>
        /// <param name="enabled"></param>
        public static void EnableTimestamp(bool enabled = true)
        {
            lock (Lock)
            {
                _timestamp = enabled;
                ecs_log_enable_timestamp(enabled);
            }
        }

        /// <summary>
        ///     Enable time delta in logging.
        /// </summary>
        /// <param name="enabled"></param>
        public static void EnableTimeDelta(bool enabled = true)
        {
            lock (Lock)
            {
                _timeDelta = enabled;
                ecs_log_enable_timedelta(enabled);
            }
        }

        /// <summary>
        ///     Debug trace (Level 1)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        public static void Dbg(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            lock (Lock)
                Message(1, message, file, line);
        }

        /// <summary>
        ///     Trace (Level 0)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        public static void Trace(string message = "", [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            lock (Lock)
                Message(0, message, file, line);
        }

        /// <summary>
        ///     Trace (Level -2)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        public static void Warn(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            lock (Lock)
                Message(-2, message, file, line);
        }

        /// <summary>
        ///     Trace (Level -3)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        public static void Err(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            lock (Lock)
                Message(-3, message, file, line);
        }

        /// <summary>
        ///     Trace (Level 0)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        public static void Push(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            lock (Lock)
            {
                Message(1, message, file, line);
                _indent++;
            }
        }

        /// <summary>
        ///     Increase log indentation.
        /// </summary>
        public static void Push()
        {
            lock (Lock)
                _indent++;
        }

        /// <summary>
        ///     Decrease log indentation.
        /// </summary>
        public static void Pop()
        {
            lock (Lock)
                _indent = Math.Max(0, _indent - 1);
        }
    }
}
