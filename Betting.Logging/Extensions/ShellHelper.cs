using System.Diagnostics;

namespace CsgoDraffle.Logging.Extensions
{
    public static class ShellHelper
    {
        public static void GetProcessFromBash(this string cmd, out Process process)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = "/bin/bash",
                    Arguments              = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                }
            };
            process.Start();
        }

        public static void BashSimple(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = "/bin/bash",
                    Arguments              = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                }
            };
            process.Start();
        }
    }
}