using System;
using System.Linq;
using OneCopy2017.Services;
using OneCopy2017.Services.SimilarImage.Services;

namespace OneCopy2017
{
    public class App
    {
        private readonly ConfigService _configService;
        private readonly EventService _eventService;
        private readonly FileSystemService _fileSystem;
        private readonly LogService _logService;

        public App(FileSystemService fileSystem, ConfigService configService, LogService logService,
            EventService eventService)
        {
            _fileSystem = fileSystem;
            _configService = configService;
            _logService = logService;
            _eventService = eventService;
        }

        public void Run()
        {
            _eventService.OnTalk += (sender, args) => { _logService.Print(args.Message); };

            _eventService.Talk("Starting deduplication process");
            _eventService.Talk($"Preview mode is {_configService.Preview}");

            var blobs = _configService.Directories.SelectMany(
                d =>
                    _fileSystem.GetAllFileBlobs(d,
                        $"{_configService.DupesDirectoryName}|{_configService.SynologyHiddenDirectoryName}")).ToList();


            _eventService.Talk($"Found {blobs.Count()} files");

            var duplicates = _fileSystem.GetDuplicates(blobs).ToArray();

            if (duplicates.Any())
            {
                _eventService.Talk($"Moving duplicates to {_configService.DupesDirectory}");

                var orderedSourceDirs = _configService.Directories.OrderByDescending(s => s.Length);

                foreach (var duplicate in duplicates)
                {
                    var src = duplicate.FullName;
                    var matchedSourceDirectory = orderedSourceDirs.First(src.StartsWith);
                    var dest = duplicate.FullName.Replace(matchedSourceDirectory, _configService.DupesDirectory);
                    _fileSystem.MoveFile(src, dest);
                }

                _fileSystem.RemoveEmptyDirectories(_configService.Directories.First(),
                    $"{_configService.DupesDirectoryName}|{_configService.SynologyHiddenDirectoryName}");
            }

            _eventService.Talk($"Finished");

            Console.ReadLine();
        }
    }
}