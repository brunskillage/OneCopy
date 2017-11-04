namespace OneCopy2017.Services
{
    public class AppManagementService
    {
        public AppManagementService()
        {
            Name = "Onecopy 2017";
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string ReleaseDate { get; set; }
        public string Help { get; set; }
        public string About { get; set; }
    }
}