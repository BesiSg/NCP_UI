using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Change these to your folders
        string folderPath1 = @"C:\Path\To\Folder1";
        string folderPath2 = @"C:\Path\To\Folder2";

        // Output file path
        string outputFile = @"C:\Path\To\output.txt";

        // Get files from both folders (including subfolders)
        var files1 = GetFilesByName(folderPath1);
        var files2 = GetFilesByName(folderPath2);

        // Find common file names
        var commonFileNames = new HashSet<string>(files1.Keys);
        commonFileNames.IntersectWith(files2.Keys);

        var sb = new StringBuilder();

        sb.AppendLine($"Comparing files in:");
        sb.AppendLine($"Folder 1: {folderPath1}");
        sb.AppendLine($"Folder 2: {folderPath2}");
        sb.AppendLine($"Common files found: {commonFileNames.Count}");
        sb.AppendLine();

        foreach (var fileName in commonFileNames)
        {
            string path1 = files1[fileName];
            string path2 = files2[fileName];

            // Compute hashes
            string md5_1 = ComputeHash(path1, MD5.Create());
            string md5_2 = ComputeHash(path2, MD5.Create());

            string sha1_1 = ComputeHash(path1, SHA1.Create());
            string sha1_2 = ComputeHash(path2, SHA1.Create());

            string sha256_1 = ComputeHash(path1, SHA256.Create());
            string sha256_2 = ComputeHash(path2, SHA256.Create());

            // Compare hashes
            bool md5Equal = md5_1 == md5_2;
            bool sha1Equal = sha1_1 == sha1_2;
            bool sha256Equal = sha256_1 == sha256_2;

            // Write results
            sb.AppendLine($"File: {fileName}");
            sb.AppendLine($"  Folder1: {path1}");
            sb.AppendLine($"  Folder2: {path2}");
            sb.AppendLine($"  MD5:    {md5_1} | {md5_2} | Equal: {md5Equal}");
            sb.AppendLine($"  SHA-1:  {sha1_1} | {sha1_2} | Equal: {sha1Equal}");
            sb.AppendLine($"  SHA-256:{sha256_1} | {sha256_2} | Equal: {sha256Equal}");
            sb.AppendLine(new string('-', 80));
        }

        // Write output to file
        File.WriteAllText(outputFile, sb.ToString());

        Console.WriteLine($"Comparison complete. Results saved to:\n{outputFile}");
    }

    // Get dictionary of file name -> full path for all files in folder and subfolders
    static Dictionary<string, string> GetFilesByName(string folderPath)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(filePath);

            // If multiple files with same name exist in the same folder tree, 
            // this will keep only the first encountered.
            // You can modify this logic if you want to handle duplicates differently.
            if (!dict.ContainsKey(fileName))
            {
                dict[fileName] = filePath;
            }
        }

        return dict;
    }

    // Compute hash of file using specified algorithm
    static string ComputeHash(string filePath, HashAlgorithm algorithm)
    {
        using (algorithm)
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hashBytes = algorithm.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}