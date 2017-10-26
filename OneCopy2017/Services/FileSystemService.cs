using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            private readonly EventService _eventService;

            public readonly Func<string, int> OrderedBySegmentCountFunction =
                d => d.ToCharArray().Count(s => s == Path.DirectorySeparatorChar);

            public FileSystemService(ConfigService configService, EventService eventService)
            {
                _configService = configService;
                _eventService = eventService;
            }

            public void EnsureFileDirectory(string filePath)
            {
                if (filePath == null)
                    return;
                new FileInfo(filePath).Directory.Create();
            }

            public void MoveFile(string srcFullPath, string destFullPath)
            {
                _eventService.Talk($"Moving file {srcFullPath} to {destFullPath}");
                if (srcFullPath == null || !File.Exists(srcFullPath))
                    return;
                EnsureFileDirectory(destFullPath);
                if(!_configService.Preview)
                    File.Move(srcFullPath, destFullPath);
            }

            public string GetHash(string filePath)
            {
                // strange paralle behaviour - unreliable numbers based n hash which went on single thread.
                if (filePath == null || !File.Exists(filePath))
                    return string.Empty;

                _eventService.Talk($"Hashing {filePath}");
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
                if(!_configService.Preview)
                    File.WriteAllText(file, text);
            }

            public IEnumerable<string> GetAllDirectories(string root, string excludeSplitByPipe)
            {
                _eventService.Talk($"Getting all directories in {root}");
                _eventService.Talk($"Exclusions are {excludeSplitByPipe}");

                if (excludeSplitByPipe == null)
                    excludeSplitByPipe = String.Empty;

                var dirs = Directory.EnumerateDirectories(root, "*.*", SearchOption.AllDirectories);
                
                var excludeArray = excludeSplitByPipe.ToUpperInvariant().Split('|');
                
                if (!string.IsNullOrWhiteSpace(excludeSplitByPipe))
                    dirs = dirs.Where(s => !excludeArray.Any(a => s.ToUpperInvariant().Contains(a)));

                return dirs;
            }

            public IEnumerable<string> GetAllFileSystemEntries(string root, string excludeSplitByPipe)
            {
                if (excludeSplitByPipe == null)
                    excludeSplitByPipe = String.Empty;

                _eventService.Talk($"Getting all file system entries in {root}");
                _eventService.Talk($"Exclusions are {excludeSplitByPipe}");

                var excludeArray = excludeSplitByPipe.ToUpperInvariant().Split('|');
                var dirs = Directory.EnumerateFileSystemEntries(root, "*.*", SearchOption.AllDirectories);
                if (!string.IsNullOrWhiteSpace(excludeSplitByPipe))
                    dirs = dirs.Where(s => !excludeArray.Any(a => s.ToUpperInvariant().Contains(a)));
                return dirs;
            }

            public IEnumerable<FileBlob> GetAllFilesInDirectory(string dir)
                => Directory.EnumerateFiles(dir).Select(path => new FileBlob(new FileInfo(path)));

            public IEnumerable<FileBlob> GetAllFileBlobs(string root, string excludeSplitByPipe)
            {
                _eventService.Talk($"Getting files for {root}");
                var dirs = GetAllDirectories(root, excludeSplitByPipe);
                var blobs = new List<FileBlob>();

                foreach (var dir in dirs)
                {
                    blobs.AddRange(GetAllFilesInDirectory(dir));
                }

                return blobs;
            }

            public IEnumerable<string> OrderBySegmentsDesc(IEnumerable<string> sourceDirs)
            {
                return sourceDirs.OrderByDescending(OrderedBySegmentCountFunction);
            }

            public List<FileBlob> GetDuplicates(List<FileBlob> blobs)
            {
                _eventService.Talk($"Getting duplicates");
                var duplicateLengthBlobs = new List<FileBlob>();

                _eventService.Talk($"Grouping by length");
                var lengthGrouping = blobs.GroupBy(b => b.Length).Where(g => g.Count() > 1);

                _eventService.Talk(
                    $"Found {lengthGrouping.Count()} groups of matching lengths containing {lengthGrouping.SelectMany(g => g.OfType<FileBlob>()).Count()} files");

                if (lengthGrouping.Count() == 0)
                    return new List<FileBlob>();

                foreach (var group in lengthGrouping)
                {
                    _eventService.Talk(
                        $"Generating hashes within the {group.Key} length group ({group.Count()} candidates)");
                    foreach (var fileBlob in group)
                    {
                        fileBlob.Hash = GetHash(fileBlob.FullName);
                    }

                    duplicateLengthBlobs.AddRange(group);
                }

                _eventService.Talk($"Grouping by hash");

                var hashGrouping = duplicateLengthBlobs.GroupBy(b => b.Hash).Where(g => g.Count() > 1);

                _eventService.Talk(
                    $"Found {hashGrouping.Count()} groups of matching hashes containing {hashGrouping.SelectMany(g => g.OfType<FileBlob>()).Count()} files");

                if (hashGrouping.Count() == 0)
                    return new List<FileBlob>();

                var duplicateMd5Blobs = new List<FileBlob>();

                foreach (var hashGroup in hashGrouping)
                {
                    _eventService.Talk("======================================");
                    _eventService.Talk(
                        $"Sorting group {hashGroup.Key.Substring(0, 5)} and collecting all but the oldest of any file times, with shtest path prefered");

                    var orderedHashGroup = hashGroup.OrderBy(b => b.OldestTime).ThenBy(b=>b.FullName.Length);
                    foreach (var fileBlob in orderedHashGroup)
                    {
                        _eventService.Talk(fileBlob.ToString());
                    }

                    duplicateMd5Blobs.AddRange(orderedHashGroup.Skip(1));
                }

                _eventService.Talk($"{duplicateMd5Blobs.Count} duplicates to be removed found");
                return duplicateMd5Blobs;
            }


            private void TryDeleteFile(string fullPath)
            {
                if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                {
                    return;
                }
                _eventService.Talk($"Deleting file {fullPath}");
                try
                {
                    if (!_configService.Preview)
                       File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _eventService.Talk($"Cannot delete file {fullPath}");
                    _eventService.Talk($"{ex}");
                }
            }

            public void RemoveEmptyDirectories(string dir, string excludeDirectories)
            {
                _eventService.Talk($"Finding empty directories in {dir}");
                var dirs = GetAllDirectories(dir, excludeDirectories);

                var singleFileDirectoryGroups = dirs.Where(s => Directory.EnumerateFiles(s).Count() == 1);

                foreach (var dirname in singleFileDirectoryGroups)
                {
                      TryDeleteFile(Path.Combine(dirname, "Thumbs.db"));
                }

                var zeroDirOrFileEntryGroups =
                    GetAllDirectories(dir, excludeDirectories)
                        .Where(s => !GetAllFileSystemEntries(s, excludeDirectories).Any())
                        .OrderByDescending(OrderedBySegmentCountFunction);

                foreach (var dirname in zeroDirOrFileEntryGroups)
                {
                    TryDeleteDirectory(dirname);
                }
            }

            private void TryDeleteDirectory(string fullPath)
            {
                if (string.IsNullOrWhiteSpace(fullPath) || (!Directory.Exists(fullPath)))
                {
                    return;
                }

                _eventService.Talk($"Deleting directory {fullPath}");
                try
                {
                    if (!_configService.Preview)
                        new DirectoryInfo(fullPath).Delete();
                }
                catch (Exception ex)
                {
                    _eventService.Talk($"Cannot delete directory {fullPath}");
                    _eventService.Talk($"{ex}");
                }
            }
        }
    }
}