using System.Diagnostics;

namespace BitBucket
{
    public class GitHandler
    {
        public async Task Clone(CancellationToken cancellationToken = default, string project = "NCPP", string repository = "units_pr3n.git", string branch = "support/amkw/v2.13.1", string destinationpath = @"D:\TestFolder")
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone -b {branch} ssh://git@bb.eu.besi.corp/scm/{project}/{repository} {destinationpath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                },
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine("[OUT] " + e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine("[PROGRESS] " + e.Data);
            };
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
        }
    }
}
