using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Git.Net
{
    public static class Git
    {
        public enum ResetMode
        {
            None,
            Soft,
            Mixed,
            Hard,
            Merge,
            Keep
        }

        private static string? ToParameter(this ResetMode mode)
        {
            return mode switch
            {
                ResetMode.None => null,
                ResetMode.Soft => "--soft",
                ResetMode.Mixed => "--mixed",
                ResetMode.Hard => "--hard",
                ResetMode.Merge => "--merge",
                ResetMode.Keep => "--keep"
            };
        }

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

        public static void Add(bool all = true)
            => GetProcessStartInfo("add", all.IsTrue("."))
                .StartAndWaitForExit();

        public static void Add(params string[] files)
            => GetProcessStartInfo("add", files)
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

        ///<summary>Use the '^1' form</summary>
        ///<param name="indexFrom">Use the '^1' form</param>
        ///<param name="ref">tag/branch/commit</param>
        public static void Reset(Index indexFrom, string @ref, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), $"{@ref ?? "HEAD"}~{indexFrom.Value}")
                .StartAndWaitForExit();

        ///<summary>Use the '^1' form</summary>
        ///<param name="indexFromHEAD">Use the '^1' form</param>
        public static void Reset(Index indexFromHEAD, ResetMode mode = ResetMode.None)
            => Reset(indexFromHEAD, "HEAD", mode);

        ///<summary>Use the '0x14ab250' form, up to 0x7fffffff</summary>
        ///<param name="sha1Commit">Use the '0x14ab250' form, up to 0x7fffffff</param>
        public static void Reset(int sha1Commit, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), $"{sha1Commit:x}")
                .StartAndWaitForExit();

        ///<summary>Use the '0x14ab250uL' form, up to 16 digits</summary>
        ///<param name="sha1Commit">Use the '0x14ab250uL' form, up to 16 digits</param>
        public static void Reset(ulong sha1Commit, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), $"{sha1Commit:x}")
                .StartAndWaitForExit();

        ///<summary>tag/branch/commit</summary>
        ///<param name="ref">tag/branch/commit</param>
        public static void Reset(string @ref, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), @ref)
                .StartAndWaitForExit();
        
        public static void Reset(params string[] files)
            => GetProcessStartInfo("reset --", files)
                .StartAndWaitForExit();

        public static void Push(bool tags = false, bool force = false, string? server = null, string? local = null)
            => GetProcessStartInfo("push", tags.IsTrue("--tags"), force.IsTrue("-f"), server, local)
                .StartAndWaitForExit();

        public static void Push(string server, string local, bool setUpstream = false, bool tags = false, bool force = false)
            => GetProcessStartInfo("push", tags.IsTrue("--tags"), force.IsTrue("-f"), setUpstream.IsTrue("-u"), server, local)
                .StartAndWaitForExit();

        public static void Tag(string label, bool force = false, bool delete = false)
            => GetProcessStartInfo("tag", force.IsTrue("-f"), delete.IsTrue("-d"), label)
                .StartAndWaitForExit();

        public static void Tag(string label, string message, bool force = false)
            => GetProcessStartInfo("tag", force.IsTrue("-f"), $"-m {message}", label)
                .StartAndWaitForExit();
    }
}
