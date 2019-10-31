using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Git.Net
{
    public static class Git
    {
        public static string Join(this IEnumerable<string?> list, string separator = " ")
            => string.Join(separator, list.Where(s => s is string));

        public static string Join(string?[] list1, bool switchListsOrder = false, params string?[] list2)
            => (switchListsOrder ? list2 : list1).Concat(switchListsOrder ? list1 : list2).Join(" ");

        public static string Join(params string?[] list)
            => list.Join(" ");
        
        private static ProcessStartInfo GetProcessStartInfo(string command, params string?[] parameters)
            => GetProcessStartInfo(command, Join(parameters));

        private static ProcessStartInfo GetProcessStartInfo(string command, string? parameters)
            => new ProcessStartInfo("git", command + (parameters is string p ? $" {p}" : ""));

        private static void StartAndWaitForExit(this ProcessStartInfo startInfo)
            => Process.Start(startInfo).WaitForExit();

        private static T? IsTrue<T>(this bool b, T @true)
            where T : class
            => b ? @true : default;
        
        private static T? IsFalse<T>(this bool b, T @false)
            where T : class
            => b ? default : @false;
        
        private static string? IsNotNull<T>(this T? @this, string format)
            where T : class
            => @this is T t ? string.Format(format, t) : default;
        
        private static string? IsNotNull<T>(this T? @this, string format)
            where T : struct
            => @this is T t ? string.Format(format, t) : default;

        public static string? GetLastCommit()
        {
            string? lastVersion = null;
            var startInfoLog = GetProcessStartInfo("log", "-1", "--oneline", "--no-decorate");
            startInfoLog.RedirectStandardOutput = true;
            using var processLog = new Process() { StartInfo = startInfoLog };
            processLog.OutputDataReceived += (s, e) =>
            {
                if (e.Data is string data && !data.StartsWith("fatal:"))
                    lastVersion = data;
            };
            processLog.Start();
            processLog.BeginOutputReadLine();
            processLog.WaitForExit();
            return lastVersion;
        }

        public static void AddAll()
            => GetProcessStartInfo("add", ".")
                .StartAndWaitForExit();

        public static void Init(string? name = null)
            => GetProcessStartInfo("init", name)
                .StartAndWaitForExit();

        public static void Clone(string server, bool checkout = true, string? branch = null, string? localDirectory = null)
            => GetProcessStartInfo("clone", checkout.IsFalse("-n"), branch.IsNotNull("-b {0}"), server, localDirectory)
                .StartAndWaitForExit();

        public static void Commit(string message, DateTime? date = null)
            => GetProcessStartInfo("commit", $"-m \"{message}\"", "--allow-empty", (date?.ToUniversalTime()).IsNotNull("--date=\"{0:R}\""))
                .StartAndWaitForExit();

        public static void Push(string? server = null, string? local = null, bool force = false)
            => GetProcessStartInfo("push", "--tags", force.IsTrue("-f"), server ?? "origin", local ?? "HEAD")
                .StartAndWaitForExit();

        public static void AddTag(string label, bool force = false)
            => GetProcessStartInfo("tag", force.IsTrue("-f"), label)
                .StartAndWaitForExit();
    }
}
