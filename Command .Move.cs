using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{
    /// <summary>реализует команду Move</summary>
    class MoveCommand : Command
    {
        public override string Names => "mv, move";
        public override string ShortDescription => "moves file or folder";
        public override string DetailedDescription =>
            "  {0} <source> <target> - move file/folder to folder <target>\n";

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
            try
            {
                string message = null;
                if (Directory.Exists(source))
                    message = Utils.CopyDirectory(source, target, true);
                else
                    File.Move(source, target);

                FileList.Reload();
                return message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
