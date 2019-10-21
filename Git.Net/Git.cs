﻿using System;
using System.Diagnostics;

namespace Git.Net
{
    public static class Git
    {
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
            var startInfo = new ProcessStartInfo("git", $"init {name ?? ""}");
            Process.Start(startInfo).WaitForExit();
        }

        public static void Clone(string server, bool checkout = true)
        {
            var startInfo = new ProcessStartInfo("git", $"clone{(checkout ? " " : " -n ")}{server}");
            Process.Start(startInfo).WaitForExit();
        }

        public static void Commit(string message, DateTime date)
        {
            var startInfo = new ProcessStartInfo("git", $"commit -m \"{message}\" --allow-empty --date=\"{date.ToUniversalTime():R}\"");
            Process.Start(startInfo).WaitForExit();
        }

        public static void Commit(string message)
        {
            var startInfo = new ProcessStartInfo("git", $"commit -m \"{message}\" --allow-empty");
            Process.Start(startInfo).WaitForExit();
        }

        public static void Push(string? server = null, string? local = null, bool force = false)
        {
            var startInfo = new ProcessStartInfo("git", $"push --tags{(force ? " -f " : " ")}{server ?? "origin"} {local ?? "HEAD"}");
            Process.Start(startInfo).WaitForExit();
        }

        public static void AddTag(string label, bool force = false)
        {
            var startInfo = new ProcessStartInfo("git", $"tag{(force ? " -f " : " ")}{label}");
            Process.Start(startInfo).WaitForExit();
        }
    }
}
