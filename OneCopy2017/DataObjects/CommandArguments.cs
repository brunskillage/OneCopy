using System;
using OneCopy2017.ThirdParty;

namespace OneCopy2017.DataObjects
{
    public static class CommandArguments
    {
        [Argument('d', "dir")]
        public static string[] Directories { get; set; }

        [Argument('p', "preview")]
        public static bool Preview { get; set; }

        [Argument('s', "dupes-dir")]
        public static string DupesDirectory { get; set; }

        [Argument('k', "strategy")]
        public static string Strategy { get; set; }

        public static void Populate()
        {
            Arguments.Populate(typeof(CommandArguments), Environment.CommandLine);

        }
    }
}