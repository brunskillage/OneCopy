using System;
using System.Data;
using System.IO;
using System.Linq;

namespace OneCopy2017.DataObjects
{
    public class FileBlob
    {
        public FileBlob(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new NoNullAllowedException("Fileinfo is null");
            FileInformation = fileInfo;
        }

        public string FullName => FileInformation.FullName;
        public string Hash { get; set; }
        public long Length => FileInformation.Length;

        public DateTime NewestTime
        {
            get
            {
                return TimeInformation.OrderBy(d => d.Ticks).Last();
            }
        }

        public DateTime OldestTime
        {
            get
            {
                return TimeInformation.OrderBy(d => d.Ticks).First();
            }
        }

        private FileInfo FileInformation { get; }

        private DateTime[] TimeInformation => new[]
        {
            FileInformation.CreationTimeUtc,
            FileInformation.LastAccessTimeUtc,
            FileInformation.LastWriteTimeUtc,
            FileInformation.Directory.CreationTimeUtc,
            FileInformation.Directory.LastAccessTimeUtc,
            FileInformation.Directory.LastWriteTimeUtc
        };

        public override string ToString()
        {
            return
                $"Name: {FileInformation.FullName} {Environment.NewLine}" +
                $"   Oldest: {OldestTime} Created:{FileInformation.CreationTimeUtc} {Environment.NewLine}" +
                $"   Modified:{FileInformation.LastWriteTimeUtc} Accessed: {FileInformation.LastAccessTimeUtc} {Environment.NewLine}" +
                $"   Hash: {Hash}";
        }
    }
}