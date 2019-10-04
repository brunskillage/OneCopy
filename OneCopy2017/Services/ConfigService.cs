using System;
using System.IO;
using System.Linq;
using OneCopy2017.DataObjects;

namespace OneCopy2017.Services
{
    public class ConfigService
    {
        private readonly AppManagementService _appManagementService;
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly ValidationService _validationService;

        public ConfigService(ValidationService validationService, ErrorHandlingService errorHandlingService,
            AppManagementService appManagementService)
        {
            _validationService = validationService;
            _errorHandlingService = errorHandlingService;
            _appManagementService = appManagementService;

            SynologyHiddenDirectoryName = @"@eaDir";
            DupesDirectoryName = @"_dupes";
            Keep = KeepOption.Oldest;
        }

        public string[] Directories { get; set; }
        public string[] ExcludeDirectoryNames { get; set; }
        public string[] IncludeFileExtensions { get; set; }
        public bool Preview { get; set; }
        public string DupesDirectory { get; set; }
        public string SynologyHiddenDirectoryName { get; }
        public string DupesDirectoryName { get; }
        public KeepOption Keep { get; set; }
        public bool Clean { get; set; }

        public void Validate()
        {
            if (Directories == null)
                _errorHandlingService.ThrowCatastrophicError(
                    $"Invalid directory, check start arguments or configuration. {_appManagementService.Help}");


            foreach (var directory in Directories)
                if (!_validationService.IsValidDirectory(directory))
                    _errorHandlingService.ThrowCatastrophicError(
                        $"Invalid directory '{directory}' check start arguments or configuration. {_appManagementService.Help}");

            KeepOption strategyOption;
            if (Enum.TryParse(CommandArguments.Strategy ?? KeepOption.Oldest.ToString(), true, out strategyOption))
                Keep = strategyOption;
            else
                _errorHandlingService.ThrowCatastrophicError(
                    "Cannot start the program. Strategy option was defined in start up arguments but is invalid. Valid options are 'oldest' or 'newest'");
        }

        public void Load()
        {
            CommandArguments.Populate();

            Preview = CommandArguments.Preview;
            Directories = CommandArguments.Directories;
            DupesDirectory = CommandArguments.DupesDirectory;
            ExcludeDirectoryNames = CommandArguments.ExcludeDirectoryNames;
            IncludeFileExtensions = CommandArguments.IncludeFileExtensions;
            Clean = CommandArguments.Clean;

            Validate();

            if (string.IsNullOrWhiteSpace(DupesDirectory))
                DupesDirectory = Path.Combine(Directories.First(), DupesDirectoryName);
        }
    }
}