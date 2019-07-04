# OneCopy
Find all the duplicates

usage:
Onecopy2017 [Arguments]

 - [Argument('d', "dir")] The directory to scan
 - [Argument('e', "exclude-dir")]  directories to exclude spearated with |  eg  .node_modules|bin|other
 - [Argument('x', "ext")] extesions to include separated by |  eg jpg|mov|png
 - [Argument('p', "preview")] take no actions to move duplicates out
 - [Argument('s', "dupes-dir")]  the directory to move duplicated files into maintaining structure
 - [Argument('k', "strategy")] the strategy to determine which binary exact files to keep based on any available file date. Use either oldest or newest.

eg Oncopy2017 -d c:\photos -e edits|notsure -x jpg|png|mov|jpeg -p -s c:\dupes -k oldest

This will scan c:\photos excluding edits and notsure directories for jpg, png, mov, jpeg files not moving anything detected as oldest duplicate becuase of preview switch. Removing preview switch would move files out.

If you decide to use this it is at your own risk. I have used it on my local photos successfully. Always take a backup preserving file metadata!

To build:
 - Clone repo
 - Open in visual studio
 - Build in release mode
 - Use binary in release folder
