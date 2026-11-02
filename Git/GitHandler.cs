using System.Diagnostics;
using System.Text;

namespace Git
{
    public class GitHandler
    {
        private string _host;
        public GitHandler(string host)
        {
            _host = host;
        }

        public async Task<string> CheckoutTag(
            string workingDirectory,
            string tag,
            CancellationToken ct = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"checkout tags/{tag}"
                }
            };

            process.Start();

            await process.WaitForExitAsync(ct);

            return process.ExitCode == 0
                ? string.Empty
                : "Checkout tag failed";
        }

        public async Task<string> Clone(
            string project,
            string repository,
            string branchtag,
            string destinationPath,
            bool singlebranch = true,
            CancellationToken cancellationToken = default)
        {
            var output = new StringBuilder();
            var sync = new object();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("clone");
            process.StartInfo.ArgumentList.Add("--branch");
            process.StartInfo.ArgumentList.Add(branchtag);

            if (singlebranch)
                process.StartInfo.ArgumentList.Add("--single-branch");

            process.StartInfo.ArgumentList.Add(
                $"ssh://{_host}/{project}/{repository}.git");

            process.StartInfo.ArgumentList.Add(destinationPath);

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    lock (sync)
                        output.AppendLine("[OUT] " + e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    lock (sync)
                        output.AppendLine("[ERR] " + e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
                process.WaitForExit(); // Ensure async readers are flushed
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill(true);

                throw;
            }

            if (process.ExitCode != 0)
            {
                return $"Git clone failed with exit code {process.ExitCode}{Environment.NewLine}{output}";
            }

            return string.Empty;
        }

        public async Task<string> CloneBK(
            string project,
            string repository,
            string branch,
            string destinationPath,
            bool singlebranch = true,
            CancellationToken cancellationToken = default)
        {
            var ErrMessage = string.Empty;
            var singlebranchcmd = singlebranch ? "--single-branch" : "";
            var arguments = $"clone --branch {branch} {singlebranchcmd} ssh://{_host}/{project}/{repository}.git \"{destinationPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ErrMessage += "[OUT] " + e.Data + "\n\r";
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    ErrMessage += "[ERR] " + e.Data + "\n\r";
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                return ($"Git clone failed with exit code {process.ExitCode}, {ErrMessage}");
            }
            return string.Empty;
        }

        public async Task<bool> BranchExists(
            string project,
            string repository,
            string branch,
            CancellationToken cancellationToken = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("ls-remote");
            process.StartInfo.ArgumentList.Add("--heads");
            process.StartInfo.ArgumentList.Add($"ssh://{_host}/{project}/{repository}.git");
            process.StartInfo.ArgumentList.Add(branch);

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            string error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                throw new Exception($"git ls-remote failed: {error}");

            return !string.IsNullOrWhiteSpace(output);
        }
    }
}
