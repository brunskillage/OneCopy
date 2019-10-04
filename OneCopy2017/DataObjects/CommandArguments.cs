using System;
using OneCopy2017.ThirdParty;

namespace OneCopy2017.DataObjects
{
    public static class CommandArguments
    {
        [Argument('d', "dir")] public static string[] Directories { get; set; }

        [Argument('e', "exclude-dir")] public static string[] ExcludeDirectoryNames { get; set; }

        [Argument('x', "ext")] public static string[] IncludeFileExtensions { get; set; }

        [Argument('p', "preview")] public static bool Preview { get; set; }

        [Argument('s', "dupes-dir")] public static string DupesDirectory { get; set; }

        [Argument('k', "strategy")] public static bool Clean { get; set; }

        [Argument('c', "clean")] public static string Strategy { get; set; }

        public static void Populate(string commandLineOveride = "")
        {
            Arguments.Populate(typeof(CommandArguments), Environment.CommandLine);
        }
    }
}