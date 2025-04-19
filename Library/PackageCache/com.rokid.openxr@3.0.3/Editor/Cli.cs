using System;
using System.Diagnostics;

namespace Rokid.UXR.Editor
{
    static class Cli
    {
        public static (string stdout, string stderr, int exitCode) Execute(string fileName, string arguments)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException($"{nameof(fileName)} must not be null or empty.", nameof(fileName));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            using (process)
            {
                process.Start();
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return (stdout, stderr, process.ExitCode);
            }
        }
        
        public static (string stdout, string stderr, int exitCode) Execute(string fileName, string[] arguments = null) =>
            Execute(fileName, arguments == null ? null : string.Join(" ", arguments));
    }
}
