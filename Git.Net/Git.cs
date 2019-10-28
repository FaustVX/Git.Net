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

        public static string Join(params string?[] list)
            => list.Join(" ");

        public static string? GetLastCommit()
        {
            string? lastVersion = null;
            var startInfoLog = new ProcessStartInfo("git", "log -1 --oneline --no-decorate") { RedirectStandardOutput = true };
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
        {
            var startInfo = new ProcessStartInfo("git", "add .");
            Process.Start(startInfo).WaitForExit();
        }

        public static void Init(string? name = null)
        {
            var startInfo = new ProcessStartInfo("git", Join("init", name));
            Process.Start(startInfo).WaitForExit();
        }

        public static void Clone(string server, bool checkout = true, string? branch = null, string? localDirectory = null)
        {
            var startInfo = new ProcessStartInfo("git", Join("clone", checkout ? null : "-n", branch is string b ? $"-b {b}" : null, server, localDirectory));
            Process.Start(startInfo).WaitForExit();
        }

        public static void Commit(string message, DateTime? date = null)
        {
            var startInfo = new ProcessStartInfo("git", Join("commit", $"-m \"{message}\"", "--allow-empty", date is DateTime d ? $"--date=\"{d.ToUniversalTime():R}\"" : null));
            Process.Start(startInfo).WaitForExit();
        }

        public static void Push(string? server = null, string? local = null, bool force = false)
        {
            var startInfo = new ProcessStartInfo("git", Join($"push", "--tags", force ? "-f" : null, server ?? "origin", local ?? "HEAD"));
            Process.Start(startInfo).WaitForExit();
        }

        public static void AddTag(string label, bool force = false)
        {
            var startInfo = new ProcessStartInfo("git", Join("tag", force ? "-f" : null, label));
            Process.Start(startInfo).WaitForExit();
        }
    }
}
