using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Git.Net
{
    public static partial class Git
    {
#region Helper

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

        private static ExitCode StartAndWaitForExit(this ProcessStartInfo startInfo)
        {
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode;
        }

        private static ExitCode StartAndWaitForExit(this ProcessStartInfo startInfo, out StreamReader stdOut)
        {
            startInfo.RedirectStandardOutput = true;
            Process process = Process.Start(startInfo);
            stdOut = process.StandardOutput;
            process.WaitForExit();
            return process.ExitCode;
        }

        private static ExitCode WaitForExitAndGetLastCommit(this ProcessStartInfo startInfo, out Ref @ref)
        {
            var result = startInfo.StartAndWaitForExit(out var stdOut);

            if(result)
            {
                var line = stdOut.ReadToEnd();
                System.Console.WriteLine(line);
                var split = line.Split(' ');
                var split1 = split[1];
                @ref = ulong.Parse(split1[..^1], NumberStyles.HexNumber);
            }
            else
                @ref = default;
            return result;
        }

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
        
        private static string InQuote(this string value)
            => @"""{value}""";

#endregion

        public static (ulong hash, string message)? GetLastCommit()
        {
            string? lastVersion = null;
            var startInfoLog = GetProcessStartInfo("log", "-1", "--oneline", "--no-decorate");
            startInfoLog.RedirectStandardOutput = true;
            using var processLog = new Process() { StartInfo = startInfoLog };
            processLog.OutputDataReceived += (s, e) =>
            {
                if (e.Data is string data)
                    lastVersion = data;
            };
            processLog.Start();
            processLog.BeginOutputReadLine();
            processLog.WaitForExit();
            if((ExitCode)processLog.ExitCode && lastVersion is string last)
            {
                var split = last.Split(' ');
                if(split.Length >= 2)
                    return (ulong.Parse(split[0], NumberStyles.HexNumber), string.Join(' ', split[1..]));
            }
            return null;
        }

#region Add

            public static ExitCode Add(bool all = true)
            => GetProcessStartInfo("add", all.IsTrue("."))
                .StartAndWaitForExit();

        public static ExitCode Add(params string[] files)
            => GetProcessStartInfo("add", files)
                .StartAndWaitForExit();

#endregion

        public static ExitCode Init(string? name = null)
            => GetProcessStartInfo("init", name)
                .StartAndWaitForExit();

        public static ExitCode Clone(string server, bool checkout = true, string? branch = null, string? localDirectory = null)
            => GetProcessStartInfo("clone", checkout.IsFalse("-n"), branch.IsNotNull("-b {0}"), server, localDirectory)
                .StartAndWaitForExit();

        public static ExitCode Commit(string message, out Ref @ref, bool addAll = false, bool amend = false, bool allowEmpty = false, DateTime? date = null)
            => GetProcessStartInfo("commit", $"-m \"{message}\"", addAll.IsTrue("--all"), amend.IsTrue("--amend"), allowEmpty.IsTrue("--allow-empty"), (date?.ToUniversalTime()).IsNotNull("--date=\"{0:R}\""))
                .WaitForExitAndGetLastCommit(out @ref);

        public static ExitCode Commit(string message, bool addAll = false, bool amend = false, bool allowEmpty = false, DateTime? date = null)
            => GetProcessStartInfo("commit", $"-m \"{message}\"", addAll.IsTrue("--all"), amend.IsTrue("--amend"), allowEmpty.IsTrue("--allow-empty"), (date?.ToUniversalTime()).IsNotNull("--date=\"{0:R}\""))
                .StartAndWaitForExit();
        
        public static ExitCode StartConfig(string username, string email, bool local = false)
        {
            var result = GetProcessStartInfo("config", local.IsTrue("--global"), "user.name", username.InQuote()).StartAndWaitForExit();
            if(result)
                return GetProcessStartInfo("config", local.IsTrue("--global"), "user.email", email.InQuote()).StartAndWaitForExit();
            return result;
        }

#region Reset

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

        public static ExitCode Reset(Ref refIn, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), @refIn)
                .StartAndWaitForExit();
        
        public static ExitCode Reset(params string[] files)
            => GetProcessStartInfo("reset --", files)
                .StartAndWaitForExit();

        public static ExitCode Reset(Ref refIn, out Ref @refOut, ResetMode mode = ResetMode.None)
            => GetProcessStartInfo("reset", mode.ToParameter(), @refIn)
                .WaitForExitAndGetLastCommit(out @refOut);
        
        public static ExitCode Reset(out Ref @ref, params string[] files)
            => GetProcessStartInfo("reset --", files)
                .WaitForExitAndGetLastCommit(out @ref);

#endregion

#region Push

        public static ExitCode Push(bool tags = false, bool force = false, string? server = null, string? local = null)
            => GetProcessStartInfo("push", tags.IsTrue("--tags"), force.IsTrue("-f"), server, local)
                .StartAndWaitForExit();

        public static ExitCode Push(string server, string local, bool setUpstream = false, bool tags = false, bool force = false)
            => GetProcessStartInfo("push", tags.IsTrue("--tags"), force.IsTrue("-f"), setUpstream.IsTrue("-u"), server, local)
                .StartAndWaitForExit();

#endregion

#region Tag

        public static ExitCode Tag(string label, Ref @ref = default, bool force = false, bool delete = false)
            => GetProcessStartInfo("tag", force.IsTrue("-f"), delete.IsTrue("-d"), label, @ref)
                .StartAndWaitForExit();

        public static ExitCode Tag(string label, string message, Ref @ref = default, bool force = false)
            => GetProcessStartInfo("tag", force.IsTrue("-f"), $"-m {message}", label, @ref)
                .StartAndWaitForExit();

#endregion
    }
}
