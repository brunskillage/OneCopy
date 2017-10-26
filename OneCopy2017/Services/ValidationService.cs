using System.IO;

namespace OneCopy2017.Services
{
    public class ValidationService
    {
        private readonly EventService _eventService;

        public ValidationService(EventService eventService)
        {
            _eventService = eventService;
        }

        public bool IsValidDirectory(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir) && new DirectoryInfo(dir).Exists)
            {
                return true;
            }

            _eventService.Talk($"Directory is not valid '{dir}'");
            return false;
        }
    }
}