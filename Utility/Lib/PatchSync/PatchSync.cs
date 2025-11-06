using System.IO;
using System.IO.Compression;
using Utility.PatchSync;

namespace Utility.Lib.PatchSync
{
    public class PatchSync : BaseUtility
    {
        private string _sourceFolder;
        private string _destinationFolder;
        private string _sourceFile;
        private string _destinationFile;
        private PatchSyncData fileStorage;
        private PatchForwardLookup forwardLookup;
        private PatchReverseLookup reverseLookup;
        public bool DestinationPatchNotEmpty
        {
            get => GetValue(() => DestinationPatchNotEmpty);
            set => SetValue(() => DestinationPatchNotEmpty, value);
        }
        public string PatchName
        {
            get => GetValue(() => PatchName);
            set => SetValue(() => PatchName, value);
        }
        public string PatchNameWithoutExtension
        {
            get => GetValue(() => PatchNameWithoutExtension);
            set => SetValue(() => PatchNameWithoutExtension, value);
        }
        public double ExtractProgress
        {
            get => GetValue(() => ExtractProgress);
            set => SetValue(() => ExtractProgress, value);
        }
        public double CopyProgress
        {
            get => GetValue(() => CopyProgress);
            set => SetValue(() => CopyProgress, value);
        }
        public double ValidateProgress
        {
            get => GetValue(() => ValidateProgress);
            set => SetValue(() => ValidateProgress, value);
        }
        public Task TransferandUnzip()
        {
            return Task.Run(() =>
            {
                ClearErrorFlags();
                try
                {
                    if (DestinationPatchNotEmpty) return;
                    if (!File.Exists(_sourceFile)) ThrowError("Source file does not exist.");
                    if (Directory.Exists($"{_destinationFolder}\\{PatchNameWithoutExtension}"))
                    {
                        if (Directory.GetFiles($"{_destinationFolder}\\{PatchNameWithoutExtension}").Length > 0)
                            ThrowError("Folder exist and not empty");
                    }
                    else
                        Directory.CreateDirectory(_destinationFolder);
                    CheckAndThrowIfError(CopyFile(_sourceFile, _destinationFile));
                    CheckAndThrowIfError(ExtractZip(_destinationFile, _destinationFolder));
                    CheckAndThrowIfError(PopulateFiles());
                }
                catch (Exception e)
                {
                    CatchAndPromptErr(e);
                }
            });
        }
        private ErrorResult ExtractZip(string zipFilePath, string extractFolderPath)
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
                        ExtractProgress = ((double)extractedEntries / totalEntries) * 100;
                    }
                }
            }
            catch (Exception e)
            {
                CatchAndPromptErr(e);
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
                        CopyProgress = ((double)totalBytesCopied / totalBytes) * 100;
                    }
                }
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return Result;
        }
        public ErrorResult PopulateFiles()
        {
            this.ClearErrorFlags();
            string rootPath = $"{_destinationFolder}\\{PatchNameWithoutExtension}";

            try
            {
                // Get all directories including subdirectories
                if (!Directory.Exists(rootPath)) return Result;
                var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories).ToList();
                var dictionaryshortened = new Dictionary<string, string>();
                var toignore = new HashSet<string>();
                directories.ForEach(name => dictionaryshortened[name.Replace(rootPath, "")] = Path.GetFileName(name));
                foreach (var shortenedpair in dictionaryshortened)
                {
                    if (shortenedpair.Value == string.Empty) continue;
                    var parts = shortenedpair.Value.Split('_');
                    if (parts.Length >= 4)
                    {
                        bool isVersionFormat = char.IsDigit(parts[0].LastOrDefault()) & char.IsDigit(parts[1].FirstOrDefault()) & char.IsDigit(parts[2].FirstOrDefault());
                        if (isVersionFormat)
                        {
                            var todeletepath = shortenedpair.Key;
                            dictionaryshortened[todeletepath] = string.Empty;
                            toignore.Add(rootPath + shortenedpair.Key);
                            foreach (var shortenedpairagain in dictionaryshortened)
                            {
                                if (shortenedpairagain.Value == string.Empty) continue;
                                if (shortenedpairagain.Key.Contains(todeletepath))
                                {
                                    dictionaryshortened[shortenedpairagain.Key] = string.Empty;
                                    toignore.Add(rootPath + shortenedpairagain.Key);
                                }

                            }
                        }
                    }
                }
                var patchfiles= new List<string>();
                directories.Add(rootPath);
                foreach (var directory in directories)
                {
                    if (toignore.Contains(directory)) continue;
                    var files = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).ToList();
                    var shortened = new List<string>();
                    files.ForEach(name =>
                    {
                        if (!name.Contains(".zip", StringComparison.OrdinalIgnoreCase) && !name.Contains(".7z", StringComparison.OrdinalIgnoreCase))
                            shortened.Add(name.Replace(rootPath, ""));
                    });

                    patchfiles.AddRange(shortened);
                }
                fileStorage.StandardFiles.Clear();
                fileStorage.StandardFiles.AddRange(patchfiles);
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return Result;
        }
        public ErrorResult Validate()
        {
            var localrootPath = $"{_destinationFolder}\\{PatchNameWithoutExtension}";
            var networkrootPath = $"{NCPFileStructure.networkDevelop}\\{PatchNameWithoutExtension}";
            var listoffilestovalidate = fileStorage.StandardFiles;
            var totalcount = listoffilestovalidate.Count;
            var currcount = 0;
            var tasks = new List<Func<Task>>();
            foreach (var file in listoffilestovalidate)
            {
                tasks.Add(async () =>
                {
                    await Task.Run(() => CheckandAddIfDifferent(localrootPath, networkrootPath, file, totalcount, ref currcount));
                });
            }
            var thread = new ThreadHandler(tasks, 10);
            thread.RunTasks().Wait();
            return Result;
        }
        object ValidationObj = new object();
        private void CheckandAddIfDifferent(string localrootPath, string networkrootPath, string file, int totalcount, ref int currcount)
        {
            var same = Hash.ComputeFileHash(localrootPath + file) == Hash.ComputeFileHash(networkrootPath + file);
            if (!same)
            {
                fileStorage.DifferentFiles.Add(file);
            }
            currcount++;
            lock (ValidationObj)
            {
                ValidateProgress = ((double)currcount / totalcount) * 100;
            }

        }
        public Task<ErrorResult> CheckIfEmpty()
        {
            return Task.Run(() =>
            {
                ClearErrorFlags();
                try
                {
                    DestinationPatchNotEmpty = Directory.Exists($"{_destinationFolder}\\{PatchNameWithoutExtension}") && Directory.EnumerateFileSystemEntries($"{_destinationFolder}\\{PatchNameWithoutExtension}").Any();
                }
                catch (Exception ex)
                {
                    CatchAndPromptErr(ex);
                }
                return Result;
            });
        }
        public PatchSync(string sourcefolder, string destinationfolder, string filename, PatchFilesStorage filestorage, PatchForwardLookup forwardlookup, PatchReverseLookup reverselookup)
        {
            _sourceFolder = sourcefolder;
            _destinationFolder = destinationfolder;
            PatchName = filename;
            PatchNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            _sourceFile = $"{_sourceFolder}\\{PatchName}";
            _destinationFile = $"{_destinationFolder}\\{PatchName}";
            fileStorage = filestorage.Get(PatchNameWithoutExtension);
            forwardLookup = forwardlookup;
            reverseLookup = reverselookup;
        }
        public void GetUnitCfgInfo()
        {
            var fromLocal = true;
            if (fromLocal)
            {
                var lines = new Dictionary<string, string>();
                var path = $"{NCPFileStructure.localDevelop}\\{PatchNameWithoutExtension}\\{NCPFileStructure.UnitCfg}";
                if (!File.Exists(path)) return;
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "\r") continue;
                        if (line.Contains("# This is a dummy entry to satisfy the CopyRelease script", StringComparison.OrdinalIgnoreCase)) break;
                        if (line.FirstOrDefault() == '#') continue;
                        if (line.Contains("AllUnitVersionsList", StringComparison.OrdinalIgnoreCase)) continue;
                        line = line.Replace("\\", "");
                        line = line.TrimEnd();
                        var parts = line.Split(' ', '\t');
                        if (parts.Count() < 2) continue;
                        if (parts[0].Contains("Version_")) parts[0] = parts[0].Replace("Version_", "");
                        if (parts[^1].EndsWith(".patch")) parts[^1] = parts[^1].Replace(".patch", "");
                        lines[parts[0]] = parts[^1];
                    }
                    var fwlist = forwardLookup.Get(PatchNameWithoutExtension);
                    fwlist.Clear();
                    foreach (var item in lines)
                    {
                        fwlist.Add($"{item.Key} {item.Value}");
                        reverseLookup[$"{item.Key} {item.Value}"] = PatchNameWithoutExtension;
                    }
                }
            }
        }
    }
}
