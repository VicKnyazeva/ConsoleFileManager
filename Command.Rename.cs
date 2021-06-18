using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleFileManager
{

    /// <summary>реализует команду Rename</summary>
    class RenameCommand : Command
    {
        public override string Names => "ren, rename";
        public override string ShortDescription => "changes name of file or folder";

        public override string DetailedDescription => 
            "  {0} <new_name>                  - renames selected file or folder\n" +
            "  {0} <original_name> <new_name>  - renames selected file or folder";

        public override string Execute(List<string> args)
        {
            args.RemoveAt(0); // remove command name
            if (args.Count < 1)
                return "params expected";

            string source;
            string target;
            if (args.Count < 2)
            {
                source = FileList.SelectedPath;
                if (source == null)
                    return "There is no selected file or directory";

                target = args[0];
            }
            else
            {
                source = args[0];
                target = args[1];
            }

            string src = Path.GetFullPath(source);
            string dir = Path.GetDirectoryName(src);
            string tgt = Path.Combine(dir, target);
            try
            {
                if (Directory.Exists(src))
                {
                    Directory.Move(src, tgt);
                }
                else if (File.Exists(src))
                {
                    File.Move(src, tgt);
                }
                FileList.Reload();
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
