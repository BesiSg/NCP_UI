using System.IO;
using System.IO.Compression;
using Utility;

namespace Utility.PatchSync
{
    public class PatchSync : BaseUtility
    {
        public event EventHandler<double> ExtractProgressUpdated;
        public event EventHandler<double> CopyProgressUpdated;
        public Task<ErrorResult> TransferandUnzip(string source, string destinationfolder)
        {
            return Task.Run(() =>
            {
                ClearErrorFlags();
                try
                {
                    var filename = Path.GetFileName(source);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                    var copiedfile = $"{destinationfolder}\\{filename}";

                    if (!File.Exists(source)) ThrowError("Source file does not exist.");
                    if (!Directory.Exists(destinationfolder))
                    {
                        Directory.CreateDirectory(destinationfolder);
                    }
                    CheckAndThrowIfError(CopyFile(source, copiedfile));
                    CheckAndThrowIfError(ExtractZip(copiedfile, destinationfolder));
                }
                catch (Exception e)
                {
                    CatchException(e);
                }
                return Result;
            });
        }
        public ErrorResult ExtractZip(string zipFilePath, string extractFolderPath)
        {
            ClearErrorFlags();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    int totalEntries = archive.Entries.Count;
                    int extractedEntries = 0;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(extractFolderPath, entry.FullName);

                        // Ensure directory exists
                        string directoryPath = Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        if (!string.IsNullOrEmpty(entry.Name)) // Skip directory entries
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }

                        extractedEntries++;
                        ExtractProgressUpdated?.Invoke(null, (double)extractedEntries / totalEntries);
                    }
                }
            }
            catch (Exception e)
            {
                CatchException(e);
            }
            return Result;
        }
        private ErrorResult CopyFile(string source, string destination)
        {
            ClearErrorFlags();
            try
            {
                const int bufferSize = 1024 * 1024; // 1MB buffer
                byte[] buffer = new byte[bufferSize];
                long totalBytes = new FileInfo(source).Length;
                long totalBytesCopied = 0;

                using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
                using (FileStream destStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                {
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);
                        totalBytesCopied += bytesRead;
                        CopyProgressUpdated?.Invoke(this, (double)totalBytesCopied / totalBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
            }
            return Result;
        }
    }
}
