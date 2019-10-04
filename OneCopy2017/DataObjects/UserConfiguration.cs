using System;
using OneCopy2017.ThirdParty;

namespace OneCopy2017.DataObjects
{
    public static class UserConfiguration
    {
        [Argument('d', "dir")] public static string[] Directories { get; set; }

        [Argument('p', "preview")] public static bool Preview { get; set; }

        [Argument('s', "dupes-dir")] public static string DupesDirectory { get; set; }

        public static void Load()
        {
            Arguments.Populate(typeof(UserConfiguration), Environment.CommandLine);
        }
    }
}