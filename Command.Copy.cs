using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Copy</summary>
    class CopyCommand : Command
    {
        public override string Names => "cp, copy";
        public override string ShortDescription => "copies file or folder";
        public override string DetailedDescription =>
            "  {0} <source> <target> - copy file/folder to folder <target>\n";

        public override string Execute(List<string> args)
        {
            if (args.Count < 2)
                return "params expected";

            string source, target;

            if (args.Count == 2)
            {
                target = args[1];

                source = FileList.SelectedPath;
                if (source == null)
                    return "selection expected";
            }
            else
            {
                source = Path.GetFullPath(args[1]);
                target = args[2];
            }

            string dir = Path.GetDirectoryName(source);
            target = Path.Combine(dir, target);

            string message = null;
            try
            {
                if (Directory.Exists(source))
                    message = Utils.CopyDirectory(source, target);
                else
                    File.Copy(source, target);

                FileList.Reload();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }
    }
}
