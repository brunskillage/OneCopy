using System;
using System.IO;
using System.Linq;
using OneCopy2017.DataObjects;

namespace OneCopy2017.Services
{
    public class ConfigService
    {
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly ValidationService _validationService;

        public ConfigService(ValidationService validationService, ErrorHandlingService errorHandlingService)
        {
            _validationService = validationService;
            _errorHandlingService = errorHandlingService;

            SynologyHiddenDirectoryName = @"@eaDir";
            DupesDirectoryName = @"_dupes";
        }

        // public string RootDirectory { get; }
        public string[] Directories { get; set; }

        public bool Preview { get; set; }
        public string DupesDirectory { get; set; }
        public string SynologyHiddenDirectoryName { get; }
        public string DupesDirectoryName { get; }

        public string Help =>
            $"{Environment.NewLine}Start example:   OneCopy2017.exe --dir \"c:\\photos\" --dir \"d:\\movies\" --preview true" +
            $"{Environment.NewLine}If multiple --dir are specified a _dupes folder will contain the duplicates for set";

        public void Validate()
        {
            foreach (var directory in Directories)
                if (!_validationService.IsValidDirectory(directory))
                    _errorHandlingService.ThrowCatastrophicError(
                        $"Invalid directory '{directory}' check start arguments or configuration. {Help}");
        }

        public void Load()
        {
            Preview = UserConfiguration.Preview;
            Directories = UserConfiguration.Directories;
            DupesDirectory = UserConfiguration.DupesDirectory;

            Validate();

            if (DupesDirectory == null)
                DupesDirectory = Path.Combine(Directories.First(), DupesDirectoryName);
        }
    }
}