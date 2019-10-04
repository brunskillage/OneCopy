using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OneCopy2017.DataObjects;

// ReSharper disable UseMethodAny.2
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable PossibleNullReferenceException

namespace OneCopy2017.Services
{
    namespace SimilarImage.Services
    {
        public class FileSystemService
        {
            private readonly ConfigService _configService;
            private readonly EventService _es;

            public readonly Func<string, int> OrderedBySegmentCountFunction =
                d => d.ToCharArray().Count(s => s == Path.DirectorySeparatorChar);

            public FileSystemService(ConfigService configService, EventService es)
            {
                _configService = configService;
                _es = es;
            }

            public void EnsureFileDirectory(string filePath)
            {
                if (filePath == null)
                    return;
                new FileInfo(filePath).Directory.Create();
            }

            public void MoveFile(string srcFullPath, string destFullPath)
            {
                _es.Talk($"Moving file {srcFullPath} to {destFullPath}");
                if (srcFullPath == null || !File.Exists(srcFullPath))
                    return;
                EnsureFileDirectory(destFullPath);
                if (!_configService.Preview)
                    File.Move(srcFullPath, destFullPath);
            }

            public void CopyFile(string srcFullPath, string destFullPath, bool overWrite)
            {
                _es.Talk($"Copying file {srcFullPath} to {destFullPath}");
                if (srcFullPath == null || !File.Exists(srcFullPath))
                    return;
                EnsureFileDirectory(destFullPath);
                if (!_configService.Preview)
                    File.Copy(srcFullPath, destFullPath, overWrite);
            }

            public string GetHash(string filePath)
            {
                // strange paralle behaviour - unreliable numbers based n hash which went on single thread.
                if (filePath == null || !File.Exists(filePath))
                    return string.Empty;

                _es.Talk($"Hashing {filePath}");
                using (var stream = new BufferedStream(File.OpenRead(filePath), 4096))
                {
                    var sha = new SHA256Managed();
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }

            public long GetFileLength(string filePath)
            {
                if (filePath == null || !File.Exists(filePath))
                    return 0;
                return new FileInfo(filePath).Length;
            }

            public DateTime GetDateCreated(string filePath)
            {
                if (filePath == null || !File.Exists(filePath))
                    return DateTime.MinValue;
                return new FileInfo(filePath).CreationTime;
            }

            public bool FileExists(string file)
            {
                return File.Exists(file);
            }

            public string GetText(string file)
            {
                return File.ReadAllText(file, Encoding.UTF8);
            }

            public void Save(string file, string text)
            {
                if (!_configService.Preview)
                    File.WriteAllText(file, text);
            }

            public IEnumerable<string> GetAllDirectories(string root, string excludeSplitByPipe)
            {
                _es.Talk($"Getting all directories in {root}");
                _es.Talk($"Exclusions are {excludeSplitByPipe}");

                if (excludeSplitByPipe == null)
                    excludeSplitByPipe = string.Empty;

                // add the root dir
                var dirs = Enumerable.Empty<string>();
                dirs = dirs.Concat(new[] {root});

                dirs = dirs.Concat(Directory.EnumerateDirectories(root, "*.*", SearchOption.AllDirectories));

                var excludeArray = excludeSplitByPipe.ToUpperInvariant().Split('|');

                if (!string.IsNullOrWhiteSpace(excludeSplitByPipe))
                    dirs = dirs.Where(s => !excludeArray.Any(a => s.ToUpperInvariant().Contains(a)));

                return dirs;
            }

            public IEnumerable<string> GetAllFileSystemEntries(string root, string excludeSplitByPipe)
            {
                _es.Talk($"Getting all file system entries in {root}");
                _es.Talk($"Exclusions are {excludeSplitByPipe}");

                var dirs = Directory.EnumerateFileSystemEntries(root, "*.*", SearchOption.AllDirectories);

                if (!string.IsNullOrWhiteSpace(excludeSplitByPipe))
                {
                    var excludeArray = excludeSplitByPipe.ToUpperInvariant().Split('|');
                    dirs = dirs.Where(s => !excludeArray.Any(a => s.ToUpperInvariant().Contains(a)));
                }

                return dirs;
            }

            public IEnumerable<FileBlob> GetAllFilesInDirectory(string dir, string includeFileExtensionsListSplitByPipe)
            {
                var files = Directory.EnumerateFiles(dir);

                if (!string.IsNullOrWhiteSpace(includeFileExtensionsListSplitByPipe))
                {
                    var includeArray = includeFileExtensionsListSplitByPipe.ToUpperInvariant().Split('|');
                    files =
                        files.Where(
                            s => includeArray.Any(a => a == Path.GetExtension(s).Substring(1).ToUpperInvariant()));
                }

                return files.Select(path => new FileBlob(new FileInfo(path)));
            }

            public IEnumerable<FileBlob> GetAllFileBlobs(string root, string includeFileExtensionsList,
                string excludeDirectoriesList)
            {
                _es.Talk($"Getting files for {root}");
                var dirs = GetAllDirectories(root, excludeDirectoriesList);
                var blobs = new List<FileBlob>();

                foreach (var dir in dirs) blobs.AddRange(GetAllFilesInDirectory(dir, includeFileExtensionsList));

                return blobs;
            }

            public IEnumerable<string> OrderBySegmentsDesc(IEnumerable<string> sourceDirs)
            {
                return sourceDirs.OrderByDescending(OrderedBySegmentCountFunction);
            }

            public List<FileBlob> GetDuplicates(List<FileBlob> blobs, KeepOption keepOption = KeepOption.Oldest)
            {
                _es.Talk("Getting duplicates");
                var duplicateLengthBlobs = new List<FileBlob>();

                _es.Talk("Grouping by length");
                var lengthGrouping = blobs.GroupBy(b => b.Length).Where(g => g.Count() > 1);

                _es.Talk(
                    $"Found {lengthGrouping.Count()} groups of matching lengths containing {lengthGrouping.SelectMany(g => g.OfType<FileBlob>()).Count()} files");

                if (lengthGrouping.Count() == 0)
                    return new List<FileBlob>();

                foreach (var group in lengthGrouping)
                {
                    _es.Talk(
                        $"Generating hashes within the {group.Key} length group ({group.Count()} candidates)");

                    //foreach (var fileBlob in group)
                    //    fileBlob.Hash = GetHash(fileBlob.FullName);

                    Parallel.ForEach(group, fileBlob => { fileBlob.Hash = GetHash(fileBlob.FullName); });

                    duplicateLengthBlobs.AddRange(group);
                }

                _es.Talk("Grouping by hash");

                var hashGrouping = duplicateLengthBlobs.GroupBy(b => b.Hash).Where(g => g.Count() > 1);

                _es.Talk(
                    $"Found {hashGrouping.Count()} groups of matching hashes containing {hashGrouping.SelectMany(g => g.OfType<FileBlob>()).Count()} files");

                if (hashGrouping.Count() == 0)
                    return new List<FileBlob>();

                var duplicateMd5Blobs = new List<FileBlob>();

                // look at the set of duplicates
                foreach (var hashGroup in hashGrouping)
                {
                    _es.Talk("======================================");
                    _es.Talk(
                        $"Sorting group {hashGroup.Key.Substring(0, 5)} and collecting all but the oldest of any file times, with shortest path prefered");

                    // execute functions on the group, this enables keep strategey
                    var orderedHashGroup =
                        ApplySort(hashGroup,
                            keepOption); // .ThenBy(b => b.FullName.Split(Path.DirectorySeparatorChar).Length);

                    // print the group contents
                    foreach (var fileBlob in orderedHashGroup)
                        _es.Talk(fileBlob.ToString());

                    // add all but the first to the duplicates group leaving the prefered original
                    duplicateMd5Blobs.AddRange(orderedHashGroup.Skip(1));
                }

                _es.Talk($"{duplicateMd5Blobs.Count} duplicates to be removed found");
                return duplicateMd5Blobs;
            }

            private IEnumerable<FileBlob> ApplySort(IGrouping<string, FileBlob> hashGroup, KeepOption keepOption)
            {
                if (keepOption == KeepOption.Oldest)
                    return hashGroup.OrderBy(b => b.OldestTime);

                if (keepOption == KeepOption.Newest)
                    return hashGroup.OrderByDescending(b => b.OldestTime);

                throw new ArgumentException("Invalid keep option");
            }


            private void TryDeleteFile(string fullPath)
            {
                if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath)) return;
                _es.Talk($"Deleting file {fullPath}");
                try
                {
                    if (!_configService.Preview)
                        File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _es.Talk($"Cannot delete file {fullPath}");
                    _es.Talk($"{ex}");
                }
            }

            public void RemoveEmptyDirectories(string dir, string excludeDirectories)
            {
                _es.Talk($"Finding empty directories in {dir}");
                var dirs = GetAllDirectories(dir, excludeDirectories);

                var singleFileDirectoryGroups = dirs.Where(s => Directory.EnumerateFiles(s).Count() == 1);

                foreach (var dirname in singleFileDirectoryGroups) TryDeleteFile(Path.Combine(dirname, "Thumbs.db"));

                var zeroDirOrFileEntryGroups =
                    GetAllDirectories(dir, excludeDirectories)
                        .Where(s => !GetAllFileSystemEntries(s, excludeDirectories).Any())
                        .OrderByDescending(OrderedBySegmentCountFunction);

                foreach (var dirname in zeroDirOrFileEntryGroups) TryDeleteDirectory(dirname);
            }

            private void TryDeleteDirectory(string fullPath)
            {
                if (string.IsNullOrWhiteSpace(fullPath) || !Directory.Exists(fullPath)) return;

                _es.Talk($"Deleting directory {fullPath}");
                try
                {
                    if (!_configService.Preview)
                        new DirectoryInfo(fullPath).Delete();
                    else _es.Talk($"PREVIEW ONLY Deleting directory {fullPath}");
                }
                catch (Exception ex)
                {
                    _es.Talk($"Cannot delete directory {fullPath}");
                    _es.Talk($"{ex}");
                }
            }
        }
    }
}